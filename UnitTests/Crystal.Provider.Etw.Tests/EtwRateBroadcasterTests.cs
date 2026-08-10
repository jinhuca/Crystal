using System.Reactive.Linq;
using Crystal.Provider.Etw;
using Microsoft.Reactive.Testing;
using Xunit;

namespace Crystal.Provider.Etw.Tests;

/// <summary>
/// Verifies the broadcaster's contract: it owns the single destructive <c>SnapshotRates</c> poll,
/// fans each snapshot out to every subscriber (so neither consumer steals the other's window), and
/// only polls while something is subscribed.
/// </summary>
public class EtwRateBroadcasterTests {
  // Counts SnapshotRates calls and hands back a scripted queue of snapshots, mirroring the reader's
  // destructive per-interval semantics (each call reports one window, then the accumulators reset).
  private sealed class CountingSource : IProcessEtwSource {
    private readonly Queue<IReadOnlyDictionary<uint, ProcessEtwMetrics>> _snapshots;

    public CountingSource(bool isRunning, string? startError,
                          params IReadOnlyDictionary<uint, ProcessEtwMetrics>[] snapshots) {
      IsRunning = isRunning;
      StartError = startError;
      _snapshots = new Queue<IReadOnlyDictionary<uint, ProcessEtwMetrics>>(snapshots);
    }

    public int SnapshotCalls { get; private set; }
    public bool IsRunning { get; }
    public string? StartError { get; }

    public IReadOnlyDictionary<uint, ProcessEtwMetrics> SnapshotRates() {
      SnapshotCalls++;
      return _snapshots.Count > 0 ? _snapshots.Dequeue() : new Dictionary<uint, ProcessEtwMetrics>();
    }

    public void Pause() { }
    public void Resume() { }
    public void Dispose() { }
  }

  private static IReadOnlyDictionary<uint, ProcessEtwMetrics> Snap(uint pid, double gpu) =>
      new Dictionary<uint, ProcessEtwMetrics> { [pid] = new(0, 0, gpu) };

  [Fact]
  public void Emits_one_snapshot_per_poll_interval() {
    var scheduler = new TestScheduler();
    var source = new CountingSource(isRunning: true, startError: null,
        Snap(1, 10), Snap(1, 20));
    var broadcaster = new EtwRateBroadcaster(source, TimeSpan.FromSeconds(1), scheduler);

    var seen = new List<IReadOnlyDictionary<uint, ProcessEtwMetrics>>();
    using var _ = broadcaster.Rates.Subscribe(seen.Add);

    scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
    scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

    Assert.Equal(2, seen.Count);
    Assert.Equal(10, seen[0][1].GpuPercent);
    Assert.Equal(20, seen[1][1].GpuPercent);
  }

  [Fact]
  public void Does_not_poll_before_the_first_interval_elapses() {
    var scheduler = new TestScheduler();
    var source = new CountingSource(isRunning: true, startError: null, Snap(1, 10));
    var broadcaster = new EtwRateBroadcaster(source, TimeSpan.FromSeconds(1), scheduler);

    using var _ = broadcaster.Rates.Subscribe();
    scheduler.AdvanceBy(TimeSpan.FromMilliseconds(999).Ticks);

    Assert.Equal(0, source.SnapshotCalls);
  }

  [Fact]
  public void Shares_a_single_poll_across_all_subscribers() {
    var scheduler = new TestScheduler();
    var source = new CountingSource(isRunning: true, startError: null, Snap(7, 42));
    var broadcaster = new EtwRateBroadcaster(source, TimeSpan.FromSeconds(1), scheduler);

    IReadOnlyDictionary<uint, ProcessEtwMetrics>? a = null, b = null;
    using var s1 = broadcaster.Rates.Subscribe(x => a = x);
    using var s2 = broadcaster.Rates.Subscribe(x => b = x);

    scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

    // One destructive snapshot, delivered to both consumers — neither steals the other's window.
    Assert.Equal(1, source.SnapshotCalls);
    Assert.Same(a, b);
    Assert.Equal(42, a![7].GpuPercent);
  }

  [Fact]
  public void Does_not_poll_while_nothing_is_subscribed() {
    var scheduler = new TestScheduler();
    var source = new CountingSource(isRunning: true, startError: null);
    var broadcaster = new EtwRateBroadcaster(source, TimeSpan.FromSeconds(1), scheduler);

    // RefCount: with no live subscription the interval timer never runs.
    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

    Assert.Equal(0, source.SnapshotCalls);
  }

  [Fact]
  public void Stops_polling_after_the_last_subscriber_leaves() {
    var scheduler = new TestScheduler();
    var source = new CountingSource(isRunning: true, startError: null, Snap(1, 1), Snap(1, 2));
    var broadcaster = new EtwRateBroadcaster(source, TimeSpan.FromSeconds(1), scheduler);

    var subscription = broadcaster.Rates.Subscribe();
    scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
    Assert.Equal(1, source.SnapshotCalls);

    subscription.Dispose();
    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

    // No further polling once the ref count drops to zero.
    Assert.Equal(1, source.SnapshotCalls);
  }

  [Fact]
  public void Passes_through_running_state_and_start_error() {
    var scheduler = new TestScheduler();
    var source = new CountingSource(isRunning: false, startError: "not elevated");
    var broadcaster = new EtwRateBroadcaster(source, TimeSpan.FromSeconds(1), scheduler);

    Assert.False(broadcaster.IsRunning);
    Assert.Equal("not elevated", broadcaster.StartError);
  }

  [Fact]
  public void Null_source_throws() =>
      Assert.Throws<ArgumentNullException>(() => new EtwRateBroadcaster(null!));
}
