using System.Windows;
using System.Windows.Input;
using CpuModule.Models;
using CpuModule.ViewModels.Interfaces;
using Crystal.Infrastructure.Constants.Navigation;


namespace CpuModule.ViewModels.Implementations;

public sealed class CpuViewModel : BindableBase, ICpuViewModel, IDisposable {
  private readonly IDisposable _specsSubscription;
  private readonly IDisposable _sensorsSubscription;
  private readonly IDisposable _fanSubscription;

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
  }

  public ICpuSpecsViewModel SpecsViewModel { get; }
  public ICpuSensorViewModel SensorsViewModel { get; }
  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  private static void OnUi(Action action) {
    var dispatcher = Application.Current?.Dispatcher;
    if (dispatcher is null || dispatcher.CheckAccess()) action();
    else dispatcher.Invoke(action);
  }

  public void Dispose() {
    _specsSubscription.Dispose();
    _sensorsSubscription.Dispose();
    _fanSubscription.Dispose();
  }
}
