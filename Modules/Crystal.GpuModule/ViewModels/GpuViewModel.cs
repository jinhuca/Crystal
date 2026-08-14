using Crystal.Controls.Threading;
using Crystal.GpuModule.Models;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Service.Gpu;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Crystal.GpuModule.ViewModels;

public sealed class GpuViewModel : BindableBase, IGpuViewModel, IDisposable {
  private readonly IDisposable _specsSubscription;
  private readonly IDisposable _sensorsSubscription;
  private readonly UiThreadMarshaller _ui = new();

  public GpuViewModel(IGpuModel model, IEventAggregator events) {
    ShowDetailCommand = new DelegateCommand(
        () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Gpu));
    ShowDashboardCommand = new DelegateCommand(
        () => events.GetEvent<ShowDashboardEvent>().Publish());

    _specsSubscription = model.Specs.Subscribe(s => OnUi(() => ApplySpecs(s)));
    _sensorsSubscription = model.Sensors.Subscribe(s => OnUi(() => ApplyLoads(s)));
  }

  public ObservableCollection<GpuAdapterViewModel> Adapters { get; } = [];
  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  private void ApplySpecs(GpuSnapshot snapshot) {
    // Rebuild the adapter list on the (rare) spec emission. Integrated first so it lands in
    // the left column, matching the reference design.
    Adapters.Clear();
    foreach (var info in snapshot.Adapters.OrderBy(a => a.Kind)) {
      var vm = new GpuAdapterViewModel();
      vm.UpdateSpecs(info);
      Adapters.Add(vm);
    }
    ApplyLoads(snapshot);
  }

  private void ApplyLoads(GpuSnapshot snapshot) {
    foreach (var adapter in Adapters) {
      var reading = snapshot.Loads.FirstOrDefault(l =>
          string.Equals(l.AdapterName, adapter.Name, StringComparison.OrdinalIgnoreCase));
      if (reading is not null)
        adapter.UpdateLoad(reading);
    }
  }

  private void OnUi(Action action) => _ui.Post(action);

  public void Dispose() {
    _specsSubscription.Dispose();
    _sensorsSubscription.Dispose();
  }
}
