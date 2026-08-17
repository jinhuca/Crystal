using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Crystal.Service.Storage;

/// <summary>Re-enumerates the storage inventory on a slow cadence — so a drive plugged in or pulled
/// out appears/disappears (hotplug) — and replays the latest to every subscriber via
/// <see cref="Specs"/>, emitting only when the physical-drive set actually changes. Also exposes a
/// live per-disk activity <see cref="Load"/> stream re-sampled on a fast cadence (ref-counted, so
/// polling only runs while subscribed). When the drive set changes it re-scans the load source's
/// hardware so the live stream stays in step.</summary>
public sealed class StorageMonitor : IDisposable {
  private readonly IConnectableObservable<StorageSnapshot> _specs;
  private readonly IObservable<StorageLoadReading> _load;
  private readonly IDisposable _connection;
  private readonly IDisposable _refreshSubscription;

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

  public IObservable<StorageSnapshot> Specs => _specs.AsObservable();
  public IObservable<StorageLoadReading> Load => _load;

  // Order-independent signature of the physical-drive set (index + serial + capacity + model). A
  // change here — a drive added, removed, or swapped — is what re-emits the inventory and triggers
  // the hardware re-scan; identical successive enumerations are suppressed.
  private static string DriveSetKey(StorageSnapshot snapshot) =>
      string.Join('|', snapshot.Drives
          .Select(d => $"{d.DriveIndex}/{d.SerialNumber}/{d.CapacityGB}/{d.Model}")
          .OrderBy(k => k, StringComparer.Ordinal));

  public void Dispose() {
    _refreshSubscription.Dispose();
    _connection.Dispose();
  }
}
