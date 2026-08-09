namespace OSModule.Models;

/// <summary>Static OS identity as a replayed stream (built once), plus a live <see cref="Live"/>
/// stream (uptime, wall clock) re-sampled on a cadence while subscribed.</summary>
public interface IOsModel {
  IObservable<OsSnapshot> Info { get; }
  IObservable<OsLiveReading> Live { get; }
}
