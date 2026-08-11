using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Crystal.Service.Storage;

/// <summary>Builds the storage inventory once and replays it to every subscriber via
/// <see cref="Specs"/>, and exposes a live per-disk activity <see cref="Load"/> stream re-sampled on
/// a fixed cadence (ref-counted, so polling only runs while subscribed).</summary>
public sealed class StorageMonitor : IDisposable {
  private readonly IConnectableObservable<StorageSnapshot> _specs;
  private readonly IObservable<StorageLoadReading> _load;
  private readonly IDisposable _connection;

  public StorageMonitor(StorageInfoBuilder builder, IStorageLoadSource loads,
                        TimeSpan? pollInterval = null, IScheduler? scheduler = null) {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentNullException.ThrowIfNull(loads);
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    _specs = Observable.FromAsync(builder.BuildAsync).Replay(1);
    _connection = _specs.Connect();

    _load = Observable
        .Interval(interval, scheduler)
        .Select(_ => loads.Read())
        .Publish()
        .RefCount();
  }

  public IObservable<StorageSnapshot> Specs => _specs.AsObservable();
  public IObservable<StorageLoadReading> Load => _load;

  public void Dispose() => _connection.Dispose();
}
