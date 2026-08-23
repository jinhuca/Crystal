using Crystal.Service.Gpu;

namespace Crystal.GpuModule.Models;

/// <summary>
/// Adapts <see cref="GpuMonitor"/> into <see cref="IGpuModel"/>; the monitor owns the
/// polling lifetime and this type just forwards its two streams.
/// </summary>
public sealed class GpuModel : IGpuModel, IDisposable {
  /// <summary>
  /// The monitor owns the polling lifetime and the Specs replay cache, so it must be a singleton.
  /// </summary>
  private readonly GpuMonitor _monitor;

  /// <summary>
  /// Initializes a new instance of the <see cref="GpuModel"/> class.
  /// </summary>
  /// <param name="monitor">The GPU monitor.</param>
  public GpuModel(GpuMonitor monitor) => _monitor = monitor;

  /// <summary>
  /// Emits a single snapshot of the GPU adapter inventory, including static specs and the live
  /// sensor readings.
  /// </summary>
  public IObservable<GpuSnapshot> Specs => _monitor.Specs;

  /// <summary>
  /// Emits a full snapshot of the GPU adapter inventory, including static specs and the live
  /// sensor readings.
  /// </summary>
  public IObservable<GpuSnapshot> Sensors => _monitor.Sensors;

  /// <summary>
  /// Disposes the monitor, which owns the polling lifetime and the Specs replay cache.
  /// </summary>
  public void Dispose() => _monitor.Dispose();
}
