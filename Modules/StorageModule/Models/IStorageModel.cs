namespace StorageModule.Models;

/// <summary>Static storage inventory as a replayed stream (built once), plus a live
/// <see cref="Load"/> stream (busiest-drive total-activity %) polled on a cadence while subscribed.</summary>
public interface IStorageModel {
  IObservable<StorageSnapshot> Specs { get; }
  IObservable<double> Load { get; }
}
