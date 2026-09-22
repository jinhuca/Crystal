namespace Crystal.Service.Network;

/// <summary>
/// Reads a live snapshot of per-interface network activity plus machine-level Wi-Fi state. Extracted
/// so <see cref="NetworkMonitor"/> can be unit-tested against a fake (the concrete
/// <see cref="NetworkLoadSource"/> opens hardware in its constructor).
/// </summary>
public interface INetworkLoadSource {
  /// <summary>
  /// Takes a fresh point-in-time reading of every connected interface (utilization, throughput,
  /// cumulative counters, link speed, and per-adapter Wi-Fi radio state) plus the machine-level
  /// Wi-Fi availability. Called once per poll by <see cref="NetworkMonitor"/>.
  /// </summary>
  /// <returns>A <see cref="NetworkSnapshot"/> describing the current network state.</returns>
  NetworkSnapshot Read();
}
