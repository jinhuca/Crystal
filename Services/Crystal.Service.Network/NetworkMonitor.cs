using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Crystal.Service.Network;

/// <summary>
/// Polls <see cref="INetworkLoadSource"/> on a fixed cadence and exposes the result as a single
/// ref-counted <see cref="Sensors"/> stream (polling only runs while subscribed). Network has no
/// static inventory, so there is no separate specs stream. The per-process <see cref="TopTalkers"/>
/// stream is driven by the shared ETW broadcaster's own cadence, so it is forwarded as-is.
/// </summary>
public sealed class NetworkMonitor {
  private readonly IObservable<NetworkSnapshot> _sensors;

  public NetworkMonitor(INetworkLoadSource loads, ProcessNetworkSource processNetwork,
                        TimeSpan? pollInterval = null, IScheduler? scheduler = null) {
    ArgumentNullException.ThrowIfNull(loads);
    ArgumentNullException.ThrowIfNull(processNetwork);
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    _sensors = Observable
        .Interval(interval, scheduler)
        .Select(_ => loads.Read())
        .Publish()
        .RefCount();

    TopTalkers = processNetwork.TopTalkers;
  }

  public IObservable<NetworkSnapshot> Sensors => _sensors;

  public IObservable<ProcessNetworkSnapshot> TopTalkers { get; }
}
