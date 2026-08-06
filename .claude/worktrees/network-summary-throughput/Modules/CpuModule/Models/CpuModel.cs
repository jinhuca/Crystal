using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Service.Cpu;

namespace CpuModule.Models;

/// <summary>
/// Adapts the <see cref="CpuMonitor"/> service into the module's <see cref="ICpuModel"/>.
/// The monitor owns the polling lifetime; this type just forwards its two streams.
/// </summary>
public sealed class CpuModel : ICpuModel, IDisposable {
  private readonly CpuMonitor _monitor;

  public CpuModel(CpuMonitor monitor) => _monitor = monitor;

  public IObservable<ISystemCpuInfo> Specs => _monitor.Specs;
  public IObservable<ISystemCpuInfo> Sensors => _monitor.Sensors;

  public void Dispose() => _monitor.Dispose();
}
