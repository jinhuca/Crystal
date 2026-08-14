using Microsoft.Reactive.Testing;
using System.Reactive.Linq;
using Xunit;

namespace Crystal.Service.Gpu.Tests;

public class GpuMonitorTests {
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  private static GpuInfoBuilder Builder(FakeGpuLoadSource loads) =>
      new(new FakeWmiHardwareProvider([VideoRows.Controller(name: "NVIDIA GeForce RTX 4070")]), loads);

  [Fact]
  public async Task Specs_emits_the_built_snapshot() {
    using var monitor = new GpuMonitor(Builder(new FakeGpuLoadSource()));

    var snap = await monitor.Specs.FirstAsync();

    Assert.Equal("NVIDIA GeForce RTX 4070", Assert.Single(snap.Adapters).Name);
  }

  [Fact]
  public async Task Specs_replays_same_snapshot_to_late_subscribers() {
    using var monitor = new GpuMonitor(Builder(new FakeGpuLoadSource()));

    var first = await monitor.Specs.FirstAsync();
    var second = await monitor.Specs.FirstAsync();

    // Replay(1) caches the single build result; both subscribers see the same instance.
    Assert.Same(first, second);
  }

  [Fact]
  public void Sensors_polls_the_builder_once_per_interval_while_subscribed() {
    var scheduler = new TestScheduler();
    var loads = new FakeGpuLoadSource();
    // The eager Specs build reads once up front; count only what the Sensors poll adds.
    using var monitor = new GpuMonitor(Builder(loads), Interval, scheduler);
    var baseline = loads.ReadCount;

    using var _ = monitor.Sensors.Subscribe();
    scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);

    Assert.Equal(3, loads.ReadCount - baseline);
  }

  [Fact]
  public void Sensors_is_cold_until_subscribed() {
    var scheduler = new TestScheduler();
    var loads = new FakeGpuLoadSource();
    using var monitor = new GpuMonitor(Builder(loads), Interval, scheduler);
    var baseline = loads.ReadCount;

    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

    Assert.Equal(0, loads.ReadCount - baseline);
  }

  [Fact]
  public void Ctor_throws_on_null_builder() =>
      Assert.Throws<ArgumentNullException>(() => new GpuMonitor(null!));
}
