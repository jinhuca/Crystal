using Crystal.Service.Network;

namespace NetworkModule.Models;

/// <summary>
/// The module's data source. Network is live-only (no static inventory), so it exposes a single
/// <see cref="Sensors"/> stream that re-emits a full per-interface snapshot on a fixed cadence
/// while subscribed.
/// </summary>
public interface INetworkModel {
  IObservable<NetworkSnapshot> Sensors { get; }

  /// <summary>Per-process network top-talkers, re-emitted on the same cadence as <see cref="Sensors"/>
  /// while subscribed. Sourced from the shared ETW broadcaster; empty when ETW isn't running.</summary>
  IObservable<ProcessNetworkSnapshot> TopTalkers { get; }
}
