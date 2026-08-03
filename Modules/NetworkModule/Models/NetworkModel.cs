using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace NetworkModule.Models;

/// <summary>
/// Polls <see cref="NetworkLoadSource"/> on a fixed cadence and exposes the result as a single
/// ref-counted <see cref="Sensors"/> stream (polling only runs while subscribed). Network has no
/// static inventory, so there is no separate specs stream.
/// </summary>
public sealed class NetworkModel : INetworkModel {
  private readonly IObservable<NetworkSnapshot> _sensors;

  public NetworkModel(NetworkLoadSource loads, TimeSpan? pollInterval = null, IScheduler? scheduler = null) {
    ArgumentNullException.ThrowIfNull(loads);
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    _sensors = Observable
        .Interval(interval, scheduler)
        .Select(_ => new NetworkSnapshot(loads.Read()))
        .Publish()
        .RefCount();
  }

  public IObservable<NetworkSnapshot> Sensors => _sensors;
}
