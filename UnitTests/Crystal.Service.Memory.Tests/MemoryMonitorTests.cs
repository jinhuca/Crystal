using Microsoft.Reactive.Testing;
using System.Reactive.Linq;
using Xunit;

namespace Crystal.Service.Memory.Tests;

public class MemoryMonitorTests {
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  private static MemoryInfoBuilder Builder() =>
      new(new FakeWmiHardwareProvider(
          sticks: [MemoryRows.Stick(deviceLocator: "DIMM A", capacityBytes: 16UL * 1024 * 1024 * 1024)],
          arrays: [MemoryRows.Array(memoryDevices: 4)]));

  [Fact]
  public async Task Specs_emits_the_built_snapshot() {
    using var monitor = new MemoryMonitor(Builder(), new FakeMemoryLoadSource());

    var snap = await monitor.Specs.FirstAsync();

    Assert.Equal("DIMM A", Assert.Single(snap.Modules).SlotLabel);
  }

  [Fact]
  public async Task Specs_replays_same_snapshot_to_late_subscribers() {
    using var monitor = new MemoryMonitor(Builder(), new FakeMemoryLoadSource());

    var first = await monitor.Specs.FirstAsync();
    var second = await monitor.Specs.FirstAsync();

    // Replay(1) caches the single build result; both subscribers see the same instance.
    Assert.Same(first, second);
  }

  [Fact]
  public void Load_polls_once_per_interval_while_subscribed() {
    var scheduler = new TestScheduler();
    var loads = new FakeMemoryLoadSource(new MemoryLoadReading(42, UsedGB: 8, AvailableGB: 8));
    using var monitor = new MemoryMonitor(Builder(), loads, Interval, scheduler);

    var received = new List<MemoryLoadReading>();
    using var _ = monitor.Load.Subscribe(received.Add);
    scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);

    Assert.Equal(3, received.Count);
    Assert.Equal(42, received[0].LoadPercent);
    Assert.Equal(3, loads.ReadCount);
  }

  [Fact]
  public void Load_is_cold_until_subscribed() {
    var scheduler = new TestScheduler();
    var loads = new FakeMemoryLoadSource();
    using var monitor = new MemoryMonitor(Builder(), loads, Interval, scheduler);

    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

    Assert.Equal(0, loads.ReadCount);
  }

  [Fact]
  public void Load_shares_one_poll_across_concurrent_subscribers() {
    var scheduler = new TestScheduler();
    var loads = new FakeMemoryLoadSource();
    using var monitor = new MemoryMonitor(Builder(), loads, Interval, scheduler);

    using var a = monitor.Load.Subscribe();
    using var b = monitor.Load.Subscribe();
    scheduler.AdvanceBy(TimeSpan.FromSeconds(2).Ticks);

    // Publish().RefCount() means both subscribers share one poll per interval, not one each.
    Assert.Equal(2, loads.ReadCount);
  }

  [Fact]
  public void Ctor_throws_on_null_builder() =>
      Assert.Throws<ArgumentNullException>(() => new MemoryMonitor(null!, new FakeMemoryLoadSource()));

  [Fact]
  public void Ctor_throws_on_null_loads() =>
      Assert.Throws<ArgumentNullException>(() => new MemoryMonitor(Builder(), null!));
}
