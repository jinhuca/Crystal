using Microsoft.Reactive.Testing;
using Xunit;

namespace Crystal.Service.Process.Tests;

// SystemStatsMonitor samples the real OS process table (System.Diagnostics.Process.GetProcesses),
// so the exact counts aren't deterministic — these assert the cadence/ref-count behavior and that
// the totals are sane, which is all the monitor's own logic controls.
public class SystemStatsMonitorTests {
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  [Fact]
  public void EmitsOnePerInterval_WithPositiveCounts() {
    var scheduler = new TestScheduler();
    var monitor = new SystemStatsMonitor(Interval, scheduler);

    var stats = new List<SystemStats>();
    using var _ = monitor.Stats.Subscribe(stats.Add);
    scheduler.AdvanceBy(Interval.Ticks * 3);

    Assert.Equal(3, stats.Count);
    foreach (var s in stats) {
      Assert.True(s.Processes > 0);
      Assert.True(s.Threads >= s.Processes);   // every process has at least one thread
      Assert.True(s.Handles >= 0);
    }
  }

  [Fact]
  public void ColdUntilSubscribed_DoesNotEmitBeforeFirstInterval() {
    var scheduler = new TestScheduler();
    var monitor = new SystemStatsMonitor(Interval, scheduler);

    var stats = new List<SystemStats>();
    using var _ = monitor.Stats.Subscribe(stats.Add);

    // No time has advanced past the first interval yet → nothing emitted (unlike the process
    // monitor, this stream has no synchronous first sample).
    Assert.Empty(stats);

    scheduler.AdvanceBy(Interval.Ticks);
    Assert.Single(stats);
  }
}
