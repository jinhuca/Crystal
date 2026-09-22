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
  /// <summary>
  /// The shared, ref-counted polling stream backing <see cref="Sensors"/>.
  /// </summary>
  private readonly IObservable<NetworkSnapshot> _sensors;

  /// <summary>
  /// Wires the polling pipeline. The <see cref="Sensors"/> stream is built once as a
  /// <c>Publish().RefCount()</c> so the source is polled only while at least one subscriber is
  /// attached and all subscribers share a single poll. The <see cref="TopTalkers"/> stream is
  /// forwarded from the ETW-driven <paramref name="processNetwork"/> unchanged.
  /// </summary>
  /// <param name="loads">Source polled once per <paramref name="pollInterval"/> for interface readings.</param>
  /// <param name="processNetwork">Provides the per-process top-talkers stream, on its own ETW cadence.</param>
  /// <param name="pollInterval">Sampling cadence; defaults to one second.</param>
  /// <param name="scheduler">Scheduler the interval runs on; defaults to <see cref="DefaultScheduler.Instance"/> (injectable for tests).</param>
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

  /// <summary>
  /// Live per-interface network snapshots, one per poll while subscribed.
  /// </summary>
  public IObservable<NetworkSnapshot> Sensors => _sensors;

  /// <summary>
  /// Ranked per-process network usage, emitted on the ETW broadcaster's own cadence.
  /// </summary>
  public IObservable<ProcessNetworkSnapshot> TopTalkers { get; }
}
