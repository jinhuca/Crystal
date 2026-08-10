using Crystal.Service.Storage;

namespace StorageModule.Models;

/// <summary>Adapts <see cref="StorageMonitor"/> into <see cref="IStorageModel"/>; the monitor owns
/// the spec build and polling lifetime and this type just forwards its two streams.</summary>
public sealed class StorageModel : IStorageModel {
  private readonly StorageMonitor _monitor;

  public StorageModel(StorageMonitor monitor) => _monitor = monitor;

  public IObservable<StorageSnapshot> Specs => _monitor.Specs;
  public IObservable<StorageLoadReading> Load => _monitor.Load;
}
