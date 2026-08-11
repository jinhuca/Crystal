namespace Crystal.Service.Network;

/// <summary>
/// Reads a live snapshot of per-interface network activity plus machine-level Wi-Fi state. Extracted
/// so <see cref="NetworkMonitor"/> can be unit-tested against a fake (the concrete
/// <see cref="NetworkLoadSource"/> opens hardware in its constructor).
/// </summary>
public interface INetworkLoadSource {
  NetworkSnapshot Read();
}
