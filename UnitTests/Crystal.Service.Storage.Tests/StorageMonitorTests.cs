using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Xunit;

namespace Crystal.Service.Storage.Tests;

public class StorageMonitorTests {
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  private static StorageInfoBuilder Builder() =>
      new(new FakeWmiHardwareProvider(
          [DiskRow.Drive(model: "Samsung SSD 990 PRO", sizeBytes: 500_107_862_016, index: 0)]));

  [Fact]
  public async Task Specs_emits_the_built_snapshot() {
    using var monitor = new StorageMonitor(Builder(), new FakeStorageLoadSource());

    var snap = await monitor.Specs.FirstAsync();

    Assert.Equal("Samsung SSD 990 PRO", Assert.Single(snap.Drives).Model);
  }

  [Fact]
  public async Task Specs_replays_same_snapshot_to_late_subscribers() {
    using var monitor = new StorageMonitor(Builder(), new FakeStorageLoadSource());

    var first = await monitor.Specs.FirstAsync();
    var second = await monitor.Specs.FirstAsync();

    // Replay(1) caches the single build result; both subscribers see the same instance.
    Assert.Same(first, second);
  }

  [Fact]
  public void Load_polls_once_per_interval_while_subscribed() {
    var scheduler = new TestScheduler();
    var loads = new FakeStorageLoadSource(new StorageLoadReading(
        [new StorageDiskLoad(DriveIndex: 0, ActivityPercent: 12, ReadRateMBps: 100, WriteRateMBps: 50, ResponseMs: 1.2)]));
    using var monitor = new StorageMonitor(Builder(), loads, Interval, scheduler);

    var received = new List<StorageLoadReading>();
    using var _ = monitor.Load.Subscribe(received.Add);
    scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);

    Assert.Equal(3, received.Count);
    Assert.Equal(12, received[0].Disks[0].ActivityPercent);
    Assert.Equal(3, loads.ReadCount);
  }

  [Fact]
  public void Load_is_cold_until_subscribed() {
    var scheduler = new TestScheduler();
    var loads = new FakeStorageLoadSource();
    using var monitor = new StorageMonitor(Builder(), loads, Interval, scheduler);

    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

    Assert.Equal(0, loads.ReadCount);
  }

  [Fact]
  public void Load_shares_one_poll_across_concurrent_subscribers() {
    var scheduler = new TestScheduler();
    var loads = new FakeStorageLoadSource();
    using var monitor = new StorageMonitor(Builder(), loads, Interval, scheduler);

    using var a = monitor.Load.Subscribe();
    using var b = monitor.Load.Subscribe();
    scheduler.AdvanceBy(TimeSpan.FromSeconds(2).Ticks);

    // Publish().RefCount() means both subscribers share one poll per interval, not one each.
    Assert.Equal(2, loads.ReadCount);
  }

  [Fact]
  public void Ctor_throws_on_null_builder() =>
      Assert.Throws<ArgumentNullException>(() => new StorageMonitor(null!, new FakeStorageLoadSource()));

  [Fact]
  public void Ctor_throws_on_null_loads() =>
      Assert.Throws<ArgumentNullException>(() => new StorageMonitor(Builder(), null!));
}
