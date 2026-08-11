using System.Collections.ObjectModel;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.Threading;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Service.Memory;
using MemoryModule.Models;

namespace MemoryModule.ViewModels;

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
  private PerformanceGraph? _loadGraph;
  private PerformanceGraph? _usedGraph;
  private PerformanceGraph? _usageGraph;

  // Task Manager-style header + stats grid.
  private string _headerSpecLabel = "—";
  private string _usageLabel = "—";
  private string _inUseLabel = "—";
  private string _availableLabel = "—";
  private string _committedLabel = "—";
  private string _cachedLabel = "—";
  private string _pagedPoolLabel = "—";
  private string _nonPagedPoolLabel = "—";
  private string _speedLabel = "—";
  private string _slotsUsedLabel = "—";
  private string _formFactorLabel = "—";
  private string _hardwareReservedLabel = "—";
  private double _compositionInUseFraction;
  private double? _compositionTotalGB;
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
  public string CachedLabel { get => _cachedLabel; private set => SetProperty(ref _cachedLabel, value); }
  public string PagedPoolLabel { get => _pagedPoolLabel; private set => SetProperty(ref _pagedPoolLabel, value); }
  public string NonPagedPoolLabel { get => _nonPagedPoolLabel; private set => SetProperty(ref _nonPagedPoolLabel, value); }
  public string SpeedLabel { get => _speedLabel; private set => SetProperty(ref _speedLabel, value); }
  public string SlotsUsedLabel { get => _slotsUsedLabel; private set => SetProperty(ref _slotsUsedLabel, value); }
  public string FormFactorLabel { get => _formFactorLabel; private set => SetProperty(ref _formFactorLabel, value); }
  public string HardwareReservedLabel { get => _hardwareReservedLabel; private set => SetProperty(ref _hardwareReservedLabel, value); }
  public double CompositionInUseFraction { get => _compositionInUseFraction; private set => SetProperty(ref _compositionInUseFraction, value); }
  public double? CompositionTotalGB { get => _compositionTotalGB; private set => SetProperty(ref _compositionTotalGB, value); }

  /// <summary>Every populated slot — bound by the detail view.</summary>
  public ObservableCollection<MemoryModuleViewModel> Modules { get; } = [];

  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  public void AttachGraph(PerformanceGraph graph) => _loadGraph = graph;
  public void AttachUsedGraph(PerformanceGraph graph) => _usedGraph = graph;
  public void AttachUsageGraph(PerformanceGraph graph) => _usageGraph = graph;

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
    _loadGraph?.AddValue(reading.LoadPercent);

    UsedGB = reading.UsedGB;
    if (reading.UsedGB is { } used) {
      _usedGraph?.AddValue(used);
      _usageGraph?.AddValue(used);
      UsageLabel = $"{used:0.#} GB";
      InUseLabel = $"{used:0.#} GB";
      if (_totalCapacityGB is { } cap and > 0)
        CompositionInUseFraction = Math.Clamp(used / cap, 0, 1);
    }

    AvailableLabel = Gb(reading.AvailableGB);
    CommittedLabel = reading is { CommittedGB: { } c, CommitLimitGB: { } limit }
        ? $"{c:0.#}/{limit:0.#} GB"
        : Gb(reading.CommittedGB);
    CachedLabel = Gb(reading.CachedGB);
    PagedPoolLabel = Gb(reading.PagedPoolGB);
    NonPagedPoolLabel = Gb(reading.NonPagedPoolGB);
    HardwareReservedLabel = reading.HardwareReservedGB is { } hw
        ? (hw >= 1 ? $"{hw:0.#} GB" : $"{hw * 1024:0} MB")
        : "—";
  }

  private static string Gb(double? value) => value is { } v ? $"{v:0.#} GB" : "—";

  private void OnUi(Action action) => _ui.Post(action);

  public void Dispose() {
    _specsSubscription.Dispose();
    _loadSubscription.Dispose();
  }
}
