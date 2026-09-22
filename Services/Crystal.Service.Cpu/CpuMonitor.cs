using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Crystal.Service.Cpu;

/// <summary>
/// Exposes the two categories of CPU information a consumer cares about as
/// separate streams:
/// <list type="bullet">
/// <item><see cref="Specs"/> - static inventory (brand, topology, cache,
/// instruction set). Built once and replayed to every subscriber; read the
/// <c>.Specs</c> members of the emitted tree.</item>
/// <item><see cref="Sensors"/> - live readings (temperature, clock, load,
/// power). Re-sampled on a fixed cadence; read the <c>.Sensors</c> members of
/// the emitted tree.</item>
/// </list>
/// Both streams emit the same neutral <see cref="ISystemCpuInfo"/> aggregate;
/// they differ only in cadence and in which half of each socket a subscriber
/// reads. The sensor stream is cold/ref-counted, so polling only runs while
/// someone is subscribed.
/// </summary>
public sealed class CpuMonitor : IDisposable {
  /// <summary>
  /// The connectable specs stream. Wraps a single <see cref="CpuInfoBuilder.BuildAsync"/> call in a
  /// <c>Replay(1)</c> so the one-time result is cached and handed to every subscriber, current or late.
  /// </summary>
  private readonly IConnectableObservable<ISystemCpuInfo> _specs;

  /// <summary>
  /// The live sensor stream. Rebuilds the CPU tree on a fixed cadence; ref-counted so the polling timer
  /// only runs while at least one subscriber is attached.
  /// </summary>
  private readonly IObservable<ISystemCpuInfo> _sensors;

  /// <summary>
  /// The eager connection to <see cref="_specs"/>, kept so it can be torn down in <see cref="Dispose"/>.
  /// </summary>
  private readonly IDisposable _specsConnection;

  /// <summary>
  /// Wires up the two streams over a shared <see cref="CpuInfoBuilder"/>. The specs stream is connected
  /// immediately so the one-time build cost is paid up front; the sensor stream stays idle until subscribed.
  /// </summary>
  /// <param name="builder">Builds the neutral CPU tree from the provider snapshots on demand.</param>
  /// <param name="pollInterval">How often the sensor stream re-samples; defaults to one second.</param>
  /// <param name="scheduler">Scheduler driving the poll timer; defaults to <see cref="DefaultScheduler.Instance"/>.</param>
  public CpuMonitor(CpuInfoBuilder builder, TimeSpan? pollInterval = null, IScheduler? scheduler = null) {
    ArgumentNullException.ThrowIfNull(builder);
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    // Static specs: build once, cache the result and replay it to every
    // subscriber (including late ones). Connect eagerly so the one-time cost
    // is paid up front rather than on first subscription.
    _specs = Observable
      .FromAsync(builder.BuildAsync)
      .Replay(1);
    _specsConnection = _specs.Connect();

    // Live sensors: rebuild on the poll cadence. Each BuildAsync calls the
    // telemetry source's Refresh(), so every emitted tree carries freshly
    // sampled sensor values. RefCount keeps the timer idle until subscribed.
    _sensors = Observable
      .Interval(interval, scheduler)
      .SelectMany(_ => Observable.FromAsync(builder.BuildAsync))
      .Publish()
      .RefCount();
  }

  /// <summary>
  /// Static CPU specs; emits once and replays to new subscribers.
  /// </summary>
  public IObservable<ISystemCpuInfo> Specs => _specs.AsObservable();

  /// <summary>
  /// Live CPU/core sensors; emits a fresh snapshot on each poll.
  /// </summary>
  public IObservable<ISystemCpuInfo> Sensors => _sensors;

  /// <summary>
  /// Tears down the eager specs subscription so its cached build is released. The ref-counted sensor
  /// stream needs no explicit teardown; it stops polling once its last subscriber unsubscribes.
  /// </summary>
  public void Dispose() => _specsConnection.Dispose();
}
