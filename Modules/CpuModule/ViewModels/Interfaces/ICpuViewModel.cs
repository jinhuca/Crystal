namespace CpuModule.ViewModels.Interfaces;

/// <summary>
/// Root view model bound to <c>CpuView</c>. Composes the static
/// <see cref="SpecsViewModel"/> and the live <see cref="SensorsViewModel"/>,
/// and owns the subscription to the model's streams.
/// </summary>
public interface ICpuViewModel {
  ICpuSpecsViewModel SpecsViewModel { get; }
  ICpuSensorViewModel SensorsViewModel { get; }
}
