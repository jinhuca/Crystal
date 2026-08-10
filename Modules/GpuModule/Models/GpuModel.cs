using Crystal.Service.Gpu;

namespace GpuModule.Models;

/// <summary>Adapts <see cref="GpuMonitor"/> into <see cref="IGpuModel"/>; the monitor owns the
/// polling lifetime and this type just forwards its two streams.</summary>
public sealed class GpuModel : IGpuModel, IDisposable {
  private readonly GpuMonitor _monitor;

  public GpuModel(GpuMonitor monitor) => _monitor = monitor;

  public IObservable<GpuSnapshot> Specs => _monitor.Specs;
  public IObservable<GpuSnapshot> Sensors => _monitor.Sensors;

  public void Dispose() => _monitor.Dispose();
}
