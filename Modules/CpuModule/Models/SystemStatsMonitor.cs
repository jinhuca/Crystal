using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CpuModule.Models;

/// <summary>
/// Polls the OS process table on a cadence and emits system-wide totals — process count and the
/// summed thread and handle counts across every process. Uses <see cref="Process.GetProcesses"/>
/// rather than WMI: it's a cheap in-process enumeration and, since Crystal runs elevated, it can
/// read thread/handle counts for processes in other sessions.
/// <para>
/// Cold and ref-counted like the other <c>*Monitor</c> types: the timer only ticks while
/// something is subscribed, and the default cadence is 1 second.
/// </para>
/// </summary>
public sealed class SystemStatsMonitor {
  private readonly IObservable<SystemStats> _stats;

  public SystemStatsMonitor(TimeSpan? pollInterval = null, IScheduler? scheduler = null) {
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    _stats = Observable
        .Interval(interval, scheduler)
        .Select(_ => Sample())
        .Publish()
        .RefCount();
  }

  /// <summary>Live system totals; emits a fresh snapshot on each poll.</summary>
  public IObservable<SystemStats> Stats => _stats;

  private static SystemStats Sample() {
    int processes = 0, threads = 0, handles = 0;
    foreach (var p in Process.GetProcesses()) {
      try {
        processes++;
        threads += p.Threads.Count;
        // HandleCount can throw for protected processes we can't open; skip those rather
        // than let one inaccessible process abort the whole sample.
        handles += p.HandleCount;
      } catch {
        // Process exited between enumeration and read, or access was denied — ignore it.
      } finally {
        p.Dispose();
      }
    }
    return new SystemStats(processes, threads, handles);
  }
}
