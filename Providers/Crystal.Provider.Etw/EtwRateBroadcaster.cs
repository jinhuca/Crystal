using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Crystal.Provider.Etw;

/// <summary>
/// Owns the single <see cref="IProcessEtwSource.SnapshotRates"/> poll and multicasts each snapshot
/// to every consumer. SnapshotRates is destructive — a call reports rates over the window since the
/// previous call, then resets the interval accumulators — so it can only ever have one caller. This
/// broadcaster is that caller: it polls on a fixed cadence while anything is subscribed (ref-counted)
/// and hands the same per-PID snapshot to all subscribers (the process list and the network
/// top-talkers view), so neither steals the other's window.
/// </summary>
public sealed class EtwRateBroadcaster {
  private readonly IProcessEtwSource _source;
  private readonly IObservable<IReadOnlyDictionary<uint, ProcessEtwMetrics>> _rates;

  public EtwRateBroadcaster(IProcessEtwSource source, TimeSpan? pollInterval = null, IScheduler? scheduler = null) {
    ArgumentNullException.ThrowIfNull(source);
    _source = source;
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    _rates = Observable
        .Interval(interval, scheduler)
        .Select(_ => _source.SnapshotRates())
        .Publish()
        .RefCount();
  }

  /// <summary>Per-PID ETW rates, one emission per poll while subscribed and shared across consumers.</summary>
  public IObservable<IReadOnlyDictionary<uint, ProcessEtwMetrics>> Rates => _rates;

  /// <summary>True once the underlying kernel session is running.</summary>
  public bool IsRunning => _source.IsRunning;

  /// <summary>Reason the kernel session did not start (e.g. "not elevated"), else null.</summary>
  public string? StartError => _source.StartError;
}
