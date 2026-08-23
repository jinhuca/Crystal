using Crystal.Service.Gpu;

namespace Crystal.GpuModule.Models;

/// <summary>
/// Adapts <see cref="GpuMonitor"/> into <see cref="IGpuModel"/>; the monitor owns the
/// polling lifetime and this type just forwards its two streams.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GpuModel"/> class.
/// </remarks>
/// <param name="monitor">The GPU monitor.</param>
public sealed class GpuModel(GpuMonitor monitor) : IGpuModel, IDisposable {

  /// <summary>
  /// Emits a single snapshot of the GPU adapter inventory, including static specs and the live
  /// sensor readings.
  /// </summary>
  public IObservable<GpuSnapshot> Specs => monitor.Specs;

  /// <summary>
  /// Emits a full snapshot of the GPU adapter inventory, including static specs and the live
  /// sensor readings.
  /// </summary>
  public IObservable<GpuSnapshot> Sensors => monitor.Sensors;

  /// <summary>
  /// Disposes the monitor, which owns the polling lifetime and the Specs replay cache.
  /// </summary>
  public void Dispose() => monitor.Dispose();
}
