using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.Threading;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Service.Storage;
using Crystal.StorageModule.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Crystal.StorageModule.ViewModels;

public sealed class StorageViewModel : BindableBase, IStorageViewModel, IDisposable {
  // Transfer rate has no natural ceiling (an NVMe drive bursts past 3 GB/s, an HDD idles near
  // zero), so the tile graph's Y-axis tracks the rolling peak over the visible window instead of a
  // fixed max. Keep the same number of samples the graph plots, floor the axis so an idle disk
  // doesn't zoom into noise, and round the peak up to a "nice" value for readable gridlines.
  private const int TransferWindow = 60;
  private const double TransferFloorMBps = 100;

  private readonly Queue<double> _transferSamples = new(TransferWindow + 1);
  private readonly IDisposable _specsSubscription;
  private readonly IDisposable _loadSubscription;
  private readonly UiThreadMarshaller _ui = new();
  private string _totalCapacityLabel = "—";
  private string _driveCountLabel = "—";
  private double _load;
  private double _transferRateMBps;
  private double _readRateMBps;
  private double _writeRateMBps;
  private double _transferMaxMBps = TransferFloorMBps;
  private double _peakTransferMBps;
  private double? _freeSpaceGB;
  private double? _totalSpaceGB;
  private int _driveCount;
  private int? _busiestDriveIndex;
  private StorageDriveViewModel? _selectedDisk;

  // History graphs are registered by their GraphIdentity.Id as each metric sub-view loads, then
  // fed by that same id in ApplyLoad. A consumer that realizes only some tiles feeds only those.
  private readonly Dictionary<string, PerformanceGraph> _graphs = [];

  public StorageViewModel(IStorageModel model, IEventAggregator events) {
    ShowDetailCommand = new DelegateCommand(
        () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Storage));
    ShowDashboardCommand = new DelegateCommand(
        () => events.GetEvent<ShowDashboardEvent>().Publish());

    _specsSubscription = model.Specs.Subscribe(s => OnUi(() => ApplySpecs(s)));
    _loadSubscription = model.Load.Subscribe(v => OnUi(() => ApplyLoad(v)));
  }

  public string TotalCapacityLabel { get => _totalCapacityLabel; private set => SetProperty(ref _totalCapacityLabel, value); }
  public string DriveCountLabel { get => _driveCountLabel; private set => SetProperty(ref _driveCountLabel, value); }
  public double Load { get => _load; private set => SetProperty(ref _load, value); }
  public double TransferRateMBps { get => _transferRateMBps; private set => SetProperty(ref _transferRateMBps, value); }
  public double ReadRateMBps { get => _readRateMBps; private set => SetProperty(ref _readRateMBps, value); }
  public double WriteRateMBps { get => _writeRateMBps; private set => SetProperty(ref _writeRateMBps, value); }
  public double TransferMaxMBps { get => _transferMaxMBps; private set => SetProperty(ref _transferMaxMBps, value); }

  // Highest combined system-wide transfer rate seen this session; GB/s once it crosses 1000 MB/s.
  public string PeakTransferLabel =>
      _peakTransferMBps >= 1000 ? $"{_peakTransferMBps / 1000:0.0} GB/s" : $"{_peakTransferMBps:0.0} MB/s";

  // Filesystem capacity roll-up across every disk with mounted volumes: used vs free of the summed
  // total. Fractions sum to 1 so the two star columns split the bar exactly. All null (bar hidden)
  // when no disk reports space — e.g. a set of unformatted drives.
  private double? UsedSpaceGB =>
      _totalSpaceGB is { } total && _freeSpaceGB is { } free ? Math.Max(0, total - free) : null;

  public bool HasCapacityData => _totalSpaceGB is { } total and > 0;
  public double UsedSpaceFraction =>
      _totalSpaceGB is { } total and > 0 && _freeSpaceGB is { } free
          ? Math.Clamp((total - free) / total, 0, 1) : 0;
  public double FreeSpaceFraction => Math.Max(0, 1 - UsedSpaceFraction);
  public string CapacityUsageLabel =>
      UsedSpaceGB is { } used && _totalSpaceGB is { } total ? $"{used:0.#} / {total:0.#} GB" : "—";
  public string UsedSpacePercentLabel => HasCapacityData ? $"{UsedSpaceFraction * 100:0}%" : "—";
  public string FreeSpacePercentLabel => HasCapacityData ? $"{FreeSpaceFraction * 100:0}% free" : "—";

  // Header roll-up: total, free, and available% all come from the same summed filesystem figures so
  // they agree with each other (the WMI TotalCapacityLabel is physical disk size and can differ).
  public string TotalSpaceLabel => _totalSpaceGB is { } gb ? $"{gb:0} GB" : "—";
  public string FreeSpaceLabel => _freeSpaceGB is { } gb ? $"{gb:0} GB" : "—";
  public string AvailablePercentLabel => HasCapacityData ? $"{FreeSpaceFraction * 100:0}%" : "—";

  // Drive count split into value + noun so the header can style the number and the word separately.
  public string DriveCountValue => _driveCount.ToString();
  public string DriveNoun => _driveCount == 1 ? "drive" : "drives";

  // Which physical disk is driving the aggregate active-time figure right now. Only meaningful — and
  // only shown — when there's more than one disk to disambiguate.
  public bool ShowBusiestDrive => Drives.Count > 1;
  public string BusiestDriveLabel => _busiestDriveIndex is { } i ? $"Disk {i} busiest" : "—";

  /// <summary>Every physical disk — the detail view's selector list.</summary>
  public ObservableCollection<StorageDriveViewModel> Drives { get; } = [];

  /// <summary>The disk whose graphs and stats the detail view currently shows.</summary>
  public StorageDriveViewModel? SelectedDisk { get => _selectedDisk; set => SetProperty(ref _selectedDisk, value); }

  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  public void AttachGraph(string id, PerformanceGraph graph) => _graphs[id] = graph;

  private void FeedGraph(string id, double value) {
    if (_graphs.TryGetValue(id, out var graph)) graph.AddValue(value);
  }

  private void ApplySpecs(StorageSnapshot snapshot) {
    TotalCapacityLabel = snapshot.TotalCapacityGB is { } gb ? $"{gb:0.#} GB" : "—";
    _driveCount = snapshot.DriveCount;
    DriveCountLabel = snapshot.DriveCount == 1 ? "1 drive" : $"{snapshot.DriveCount} drives";
    RaisePropertyChanged(nameof(DriveCountValue));
    RaisePropertyChanged(nameof(DriveNoun));

    // Merge rather than rebuild so drives that survive a hotplug keep their live state and graph
    // history: drop rows whose disk is gone, add rows for newly attached disks, leave the rest.
    // Joined by physical-disk index, the same key ApplyLoad routes readings by.
    var incomingIndices = snapshot.Drives.Select(d => d.DriveIndex).ToHashSet();
    for (int i = Drives.Count - 1; i >= 0; i--)
      if (!incomingIndices.Contains(Drives[i].DriveIndex))
        Drives.RemoveAt(i);
    foreach (var drive in snapshot.Drives)
      if (Drives.All(d => d.DriveIndex != drive.DriveIndex))
        Drives.Add(new StorageDriveViewModel(drive));

    if (SelectedDisk is null || !Drives.Contains(SelectedDisk))
      SelectedDisk = Drives.FirstOrDefault();
    RaisePropertyChanged(nameof(ShowBusiestDrive));
  }

  private void ApplyLoad(StorageLoadReading reading) {
    // Route each disk's sample to its matching per-disk VM (joined by physical-disk index).
    double maxActivity = -1;
    int? busiestIndex = null;
    double totalRead = 0;
    double totalWrite = 0;
    double aggFree = 0;
    double aggTotal = 0;
    var anySpace = false;
    foreach (var disk in reading.Disks) {
      var vm = Drives.FirstOrDefault(d => d.DriveIndex == disk.DriveIndex);
      vm?.Update(disk);

      if (disk.ActivityPercent > maxActivity) {
        maxActivity = disk.ActivityPercent;
        busiestIndex = disk.DriveIndex;
      }
      totalRead += disk.ReadRateMBps;
      totalWrite += disk.WriteRateMBps;

      if (disk.TotalSpaceGB is { } total and > 0) {
        aggTotal += total;
        aggFree += disk.FreeSpaceGB ?? 0;
        anySpace = true;
      }
    }
    double totalTransfer = totalRead + totalWrite;
    double busiestActivity = Math.Max(0, maxActivity); // -1 only when there were no disks at all.

    // The dashboard tile stays aggregate: busiest disk's activity + system-wide transfer rate,
    // split into read vs write so the tile matches HWiNFO's separate R/W figures.
    Load = busiestActivity;
    FeedGraph("Storage.Activity", busiestActivity);
    _busiestDriveIndex = busiestIndex;
    RaisePropertyChanged(nameof(BusiestDriveLabel));

    ReadRateMBps = totalRead;
    WriteRateMBps = totalWrite;
    TransferRateMBps = totalTransfer;
    FeedGraph("Storage.Transfer", totalTransfer);
    _transferSamples.Enqueue(totalTransfer);
    while (_transferSamples.Count > TransferWindow) _transferSamples.Dequeue();
    TransferMaxMBps = NiceCeiling(Math.Max(TransferFloorMBps, _transferSamples.Max()));
    _peakTransferMBps = Math.Max(_peakTransferMBps, totalTransfer);
    RaisePropertyChanged(nameof(PeakTransferLabel));

    _totalSpaceGB = anySpace ? aggTotal : null;
    _freeSpaceGB = anySpace ? aggFree : null;
    RaisePropertyChanged(nameof(HasCapacityData));
    RaisePropertyChanged(nameof(UsedSpaceFraction));
    RaisePropertyChanged(nameof(FreeSpaceFraction));
    RaisePropertyChanged(nameof(CapacityUsageLabel));
    RaisePropertyChanged(nameof(UsedSpacePercentLabel));
    RaisePropertyChanged(nameof(FreeSpacePercentLabel));
    RaisePropertyChanged(nameof(TotalSpaceLabel));
    RaisePropertyChanged(nameof(FreeSpaceLabel));
    RaisePropertyChanged(nameof(AvailablePercentLabel));
  }

  // Round a peak up to a readable axis top: 1/2/5 × a power of ten (100, 200, 500, 1000, …), so the
  // gridlines land on round transfer rates as the window's busiest sample grows or subsides.
  private static double NiceCeiling(double value) {
    double magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
    double normalized = value / magnitude;
    double step = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
    return step * magnitude;
  }

  private void OnUi(Action action) => _ui.Post(action);

  public void Dispose() {
    _specsSubscription.Dispose();
    _loadSubscription.Dispose();
  }
}
