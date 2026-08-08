using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MemoryModule.Models;

/// <summary>Builds the memory inventory once and replays it to every subscriber, and exposes a
/// live used-percentage <see cref="Load"/> stream re-sampled on a fixed cadence (ref-counted, so
/// polling only runs while subscribed).</summary>
public sealed class MemoryModel : IMemoryModel, IDisposable {
  private readonly IConnectableObservable<MemorySnapshot> _specs;
  private readonly IObservable<MemoryLoadReading> _load;
  private readonly IDisposable _connection;

  public MemoryModel(MemoryInfoBuilder builder, MemoryLoadSource loads,
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

  public IObservable<MemorySnapshot> Specs => _specs.AsObservable();
  public IObservable<MemoryLoadReading> Load => _load;

  public void Dispose() => _connection.Dispose();
}
