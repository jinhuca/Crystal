using System.Reactive.Linq;
using Crystal.Provider.Etw;
using Crystal.Service.Network;
using Microsoft.Reactive.Testing;
using Xunit;

namespace NetworkModule.Tests;

public class ProcessNetworkSourceTests {
  // Feeds a scripted sequence of per-poll rate snapshots; each SnapshotRates() call returns the next
  // one, mirroring the real reader's destructive per-interval semantics.
  private sealed class FakeEtwSource : IProcessEtwSource {
    private readonly Queue<IReadOnlyDictionary<uint, ProcessEtwMetrics>> _snapshots;

    public FakeEtwSource(bool isRunning, string? startError,
                         params IReadOnlyDictionary<uint, ProcessEtwMetrics>[] snapshots) {
      IsRunning = isRunning;
      StartError = startError;
      _snapshots = new Queue<IReadOnlyDictionary<uint, ProcessEtwMetrics>>(snapshots);
    }

    public bool IsRunning { get; }
    public string? StartError { get; }

    public IReadOnlyDictionary<uint, ProcessEtwMetrics> SnapshotRates() =>
        _snapshots.Count > 0 ? _snapshots.Dequeue() : new Dictionary<uint, ProcessEtwMetrics>();

    public void Pause() { }
    public void Resume() { }
    public void Dispose() { }
  }

  private static ProcessEtwMetrics Net(double bytesPerSec) => new(0, bytesPerSec, 0);

  private static ProcessNetworkSource Create(IProcessEtwSource etw, TestScheduler scheduler,
                                             IReadOnlyDictionary<uint, string>? names = null) {
    var broadcaster = new EtwRateBroadcaster(etw, TimeSpan.FromSeconds(1), scheduler);
    return new ProcessNetworkSource(broadcaster, names is null ? null : () => names);
  }

  [Fact]
  public void Ranks_by_net_rate_and_joins_names() {
    var scheduler = new TestScheduler();
    var etw = new FakeEtwSource(isRunning: true, startError: null,
        new Dictionary<uint, ProcessEtwMetrics> {
          [10] = Net(2_048),
          [20] = Net(5_000_000),
        });
    var names = new Dictionary<uint, string> { [10] = "svchost", [20] = "chrome" };
    var source = Create(etw, scheduler, names);

    ProcessNetworkSnapshot? result = null;
    using var _ = source.TopTalkers.Subscribe(s => result = s);
    scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

    Assert.NotNull(result);
    Assert.True(result!.IsRunning);
    Assert.Equal(2, result.TopTalkers.Count);
    Assert.Equal(20u, result.TopTalkers[0].ProcessId);
    Assert.Equal("chrome", result.TopTalkers[0].Name);
    Assert.Equal(10u, result.TopTalkers[1].ProcessId);
  }

  [Fact]
  public void Drops_processes_with_no_network_activity() {
    var scheduler = new TestScheduler();
    var etw = new FakeEtwSource(isRunning: true, startError: null,
        new Dictionary<uint, ProcessEtwMetrics> {
          [10] = Net(0),
          [20] = Net(1_000),
        });
    var source = Create(etw, scheduler, new Dictionary<uint, string> { [20] = "steam" });

    ProcessNetworkSnapshot? result = null;
    using var _ = source.TopTalkers.Subscribe(s => result = s);
    scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

    Assert.NotNull(result);
    Assert.Single(result!.TopTalkers);
    Assert.Equal(20u, result.TopTalkers[0].ProcessId);
  }

  [Fact]
  public void Unresolved_pid_falls_back_to_pid_label() {
    var scheduler = new TestScheduler();
    var etw = new FakeEtwSource(isRunning: true, startError: null,
        new Dictionary<uint, ProcessEtwMetrics> { [999] = Net(4_096) });
    var source = Create(etw, scheduler, new Dictionary<uint, string>());

    ProcessNetworkSnapshot? result = null;
    using var _ = source.TopTalkers.Subscribe(s => result = s);
    scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

    Assert.Equal("PID 999", result!.TopTalkers[0].Name);
  }

  [Fact]
  public void Reports_status_when_etw_is_not_running() {
    var scheduler = new TestScheduler();
    var etw = new FakeEtwSource(isRunning: false, startError: "not elevated");
    var source = Create(etw, scheduler);

    ProcessNetworkSnapshot? result = null;
    using var _ = source.TopTalkers.Subscribe(s => result = s);
    scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

    Assert.NotNull(result);
    Assert.False(result!.IsRunning);
    Assert.Equal("not elevated", result.StatusError);
    Assert.Empty(result.TopTalkers);
  }
}
