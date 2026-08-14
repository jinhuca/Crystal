using Crystal.Service.Network;

namespace Crystal.NetworkModule.Models;

/// <summary>Adapts <see cref="NetworkMonitor"/> into <see cref="INetworkModel"/>; the monitor owns
/// the polling lifetime and this type just forwards its two streams.</summary>
public sealed class NetworkModel : INetworkModel {
  private readonly NetworkMonitor _monitor;

  public NetworkModel(NetworkMonitor monitor) => _monitor = monitor;

  public IObservable<NetworkSnapshot> Sensors => _monitor.Sensors;

  public IObservable<ProcessNetworkSnapshot> TopTalkers => _monitor.TopTalkers;
}
