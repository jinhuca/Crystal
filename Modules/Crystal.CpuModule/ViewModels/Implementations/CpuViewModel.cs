using Crystal.Controls.Threading;
using Crystal.CpuModule.Models;
using Crystal.CpuModule.ViewModels.Interfaces;
using Crystal.Infrastructure.Constants.Navigation;
using System.Windows.Input;

namespace Crystal.CpuModule.ViewModels.Implementations;

/// <summary>
/// Root view model bound to the CPU summary tile and detail view. Composes the static
/// </summary>
public sealed class CpuViewModel : BindableBase, ICpuViewModel, IDisposable {
  /// <summary>
  /// The subscriptions to the model's two streams, plus the CPU fan monitor's two streams. 
  /// Disposed when the view model is disposed.
  /// </summary>
  private readonly IDisposable _specsSubscription;

  /// <summary>
  /// The subscriptions to the model's two streams, plus the CPU fan monitor's two streams.
  /// </summary>
  private readonly IDisposable _sensorsSubscription;

  /// <summary>
  /// The subscriptions to the model's two streams, plus the CPU fan monitor's two streams.
  /// </summary>
  private readonly IDisposable _fanSubscription;

  /// <summary>
  /// The subscription to the CPU fan monitor's percentage stream.
  /// </summary>
  private readonly IDisposable _fanPercentSubscription;
  
  /// <summary>
  /// The UI thread marshaller for marshaling actions onto the UI thread.
  /// </summary>
  private readonly UiThreadMarshaller _ui = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="CpuViewModel"/> class.
  /// </summary>
  /// <param name="model"></param>
  /// <param name="cpuFan"></param>
  /// <param name="specs"></param>
  /// <param name="sensors"></param>
  /// <param name="events"></param>
  public CpuViewModel(ICpuModel model,
                      CpuFanMonitor cpuFan,
                      ICpuSpecsViewModel specs,
                      ICpuSensorViewModel sensors,
                      IEventAggregator events) {
    SpecsViewModel = specs;
    SensorsViewModel = sensors;

    ShowDetailCommand = new DelegateCommand(
      () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Cpu));
    ShowDashboardCommand = new DelegateCommand(
      () => events.GetEvent<ShowDashboardEvent>().Publish());

    // Both streams emit off the thread pool (CpuMonitor builds via FromAsync on the
    // default scheduler), so marshal every emission onto the UI thread before touching
    // bound properties / the instruction-set ObservableCollection.
    _specsSubscription = model.Specs.Subscribe(info => OnUi(() => SpecsViewModel.Update(info)));
    _sensorsSubscription = model.Sensors.Subscribe(info => OnUi(() => SensorsViewModel.Update(info)));
    _fanSubscription = cpuFan.Rpm.Subscribe(rpm => OnUi(() => SensorsViewModel.UpdateFan(rpm)));
    _fanPercentSubscription = cpuFan.Percent.Subscribe(pct => OnUi(() => SensorsViewModel.UpdateFanPercent(pct)));
  }

  /// <summary>
  /// The static CPU information emitted once on startup, 
  /// driving the summary tile and detail views.
  /// </summary>
  public ICpuSpecsViewModel SpecsViewModel { get; }

  /// <summary>
  /// The live CPU readings driving the gauges (Load / Voltage / Speed / Power / Temperature)
  /// </summary>
  public ICpuSensorViewModel SensorsViewModel { get; }

  /// <summary>
  /// Raises <c>ShowDetailEvent</c> so the shell swaps in the CPU detail view.
  /// </summary>
  public ICommand ShowDetailCommand { get; }

  /// <summary>
  /// Raises <c>ShowDashboardEvent</c> so the shell returns to the tile dashboard.
  /// </summary>
  public ICommand ShowDashboardCommand { get; }

  /// <summary>
  /// Marshals the specified action onto the UI thread.
  /// </summary>
  /// <param name="action"></param>
  private void OnUi(Action action) => _ui.Post(action);

  /// <summary>
  /// Disposes the subscriptions to the model's streams and the CPU fan monitor's streams.
  /// </summary>
  public void Dispose() {
    _specsSubscription.Dispose();
    _sensorsSubscription.Dispose();
    _fanSubscription.Dispose();
    _fanPercentSubscription.Dispose();
  }
}
