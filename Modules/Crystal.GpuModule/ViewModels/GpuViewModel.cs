using Crystal.Controls.Threading;
using Crystal.GpuModule.Models;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Service.Gpu;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Crystal.GpuModule.ViewModels;

/// <summary>
/// Root view model bound to the GPU summary tile and detail view. Exposes one
/// <see cref="GpuAdapterViewModel"/> per detected adapter (integrated / dedicated columns of
/// the reference design) and the two navigation commands the shell wires to.
/// </summary>
public sealed class GpuViewModel : BindableBase, IGpuViewModel, IDisposable {
  /// <summary>
  /// The subscriptions for the GPU specs and sensors.
  /// </summary>
  private readonly IDisposable _specsSubscription;

  /// <summary>
  /// The subscriptions for the GPU specs and sensors.
  /// </summary>
  private readonly IDisposable _sensorsSubscription;

  /// <summary>
  /// Marshals actions to the UI thread.
  /// </summary>
  private readonly UiThreadMarshaller _ui = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="GpuViewModel"/> class.
  /// </summary>
  /// <param name="model">The GPU model.</param>
  /// <param name="events">The event aggregator.</param>
  public GpuViewModel(IGpuModel model, IEventAggregator events) {
    ShowDetailCommand = new DelegateCommand(() => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Gpu));
    ShowDashboardCommand = new DelegateCommand(() => events.GetEvent<ShowDashboardEvent>().Publish());

    _specsSubscription = model.Specs.Subscribe(s => OnUi(() => ApplySpecs(s)));
    _sensorsSubscription = model.Sensors.Subscribe(s => OnUi(() => ApplyLoads(s)));
  }

  /// <summary>
  /// Exposes one <see cref="GpuAdapterViewModel"/> per detected adapter (integrated / dedicated
  /// columns of the reference design).
  /// </summary>
  public ObservableCollection<GpuAdapterViewModel> Adapters { get; } = [];

  private GpuAdapterViewModel? _integratedAdapter;
  private GpuAdapterViewModel? _dedicatedAdapter;

  /// <summary>
  /// The integrated adapter, bound to the left block of the summary design (null when the machine
  /// has no integrated graphics, in which case the block is collapsed).
  /// </summary>
  public GpuAdapterViewModel? IntegratedAdapter {
    get => _integratedAdapter;
    private set => SetProperty(ref _integratedAdapter, value);
  }

  /// <summary>
  /// The dedicated adapter, bound to the right block of the summary design (null when the machine
  /// has no discrete card, in which case the block is collapsed).
  /// </summary>
  public GpuAdapterViewModel? DedicatedAdapter {
    get => _dedicatedAdapter;
    private set => SetProperty(ref _dedicatedAdapter, value);
  }

  /// <summary>
  /// Raises <c>ShowDetailEvent</c> so the shell swaps in the GPU detail view.
  /// </summary>
  public ICommand ShowDetailCommand { get; }

  /// <summary>
  /// Raises <c>ShowDashboardEvent</c> so the shell returns to the tile dashboard.
  /// </summary>
  public ICommand ShowDashboardCommand { get; }

  /// <summary>
  /// Applies the GPU specs to the view model, rebuilding the adapter list and updating the loads.
  /// </summary>
  /// <param name="snapshot"></param>
  private void ApplySpecs(GpuSnapshot snapshot) {
    // Rebuild the adapter list on the (rare) spec emission. Integrated first so it lands in
    // the left column, matching the reference design.
    Adapters.Clear();
    foreach (var info in snapshot.Adapters.OrderBy(a => a.Kind)) {
      var vm = new GpuAdapterViewModel();
      vm.UpdateSpecs(info);
      Adapters.Add(vm);
    }

    IntegratedAdapter = Adapters.FirstOrDefault(a => a.IsIntegrated);
    DedicatedAdapter = Adapters.FirstOrDefault(a => a.IsDedicated);

    ApplyLoads(snapshot);
  }

  /// <summary>
  /// Applies the GPU loads to the view model, updating the existing adapter list.
  /// </summary>
  /// <param name="snapshot">The GPU snapshot containing the load data.</param>
  private void ApplyLoads(GpuSnapshot snapshot) {
    foreach (var adapter in Adapters) {
      var reading = snapshot.Loads.FirstOrDefault(l =>
          string.Equals(l.AdapterName, adapter.Name, StringComparison.OrdinalIgnoreCase));
      if (reading is not null) {
        adapter.UpdateLoad(reading);
      }
    }
  }

  /// <summary>
  /// Marshals the specified action to the UI thread.
  /// </summary>
  /// <param name="action">The action to marshal to the UI thread.</param>
  private void OnUi(Action action) => _ui.Post(action);

  /// <summary>
  /// Disposes the GPU view model, unsubscribing from the specs and sensors streams.
  /// </summary>
  public void Dispose() {
    _specsSubscription.Dispose();
    _sensorsSubscription.Dispose();
  }
}
