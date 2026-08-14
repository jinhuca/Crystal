using Crystal.Service.Memory;

namespace Crystal.MemoryModule.Models;

/// <summary>Adapts <see cref="MemoryMonitor"/> into <see cref="IMemoryModel"/>; the monitor owns
/// the polling lifetime and this type just forwards its two streams.</summary>
public sealed class MemoryModel : IMemoryModel, IDisposable {
  private readonly MemoryMonitor _monitor;

  public MemoryModel(MemoryMonitor monitor) => _monitor = monitor;

  public IObservable<MemorySnapshot> Specs => _monitor.Specs;
  public IObservable<MemoryLoadReading> Load => _monitor.Load;

  public void Dispose() => _monitor.Dispose();
}
