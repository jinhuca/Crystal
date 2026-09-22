using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Crystal.Service.Gpu;

/// <summary>
/// Exposes GPU information as two streams, mirroring the CPU service's <c>CpuMonitor</c>:
/// <see cref="Specs"/> (built once, replayed to every subscriber) and <see cref="Sensors"/>
/// (re-sampled on a cadence, ref-counted so polling only runs while subscribed).
/// </summary>
public sealed class GpuMonitor : IDisposable {
  /// <summary>
  /// The specs stream, built exactly once and cached via <c>Replay(1)</c> so every late subscriber
  /// immediately receives the same snapshot without re-hitting WMI. Held as an
  /// <see cref="IConnectableObservable{T}"/> so the build is triggered eagerly at construction (via
  /// <see cref="_specsConnection"/>) rather than per subscription.
  /// </summary>
  private readonly IConnectableObservable<GpuSnapshot> _specs;

  /// <summary>
  /// The sensor stream, re-sampled on the poll interval. <c>Publish().RefCount()</c> shares one
  /// timer across subscribers and stops polling entirely once the last subscriber disposes, so no
  /// background sampling runs while nothing is watching.
  /// </summary>
  private readonly IObservable<GpuSnapshot> _sensors;

  /// <summary>
  /// The live connection that keeps the <see cref="_specs"/> replay cache warm; disposed when the
  /// monitor is disposed to release the underlying build subscription.
  /// </summary>
  private readonly IDisposable _specsConnection;

  /// <summary>
  /// Initializes a new instance of the <see cref="GpuMonitor"/> class and eagerly connects the
  /// specs replay cache so the one-time inventory build starts immediately.
  /// </summary>
  /// <param name="builder">Builds each <see cref="GpuSnapshot"/> from WMI plus the live load source.</param>
  /// <param name="pollInterval">How often the <see cref="Sensors"/> stream re-samples; defaults to one second.</param>
  /// <param name="scheduler">Scheduler driving the poll timer; defaults to <see cref="DefaultScheduler.Instance"/>
  /// (injectable so tests can drive time deterministically).</param>
  public GpuMonitor(
    GpuInfoBuilder builder,
    TimeSpan? pollInterval = null,
    IScheduler? scheduler = null) {
    ArgumentNullException.ThrowIfNull(builder);
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    _specs = Observable.FromAsync(builder.BuildAsync).Replay(1);
    _specsConnection = _specs.Connect();

    _sensors = Observable
      .Interval(interval, scheduler)
      .SelectMany(_ => Observable.FromAsync(builder.BuildAsync))
      .Publish()
      .RefCount();
  }

  /// <summary>
  /// The static GPU inventory: a single snapshot built once and replayed to every subscriber,
  /// whenever they subscribe. Use this for the specs view that does not change between polls.
  /// </summary>
  public IObservable<GpuSnapshot> Specs => _specs.AsObservable();

  /// <summary>
  /// The live sensor stream: a fresh <see cref="GpuSnapshot"/> emitted on each poll tick. Polling is
  /// ref-counted, so it only runs while at least one subscriber is attached.
  /// </summary>
  public IObservable<GpuSnapshot> Sensors => _sensors;

  /// <summary>
  /// Disposes the monitor, which owns the polling lifetime and the Specs replay cache.
  /// </summary>
  public void Dispose() => _specsConnection.Dispose();
}
