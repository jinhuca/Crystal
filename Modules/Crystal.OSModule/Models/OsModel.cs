using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Crystal.OSModule.Models;

/// <summary>Builds the OS identity once and replays it to every subscriber, and exposes a live
/// uptime/clock stream re-sampled on a fixed cadence (ref-counted, so polling only runs while
/// subscribed). Mirrors <c>MemoryModel</c>: a one-shot replayed spec plus a ref-counted poll.</summary>
public sealed class OsModel : IOsModel, IDisposable {
  private readonly IConnectableObservable<OsSnapshot> _info;
  private readonly IObservable<OsLiveReading> _live;
  private readonly IDisposable _connection;

  public OsModel(OsInfoBuilder builder, TimeSpan? pollInterval = null, IScheduler? scheduler = null,
                 Func<DateTimeOffset>? clock = null) {
    ArgumentNullException.ThrowIfNull(builder);
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;
    clock ??= () => DateTimeOffset.Now;

    var snapshot = builder.Build();
    _info = Observable.Return(snapshot).Replay(1);
    _connection = _info.Connect();

    // Emit immediately (StartWith) so the tile shows uptime without waiting a full interval, then
    // re-sample every tick. Uptime is measured from the boot instant captured in the snapshot;
    // when it's unknown we fall back to the kernel tick count so the value is never blank.
    _live = Observable
        .Interval(interval, scheduler)
        .Select(_ => Read(snapshot, clock))
        .StartWith(Read(snapshot, clock))
        .Publish()
        .RefCount();
  }

  private static OsLiveReading Read(OsSnapshot snapshot, Func<DateTimeOffset> clock) {
    var now = clock();
    var uptime = snapshot.LastBootTime is { } boot
        ? now - boot
        : TimeSpan.FromMilliseconds(Environment.TickCount64);
    return new OsLiveReading(uptime, now);
  }

  public IObservable<OsSnapshot> Info => _info.AsObservable();
  public IObservable<OsLiveReading> Live => _live;

  public void Dispose() => _connection.Dispose();
}
