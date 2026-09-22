using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Crystal.Service.Memory;

/// <summary>
/// Exposes memory information as two streams, mirroring the CPU/GPU services' monitors:
/// <see cref="Specs"/> (static inventory built once, replayed to every subscriber) and
/// <see cref="Load"/> (used %, used/available GB, kernel figures — re-sampled on a cadence,
/// ref-counted so polling only runs while subscribed).
/// </summary>
public sealed class MemoryMonitor : IDisposable {
  /// <summary>
  /// The static-inventory stream, built once and replayed (Replay(1)) so late subscribers
  /// still receive the last snapshot without rebuilding it.
  /// </summary>
  private readonly IConnectableObservable<MemorySnapshot> _specs;

  /// <summary>
  /// The live-load stream, ref-counted so the poll timer only runs while subscribed.
  /// </summary>
  private readonly IObservable<MemoryLoadReading> _load;

  /// <summary>
  /// Handle to the eager <see cref="_specs"/> connection, disposed to tear it down.
  /// </summary>
  private readonly IDisposable _specsConnection;

  /// <summary>
  /// Wires up the two streams. The specs stream connects immediately (the inventory build starts at
  /// construction and is cached); the load stream stays cold until first subscription.
  /// </summary>
  /// <param name="builder">Builds the static memory inventory.</param>
  /// <param name="loads">Source of live memory-load samples.</param>
  /// <param name="pollInterval">How often to re-sample load; defaults to one second.</param>
  /// <param name="scheduler">Scheduler the poll timer runs on; defaults to
  /// <see cref="DefaultScheduler.Instance"/> (injectable for deterministic tests).</param>
  public MemoryMonitor(MemoryInfoBuilder builder, IMemoryLoadSource loads,
                       TimeSpan? pollInterval = null, IScheduler? scheduler = null) {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentNullException.ThrowIfNull(loads);
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    _specs = Observable.FromAsync(builder.BuildAsync).Replay(1);
    _specsConnection = _specs.Connect();

    _load = Observable
        .Interval(interval, scheduler)
        .Select(_ => loads.Read())
        .Publish()
        .RefCount();
  }

  /// <summary>
  /// The static memory inventory, replayed to every subscriber (fires once, then completes
  /// per the underlying build).
  /// </summary>
  public IObservable<MemorySnapshot> Specs => _specs.AsObservable();

  /// <summary>
  /// The live memory-load stream. Polling starts on first subscription and stops when the
  /// last subscriber unsubscribes (ref-counted).
  /// </summary>
  public IObservable<MemoryLoadReading> Load => _load;

  /// <summary>
  /// Tears down the eager specs connection. Does not affect load subscribers, which manage
  /// their own lifetime via ref-counting.
  /// </summary>
  public void Dispose() => _specsConnection.Dispose();
}
