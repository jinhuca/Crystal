using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Infrastructure.Constants.Navigation;
using NetworkModule.Models;

namespace NetworkModule.ViewModels;

public sealed class NetworkViewModel : BindableBase, INetworkViewModel, IDisposable {
  private readonly IDisposable _sensorsSubscription;
  private double _load;
  private PerformanceGraph? _loadGraph;

  public NetworkViewModel(INetworkModel model, IEventAggregator events) {
    ShowDetailCommand = new DelegateCommand(
        () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Network));
    ShowDashboardCommand = new DelegateCommand(
        () => events.GetEvent<ShowDashboardEvent>().Publish());

    _sensorsSubscription = model.Sensors.Subscribe(s => OnUi(() => Apply(s)));
  }

  public ObservableCollection<NetworkAdapterViewModel> Adapters { get; } = [];
  public double Load { get => _load; private set => SetProperty(ref _load, value); }
  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  public void AttachGraph(PerformanceGraph graph) => _loadGraph = graph;

  private void Apply(NetworkSnapshot snapshot) {
    // Reconcile the adapter list against the current interfaces (they can come and go as NICs
    // connect/disconnect), keyed by name.
    SyncAdapters(snapshot.Interfaces);

    var overall = 0.0;
    foreach (var reading in snapshot.Interfaces) {
      var adapter = Adapters.FirstOrDefault(a =>
          string.Equals(a.Name, reading.Name, StringComparison.OrdinalIgnoreCase));
      adapter?.Update(reading);
      overall = Math.Max(overall, reading.UtilizationPercent);
    }

    Load = overall;
    _loadGraph?.AddValue(overall);
  }

  private void SyncAdapters(IReadOnlyList<NetworkInterfaceReading> interfaces) {
    for (var i = Adapters.Count - 1; i >= 0; i--) {
      if (!interfaces.Any(r => string.Equals(r.Name, Adapters[i].Name, StringComparison.OrdinalIgnoreCase)))
        Adapters.RemoveAt(i);
    }
    foreach (var reading in interfaces) {
      if (!Adapters.Any(a => string.Equals(a.Name, reading.Name, StringComparison.OrdinalIgnoreCase)))
        Adapters.Add(new NetworkAdapterViewModel());
    }
  }

  private static void OnUi(Action action) {
    var dispatcher = Application.Current?.Dispatcher;
    if (dispatcher is null || dispatcher.CheckAccess()) action();
    else dispatcher.Invoke(action);
  }

  public void Dispose() => _sensorsSubscription.Dispose();
}
