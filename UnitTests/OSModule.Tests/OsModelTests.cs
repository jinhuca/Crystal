using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using OSModule.Models;
using Xunit;

namespace OSModule.Tests;

public class OsModelTests {
  [Fact]
  public void Info_replays_the_built_snapshot_to_every_subscriber() {
    using var model = new OsModel(new OsInfoBuilder());

    OsSnapshot? first = null;
    OsSnapshot? second = null;
    using (model.Info.Subscribe(s => first = s))
    using (model.Info.Subscribe(s => second = s)) { }

    // Built once and replayed: both subscribers see the same (non-null) identity snapshot.
    Assert.NotNull(first);
    Assert.NotNull(second);
    Assert.Same(first, second);
  }

  [Fact]
  public void Live_emits_immediately_then_on_each_interval_tick() {
    var scheduler = new TestScheduler();
    var clock = new DateTimeOffset(2024, 8, 1, 0, 0, 0, TimeSpan.Zero);
    using var model = new OsModel(new OsInfoBuilder(),
        pollInterval: TimeSpan.FromSeconds(1), scheduler: scheduler, clock: () => clock);

    var readings = new List<OsLiveReading>();
    using var sub = model.Live.Subscribe(readings.Add);

    // StartWith gives one reading synchronously, before the scheduler advances.
    Assert.Single(readings);

    scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);

    // One initial + three interval ticks.
    Assert.Equal(4, readings.Count);
  }

  [Fact]
  public void Live_reading_stamps_now_from_the_injected_clock() {
    var scheduler = new TestScheduler();
    var now = new DateTimeOffset(2024, 8, 1, 12, 0, 0, TimeSpan.Zero);
    using var model = new OsModel(new OsInfoBuilder(),
        pollInterval: TimeSpan.FromSeconds(1), scheduler: scheduler, clock: () => now);

    OsLiveReading? reading = null;
    using var sub = model.Live.Subscribe(r => reading = r);

    Assert.NotNull(reading);
    Assert.Equal(now, reading!.Now);
  }

  [Fact]
  public void Live_uptime_falls_back_to_tick_count_from_a_real_clock() {
    var scheduler = new TestScheduler();
    // With the system clock the boot instant (now - ticks) precedes now, so uptime is non-negative.
    using var model = new OsModel(new OsInfoBuilder(),
        pollInterval: TimeSpan.FromSeconds(1), scheduler: scheduler, clock: () => DateTimeOffset.Now);

    OsLiveReading? reading = null;
    using var sub = model.Live.Subscribe(r => reading = r);

    Assert.NotNull(reading);
    Assert.True(reading!.Uptime >= TimeSpan.Zero);
  }
}
