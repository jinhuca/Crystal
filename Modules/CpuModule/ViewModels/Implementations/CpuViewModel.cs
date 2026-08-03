using System.Windows;
using CpuModule.Models;
using CpuModule.ViewModels.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

namespace CpuModule.ViewModels.Implementations;

public sealed class CpuViewModel : BindableBase, ICpuViewModel, IDisposable {
  private readonly IDisposable _specsSubscription;
  private readonly IDisposable _sensorsSubscription;

  public CpuViewModel(ICpuModel model, ICpuSpecsViewModel specs, ICpuSensorViewModel sensors) {
    SpecsViewModel = specs;
    SensorsViewModel = sensors;

    // Both streams emit off the thread pool (CpuMonitor builds via FromAsync on the
    // default scheduler), so marshal every emission onto the UI thread before touching
    // bound properties / the instruction-set ObservableCollection.
    _specsSubscription = model.Specs.Subscribe(info => OnUi(() => SpecsViewModel.Update(info)));
    _sensorsSubscription = model.Sensors.Subscribe(info => OnUi(() => SensorsViewModel.Update(info)));
  }

  public ICpuSpecsViewModel SpecsViewModel { get; }
  public ICpuSensorViewModel SensorsViewModel { get; }

  private static void OnUi(Action action) {
    var dispatcher = Application.Current?.Dispatcher;
    if (dispatcher is null || dispatcher.CheckAccess()) action();
    else dispatcher.Invoke(action);
  }

  public void Dispose() {
    _specsSubscription.Dispose();
    _sensorsSubscription.Dispose();
  }
}
