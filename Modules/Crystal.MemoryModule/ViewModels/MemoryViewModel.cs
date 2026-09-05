using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.Threading;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.MemoryModule.Models;
using Crystal.Service.Memory;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Crystal.MemoryModule.ViewModels;

public sealed class MemoryViewModel : BindableBase, IMemoryViewModel, IDisposable {
  private readonly IDisposable _specsSubscription;
  private readonly IDisposable _loadSubscription;
  private readonly UiThreadMarshaller _ui = new();
  private string _totalCapacityLabel = "—";
  private string _slotsLabel = "—";
  private string _maxSpeedLabel = "—";
  private double _load;
  private double? _usedGB;
  private double? _totalCapacityGB;
  // Summary-tile history graphs, registered by their GraphIdentity.Id as each metric sub-view
  // loads, then fed by that same id in ApplyLoad. The detail view's usage/commit graphs use a
  // different wrapper control with no id and stay on their own attach methods below.
  private readonly Dictionary<string, ISingleSeriesGraph> _graphs = [];
  private ISingleSeriesGraph? _usageGraph;
  private ISingleSeriesGraph? _commitGraph;

  // Task Manager-style header + stats grid.
  private string _headerSpecLabel = "—";
  private string _usageLabel = "—";
  private string _inUseLabel = "—";
  private string _availableLabel = "—";
  private string _committedLabel = "—";
  private string _commitUsageLabel = "—";
  private string _commitPeakLabel = "—";
  private string _cachedLabel = "—";
  private string _pagedPoolLabel = "—";
  private string _nonPagedPoolLabel = "—";
  private string _pageFileLabel = "—";
  private string _pageFilePeakLabel = "—";
  private string _speedLabel = "—";
  private string _slotsUsedLabel = "—";
  private string _formFactorLabel = "—";
  private string _hardwareReservedLabel = "—";
  private double _compositionInUseFraction;
  private double _compositionModifiedFraction;
  private double _compositionStandbyFraction;
  private double _compositionFreeFraction;
  private double _compositionRemainderFraction;
  private string _compositionInUseLabel = "—";
  private string _compositionModifiedLabel = "—";
  private string _compositionStandbyLabel = "—";
  private string _compositionFreeLabel = "—";
  private double? _compositionTotalGB;
  private double? _commitLimitGB;
  private string? _memoryType;

  public MemoryViewModel(IMemoryModel model, IEventAggregator events) {
    ShowDetailCommand = new DelegateCommand(
        () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Memory));
    ShowDashboardCommand = new DelegateCommand(
        () => events.GetEvent<ShowDashboardEvent>().Publish());

    _specsSubscription = model.Specs.Subscribe(s => OnUi(() => ApplySpecs(s)));
    _loadSubscription = model.Load.Subscribe(v => OnUi(() => ApplyLoad(v)));
  }

  public string TotalCapacityLabel { get => _totalCapacityLabel; private set => SetProperty(ref _totalCapacityLabel, value); }
  public string SlotsLabel { get => _slotsLabel; private set => SetProperty(ref _slotsLabel, value); }
  public string MaxSpeedLabel { get => _maxSpeedLabel; private set => SetProperty(ref _maxSpeedLabel, value); }
  public double Load { get => _load; private set => SetProperty(ref _load, value); }
  public double? UsedGB { get => _usedGB; private set => SetProperty(ref _usedGB, value); }
  public double? TotalCapacityGB { get => _totalCapacityGB; private set => SetProperty(ref _totalCapacityGB, value); }

  public string HeaderSpecLabel { get => _headerSpecLabel; private set => SetProperty(ref _headerSpecLabel, value); }
  public string UsageLabel { get => _usageLabel; private set => SetProperty(ref _usageLabel, value); }
  public string InUseLabel { get => _inUseLabel; private set => SetProperty(ref _inUseLabel, value); }
  public string AvailableLabel { get => _availableLabel; private set => SetProperty(ref _availableLabel, value); }
  public string CommittedLabel { get => _committedLabel; private set => SetProperty(ref _committedLabel, value); }
  /// <summary>Committed charge with its share of the commit limit as a percent — shown on the commit graph caption.</summary>
  public string CommitUsageLabel { get => _commitUsageLabel; private set => SetProperty(ref _commitUsageLabel, value); }
  public string CommitPeakLabel { get => _commitPeakLabel; private set => SetProperty(ref _commitPeakLabel, value); }
  public string CachedLabel { get => _cachedLabel; private set => SetProperty(ref _cachedLabel, value); }
  public string PagedPoolLabel { get => _pagedPoolLabel; private set => SetProperty(ref _pagedPoolLabel, value); }
  public string NonPagedPoolLabel { get => _nonPagedPoolLabel; private set => SetProperty(ref _nonPagedPoolLabel, value); }
  public string PageFileLabel { get => _pageFileLabel; private set => SetProperty(ref _pageFileLabel, value); }
  public string PageFilePeakLabel { get => _pageFilePeakLabel; private set => SetProperty(ref _pageFilePeakLabel, value); }
  public string SpeedLabel { get => _speedLabel; private set => SetProperty(ref _speedLabel, value); }
  public string SlotsUsedLabel { get => _slotsUsedLabel; private set => SetProperty(ref _slotsUsedLabel, value); }
  public string FormFactorLabel { get => _formFactorLabel; private set => SetProperty(ref _formFactorLabel, value); }
  public string HardwareReservedLabel { get => _hardwareReservedLabel; private set => SetProperty(ref _hardwareReservedLabel, value); }
  public double CompositionInUseFraction { get => _compositionInUseFraction; private set => SetProperty(ref _compositionInUseFraction, value); }
  public double CompositionModifiedFraction { get => _compositionModifiedFraction; private set => SetProperty(ref _compositionModifiedFraction, value); }
  public double CompositionStandbyFraction { get => _compositionStandbyFraction; private set => SetProperty(ref _compositionStandbyFraction, value); }
  public double CompositionFreeFraction { get => _compositionFreeFraction; private set => SetProperty(ref _compositionFreeFraction, value); }
  /// <summary>Empty-track remainder so the segment fractions always sum to 1 (star columns need this).</summary>
  public double CompositionRemainderFraction { get => _compositionRemainderFraction; private set => SetProperty(ref _compositionRemainderFraction, value); }
  public string CompositionInUseLabel { get => _compositionInUseLabel; private set => SetProperty(ref _compositionInUseLabel, value); }
  public string CompositionModifiedLabel { get => _compositionModifiedLabel; private set => SetProperty(ref _compositionModifiedLabel, value); }
  public string CompositionStandbyLabel { get => _compositionStandbyLabel; private set => SetProperty(ref _compositionStandbyLabel, value); }
  public string CompositionFreeLabel { get => _compositionFreeLabel; private set => SetProperty(ref _compositionFreeLabel, value); }
  public double? CompositionTotalGB { get => _compositionTotalGB; private set => SetProperty(ref _compositionTotalGB, value); }
  public double? CommitLimitGB { get => _commitLimitGB; private set => SetProperty(ref _commitLimitGB, value); }

  /// <summary>Every populated slot — bound by the detail view.</summary>
  public ObservableCollection<MemoryModuleViewModel> Modules { get; } = [];

  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  public void AttachGraph(string id, ISingleSeriesGraph graph) => _graphs[id] = graph;
  public void AttachUsageGraph(ISingleSeriesGraph graph) => _usageGraph = graph;
  public void AttachCommitGraph(ISingleSeriesGraph graph) => _commitGraph = graph;

  private void FeedGraph(string id, double value) {
    if (_graphs.TryGetValue(id, out var graph)) graph.AddValue(value);
  }

  private void ApplySpecs(MemorySnapshot snapshot) {
    TotalCapacityLabel = snapshot.TotalCapacityGB is { } gb ? $"{gb:0.#} GB" : "—";
    TotalCapacityGB = snapshot.TotalCapacityGB;
    CompositionTotalGB = snapshot.TotalCapacityGB;
    SlotsLabel = $"{snapshot.PopulatedSlots} populated";
    MaxSpeedLabel = snapshot.MaxSpeedMHz is { } s ? $"{s} MHz" : "—";
    _memoryType = snapshot.MemoryType;

    // Header: "32.0 GB DDR5" (type omitted when unknown).
    HeaderSpecLabel = snapshot.TotalCapacityGB is { } total
        ? (snapshot.MemoryType is { } type ? $"{total:0.#} GB {type}" : $"{total:0.#} GB")
        : "—";
    SpeedLabel = snapshot.MaxSpeedMHz is { } speed ? $"{speed} MT/s" : "—";
    SlotsUsedLabel = snapshot.TotalSlots is { } slots
        ? $"{snapshot.PopulatedSlots} of {slots}"
        : $"{snapshot.PopulatedSlots}";
    FormFactorLabel = snapshot.FormFactor ?? "—";

    Modules.Clear();
    foreach (var module in snapshot.Modules)
      Modules.Add(new MemoryModuleViewModel(module));
  }

  private void ApplyLoad(MemoryLoadReading reading) {
    Load = reading.LoadPercent;
    FeedGraph("Memory.Utilization", reading.LoadPercent);
    // The tile's usage graph is scaled 0–100, so plot the utilization percent (not the GB figure,
    // which the graph's MaxValue would have to track against a specs total the sensor can meet).
    _usageGraph?.AddValue(reading.LoadPercent);

    UsedGB = reading.UsedGB;
    if (reading.UsedGB is { } used) {
      FeedGraph("Memory.Used", used);
      UsageLabel = $"{used:0.#} GB";
      InUseLabel = $"{used:0.#} GB";
    }
    ApplyComposition(reading);

    AvailableLabel = Gb(reading.AvailableGB);
    CommitLimitGB = reading.CommitLimitGB;
    if (reading.CommittedGB is { } committed)
      _commitGraph?.AddValue(committed);
    CommittedLabel = reading is { CommittedGB: { } c, CommitLimitGB: { } limit }
        ? $"{c:0.#}/{limit:0.#} GB"
        : Gb(reading.CommittedGB);
    CommitUsageLabel = reading is { CommittedGB: { } cu, CommitLimitGB: { } cl and > 0 }
        ? $"{cu:0.#}/{cl:0.#} GB · {cu / cl * 100:0}%"
        : CommittedLabel;
    CommitPeakLabel = Gb(reading.CommitPeakGB);
    CachedLabel = Gb(reading.CachedGB);
    PagedPoolLabel = Gb(reading.PagedPoolGB);
    NonPagedPoolLabel = Gb(reading.NonPagedPoolGB);
    PageFileLabel = reading is { PageFileUsedGB: { } pfUsed, PageFileTotalGB: { } pfTotal }
        ? $"{pfUsed:0.#}/{pfTotal:0.#} GB"
        : Gb(reading.PageFileUsedGB);
    PageFilePeakLabel = Gb(reading.PageFilePeakGB);
    HardwareReservedLabel = reading.HardwareReservedGB is { } hw
        ? (hw >= 1 ? $"{hw:0.#} GB" : $"{hw * 1024:0} MB")
        : "—";
  }

  // Prefer Task Manager's four-segment breakdown (In use / Modified / Standby / Free) when the
  // page-list perf counters are available; otherwise fall back to a single in-use segment sized by
  // used/capacity. A trailing remainder keeps the fractions summing to 1 for the star-column bar.
  private void ApplyComposition(MemoryLoadReading reading) {
    if (reading is { PhysicalTotalGB: { } total and > 0, ModifiedGB: { } modified,
                     StandbyGB: { } standby, FreeGB: { } free }) {
      double inUse = Math.Max(0, total - modified - standby - free);
      CompositionInUseFraction = Frac(inUse, total);
      CompositionModifiedFraction = Frac(modified, total);
      CompositionStandbyFraction = Frac(standby, total);
      CompositionFreeFraction = Frac(free, total);
      CompositionInUseLabel = Gb(inUse);
      CompositionModifiedLabel = Gb(modified);
      CompositionStandbyLabel = Gb(standby);
      CompositionFreeLabel = Gb(free);
    } else {
      double inUse = reading.UsedGB is { } used && _totalCapacityGB is { } cap and > 0
          ? Math.Clamp(used / cap, 0, 1) : 0;
      CompositionInUseFraction = inUse;
      CompositionModifiedFraction = 0;
      CompositionStandbyFraction = 0;
      CompositionFreeFraction = 0;
      CompositionInUseLabel = Gb(reading.UsedGB);
      CompositionModifiedLabel = "—";
      CompositionStandbyLabel = "—";
      CompositionFreeLabel = "—";
    }
    CompositionRemainderFraction = Math.Max(0,
        1 - CompositionInUseFraction - CompositionModifiedFraction
          - CompositionStandbyFraction - CompositionFreeFraction);
  }

  private static double Frac(double value, double total) => Math.Clamp(value / total, 0, 1);

  private static string Gb(double? value) => value is { } v ? $"{v:0.#} GB" : "—";

  private void OnUi(Action action) => _ui.Post(action);

  public void Dispose() {
    _specsSubscription.Dispose();
    _loadSubscription.Dispose();
  }
}
