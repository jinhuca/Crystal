using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Service.Cpu;

namespace Crystal.CpuModule.Models;

/// <summary>
/// Adapts the <see cref="CpuMonitor"/> service into the module's <see cref="ICpuModel"/>.
/// The monitor owns the polling lifetime; this type just forwards its two streams.
/// </summary>
public sealed class CpuModel : ICpuModel, IDisposable {
  /// <summary>
  /// The service that owns the polling lifetime and the Specs replay cache. 
  /// This type just forwards its two streams.
  /// </summary>
  private readonly CpuMonitor _monitor;

  /// <summary>
  /// Initializes a new instance of the <see cref="CpuModel"/> class, 
  /// forwarding the <paramref name="monitor"/>'s two streams.
  /// </summary>
  /// <param name="monitor"></param>
  public CpuModel(CpuMonitor monitor) => _monitor = monitor;

  /// <summary>
  /// Static CPU specs; emits once and replays to new subscribers.
  /// </summary>
  public IObservable<ISystemCpuInfo> Specs => _monitor.Specs;

  /// <summary>
  /// Live CPU/core sensors; emits a fresh snapshot on each poll.
  /// </summary>
  public IObservable<ISystemCpuInfo> Sensors => _monitor.Sensors;

  /// <summary>
  /// Disposes the underlying <see cref="CpuMonitor"/> to stop polling and release resources.
  /// </summary>
  public void Dispose() => _monitor.Dispose();
}
