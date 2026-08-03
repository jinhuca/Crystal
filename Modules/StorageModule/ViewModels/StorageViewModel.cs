using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Infrastructure.Constants.Navigation;
using StorageModule.Models;

namespace StorageModule.ViewModels;

public sealed class StorageViewModel : BindableBase, IStorageViewModel, IDisposable {
  private readonly IDisposable _specsSubscription;
  private readonly IDisposable _loadSubscription;
  private string _totalCapacityLabel = "—";
  private string _driveCountLabel = "—";
  private double _load;
  private PerformanceGraph? _loadGraph;

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
  public ObservableCollection<StorageDriveViewModel> Drives { get; } = [];

  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  public void AttachGraph(PerformanceGraph graph) => _loadGraph = graph;

  private void ApplySpecs(StorageSnapshot snapshot) {
    TotalCapacityLabel = snapshot.TotalCapacityGB is { } gb ? $"{gb:0.#} GB" : "—";
    DriveCountLabel = snapshot.DriveCount == 1 ? "1 drive" : $"{snapshot.DriveCount} drives";

    Drives.Clear();
    foreach (var drive in snapshot.Drives)
      Drives.Add(new StorageDriveViewModel(drive));
  }

  private void ApplyLoad(double loadPercent) {
    Load = loadPercent;
    _loadGraph?.AddValue(loadPercent);
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
