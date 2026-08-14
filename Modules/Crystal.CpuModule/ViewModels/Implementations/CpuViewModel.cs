using Crystal.Controls.Threading;
using Crystal.CpuModule.Models;
using Crystal.CpuModule.ViewModels.Interfaces;
using Crystal.Infrastructure.Constants.Navigation;
using System.Windows.Input;


namespace Crystal.CpuModule.ViewModels.Implementations;

public sealed class CpuViewModel : BindableBase, ICpuViewModel, IDisposable {
  private readonly IDisposable _specsSubscription;
  private readonly IDisposable _sensorsSubscription;
  private readonly IDisposable _fanSubscription;
  private readonly IDisposable _fanPercentSubscription;
  private readonly UiThreadMarshaller _ui = new();

  public CpuViewModel(ICpuModel model, CpuFanMonitor cpuFan, ICpuSpecsViewModel specs,
                      ICpuSensorViewModel sensors, IEventAggregator events) {
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

  public ICpuSpecsViewModel SpecsViewModel { get; }
  public ICpuSensorViewModel SensorsViewModel { get; }
  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  private void OnUi(Action action) => _ui.Post(action);

  public void Dispose() {
    _specsSubscription.Dispose();
    _sensorsSubscription.Dispose();
    _fanSubscription.Dispose();
    _fanPercentSubscription.Dispose();
  }
}
