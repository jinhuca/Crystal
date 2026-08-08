using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Infrastructure.Constants.Navigation;
using StorageModule.Models;

namespace StorageModule.ViewModels;

public sealed class StorageViewModel : BindableBase, IStorageViewModel, IDisposable {
  // Transfer rate has no natural ceiling (an NVMe drive bursts past 3 GB/s, an HDD idles near
  // zero), so the graph's Y-axis tracks the rolling peak over the visible window instead of a fixed
  // max. Keep the same number of samples the graph plots, floor the axis so an idle disk doesn't
  // zoom into noise, and round the peak up to a "nice" value for readable gridlines.
  private const int TransferWindow = 60;
  private const double TransferFloorMBps = 100;

  private readonly Queue<double> _transferSamples = new(TransferWindow + 1);
  private readonly IDisposable _specsSubscription;
  private readonly IDisposable _loadSubscription;
  private string _totalCapacityLabel = "—";
  private string _driveCountLabel = "—";
  private double _load;
  private double _transferRateMBps;
  private double _transferMaxMBps = TransferFloorMBps;
  private PerformanceGraph? _loadGraph;
  private PerformanceGraph? _transferGraph;

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
  public double TransferMaxMBps { get => _transferMaxMBps; private set => SetProperty(ref _transferMaxMBps, value); }

  /// <summary>Every physical drive — bound by the detail view.</summary>
  public ObservableCollection<StorageDriveViewModel> Drives { get; } = [];

  /// <summary>The first two fixed hard drives — bound by the compact dashboard tile so it stays
  /// dense; the detail view shows the rest.</summary>
  public ObservableCollection<StorageDriveViewModel> SummaryDrives { get; } = [];

  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  public void AttachGraph(PerformanceGraph graph) => _loadGraph = graph;
  public void AttachTransferGraph(PerformanceGraph graph) => _transferGraph = graph;

  private void ApplySpecs(StorageSnapshot snapshot) {
    TotalCapacityLabel = snapshot.TotalCapacityGB is { } gb ? $"{gb:0.#} GB" : "—";
    DriveCountLabel = snapshot.DriveCount == 1 ? "1 drive" : $"{snapshot.DriveCount} drives";

    Drives.Clear();
    foreach (var drive in snapshot.Drives)
      Drives.Add(new StorageDriveViewModel(drive));

    // The tile lists only the first two fixed hard drives to stay compact; everything else
    // (removable/external media, or additional fixed disks) lives in the detail view.
    SummaryDrives.Clear();
    foreach (var drive in Drives.Where(d => d.IsFixedHardDrive).Take(2))
      SummaryDrives.Add(drive);
  }

  private void ApplyLoad(StorageLoadReading reading) {
    Load = reading.ActivityPercent;
    _loadGraph?.AddValue(reading.ActivityPercent);

    TransferRateMBps = reading.TransferRateMBps;
    _transferGraph?.AddValue(reading.TransferRateMBps);

    _transferSamples.Enqueue(reading.TransferRateMBps);
    while (_transferSamples.Count > TransferWindow) _transferSamples.Dequeue();
    TransferMaxMBps = NiceCeiling(Math.Max(TransferFloorMBps, _transferSamples.Max()));
  }

  // Round a peak up to a readable axis top: 1/2/5 × a power of ten (100, 200, 500, 1000, …), so the
  // gridlines land on round transfer rates as the window's busiest sample grows or subsides.
  private static double NiceCeiling(double value) {
    double magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
    double normalized = value / magnitude;
    double step = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
    return step * magnitude;
  }

  private static void OnUi(Action action) {
    var dispatcher = Application.Current?.Dispatcher;
    if (dispatcher is null || dispatcher.CheckAccess()) action();
    else dispatcher.Invoke(action);
  }

  public void Dispose() {
    _specsSubscription.Dispose();
    _loadSubscription.Dispose();
  }
}
