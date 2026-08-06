namespace MemoryModule.Models;

/// <summary>Static memory inventory as a replayed stream (built once), plus a live
/// <see cref="Load"/> stream (physical-memory used %) polled on a cadence while subscribed.</summary>
public interface IMemoryModel {
  IObservable<MemorySnapshot> Specs { get; }
  IObservable<double> Load { get; }
}
