using Crystal.Provider.Mmi.MmiEngine;
using Microsoft.Reactive.Testing;
using System.Collections.Frozen;
using System.Reactive.Linq;
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

  private const ulong GB = 1024UL * 1024 * 1024;
  private static readonly FrozenDictionary<string, WmiValue> Samsung =
      DiskRow.Drive(model: "Samsung SSD 990 PRO", sizeBytes: 500 * GB, index: 0);
  private static readonly FrozenDictionary<string, WmiValue> WdBlue =
      DiskRow.Drive(model: "WD Blue", sizeBytes: 2000 * GB, index: 1);

  [Fact]
  public async Task Plugging_in_a_drive_reemits_the_inventory_and_rescans_the_load_source() {
    var wmi = new MutableWmiHardwareProvider([Samsung]);
    var loads = new FakeStorageLoadSource();
    using var monitor = new StorageMonitor(
        new StorageInfoBuilder(wmi), loads, inventoryInterval: TimeSpan.FromMilliseconds(20));

    var first = await monitor.Specs.FirstAsync();
    Assert.Single(first.Drives);
    Assert.Equal(0, loads.RefreshCount); // initial inventory doesn't count as a hotplug

    // Subscribe before the change so Skip(1) skips only the replayed initial snapshot, then plug in.
    var gate = new SemaphoreSlim(0);
    StorageSnapshot? changed = null;
    using var sub = monitor.Specs.Skip(1).Subscribe(s => { changed = s; gate.Release(); });
    wmi.Set([Samsung, WdBlue]);

    Assert.True(await gate.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    Assert.Equal(2, changed!.Drives.Count);
    Assert.Equal(1, loads.RefreshCount); // the changed drive set re-scanned live hardware
  }

  [Fact]
  public async Task Removing_a_drive_reemits_the_inventory_and_rescans_the_load_source() {
    var wmi = new MutableWmiHardwareProvider([Samsung, WdBlue]);
    var loads = new FakeStorageLoadSource();
    using var monitor = new StorageMonitor(
        new StorageInfoBuilder(wmi), loads, inventoryInterval: TimeSpan.FromMilliseconds(20));

    var first = await monitor.Specs.FirstAsync();
    Assert.Equal(2, first.Drives.Count);

    var gate = new SemaphoreSlim(0);
    StorageSnapshot? changed = null;
    using var sub = monitor.Specs.Skip(1).Subscribe(s => { changed = s; gate.Release(); });
    wmi.Set([Samsung]); // pull the second drive

    Assert.True(await gate.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    Assert.Equal("Samsung SSD 990 PRO", Assert.Single(changed!.Drives).Model);
    Assert.Equal(1, loads.RefreshCount);
  }

  [Fact]
  public void Ctor_throws_on_null_builder() =>
      Assert.Throws<ArgumentNullException>(() => new StorageMonitor(null!, new FakeStorageLoadSource()));

  [Fact]
  public void Ctor_throws_on_null_loads() =>
      Assert.Throws<ArgumentNullException>(() => new StorageMonitor(Builder(), null!));
}
