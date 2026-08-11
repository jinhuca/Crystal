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
  private readonly IConnectableObservable<MemorySnapshot> _specs;
  private readonly IObservable<MemoryLoadReading> _load;
  private readonly IDisposable _specsConnection;

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

  public IObservable<MemorySnapshot> Specs => _specs.AsObservable();
  public IObservable<MemoryLoadReading> Load => _load;

  public void Dispose() => _specsConnection.Dispose();
}
