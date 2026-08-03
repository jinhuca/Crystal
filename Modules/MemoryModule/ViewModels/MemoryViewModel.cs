using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Infrastructure.Constants.Navigation;
using MemoryModule.Models;

namespace MemoryModule.ViewModels;

public sealed class MemoryViewModel : BindableBase, IMemoryViewModel, IDisposable {
  private readonly IDisposable _specsSubscription;
  private readonly IDisposable _loadSubscription;
  private string _totalCapacityLabel = "—";
  private string _slotsLabel = "—";
  private string _maxSpeedLabel = "—";
  private double _load;
  private PerformanceGraph? _loadGraph;

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
  public ObservableCollection<MemoryModuleViewModel> Modules { get; } = [];

  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  public void AttachGraph(PerformanceGraph graph) => _loadGraph = graph;

  private void ApplySpecs(MemorySnapshot snapshot) {
    TotalCapacityLabel = snapshot.TotalCapacityGB is { } gb ? $"{gb:0.#} GB" : "—";
    SlotsLabel = $"{snapshot.PopulatedSlots} populated";
    MaxSpeedLabel = snapshot.MaxSpeedMHz is { } s ? $"{s} MHz" : "—";

    Modules.Clear();
    foreach (var module in snapshot.Modules)
      Modules.Add(new MemoryModuleViewModel(module));
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
