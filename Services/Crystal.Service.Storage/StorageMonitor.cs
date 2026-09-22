using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Crystal.Service.Storage;

/// <summary>
/// Re-enumerates the storage inventory on a slow cadence — so a drive plugged in or pulled
/// out appears/disappears (hotplug) — and replays the latest to every subscriber via
/// <see cref="Specs"/>, emitting only when the physical-drive set actually changes. Also exposes a
/// live per-disk activity <see cref="Load"/> stream re-sampled on a fast cadence (ref-counted, so
/// polling only runs while subscribed). When the drive set changes it re-scans the load source's
/// hardware so the live stream stays in step.
/// </summary>
public sealed class StorageMonitor : IDisposable {
  /// <summary>
  /// The hot, replayed inventory stream: emits a fresh <see cref="StorageSnapshot"/> only
  /// when the drive set changes, with the latest replayed to late subscribers.
  /// </summary>
  private readonly IConnectableObservable<StorageSnapshot> _specs;

  /// <summary>
  /// The ref-counted live activity stream; the underlying poll only runs while subscribed.
  /// </summary>
  private readonly IObservable<StorageLoadReading> _load;

  /// <summary>
  /// Keeps <see cref="_specs"/> connected (and thus its Replay(1) buffer warm) for the
  /// monitor's lifetime, independent of whether anyone is currently subscribed.
  /// </summary>
  private readonly IDisposable _connection;

  /// <summary>
  /// The subscription that re-scans the load source's hardware on each inventory change.
  /// </summary>
  private readonly IDisposable _refreshSubscription;

  /// <summary>
  /// Wires up the inventory and live-load pipelines. <paramref name="pollInterval"/> sets the
  /// fast activity cadence (default 1s) and <paramref name="inventoryInterval"/> the slow hotplug
  /// re-scan cadence (default 5s); <paramref name="scheduler"/> is injectable for deterministic tests.
  /// </summary>
  /// <param name="builder">Builds the static storage inventory from WMI.</param>
  /// <param name="loads">The live per-disk activity source, re-scanned when the drive set changes.</param>
  /// <param name="pollInterval">The activity sampling interval; defaults to 1 second.</param>
  /// <param name="scheduler">The scheduler driving both timers; defaults to <see cref="DefaultScheduler.Instance"/>.</param>
  /// <param name="inventoryInterval">The inventory re-enumeration interval; defaults to 5 seconds.</param>
  public StorageMonitor(StorageInfoBuilder builder, IStorageLoadSource loads,
                        TimeSpan? pollInterval = null, IScheduler? scheduler = null,
                        TimeSpan? inventoryInterval = null) {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentNullException.ThrowIfNull(loads);
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    var inventoryPoll = inventoryInterval ?? TimeSpan.FromSeconds(5);
    scheduler ??= DefaultScheduler.Instance;

    // Build the inventory eagerly, then re-query WMI on a slow cadence for hotplug. Only surface a
    // snapshot when the drive set changed, so a steady machine emits exactly once (late subscribers
    // still get it via Replay(1)) while a plug/unplug re-emits the new inventory.
    _specs = Observable
        .FromAsync(builder.BuildAsync)
        .Concat(Observable
            .Interval(inventoryPoll, scheduler)
            .SelectMany(_ => Observable.FromAsync(builder.BuildAsync)))
        .DistinctUntilChanged(DriveSetKey)
        .Replay(1);
    _connection = _specs.Connect();

    // On every change after the initial inventory, re-scan the load source's hardware so the new
    // drive reports (or the removed one stops). Skip(1) ignores the first snapshot — the source
    // already opened that drive set in its constructor.
    _refreshSubscription = _specs.Skip(1).Subscribe(_ => loads.Refresh());

    _load = Observable
        .Interval(interval, scheduler)
        .Select(_ => loads.Read())
        .Publish()
        .RefCount();
  }

  /// <summary>
  /// The storage inventory stream: the current drive set, re-emitted on hotplug. Replays the
  /// latest snapshot to new subscribers.
  /// </summary>
  public IObservable<StorageSnapshot> Specs => _specs.AsObservable();

  /// <summary>
  /// The live per-disk activity stream, sampled on the fast poll cadence while subscribed.
  /// </summary>
  public IObservable<StorageLoadReading> Load => _load;

  // Order-independent signature of the physical-drive set (index + serial + capacity + model). A
  // change here — a drive added, removed, or swapped — is what re-emits the inventory and triggers
  // the hardware re-scan; identical successive enumerations are suppressed.
  private static string DriveSetKey(StorageSnapshot snapshot) =>
      string.Join('|', snapshot.Drives
          .Select(d => $"{d.DriveIndex}/{d.SerialNumber}/{d.CapacityGB}/{d.Model}")
          .OrderBy(k => k, StringComparer.Ordinal));

  /// <summary>
  /// Tears down the hotplug re-scan subscription and disconnects the inventory stream. The
  /// load stream stops on its own once its last subscriber unsubscribes (RefCount).
  /// </summary>
  public void Dispose() {
    _refreshSubscription.Dispose();
    _connection.Dispose();
  }
}
