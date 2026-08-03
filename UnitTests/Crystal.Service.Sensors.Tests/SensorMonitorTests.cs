using System.Reactive.Linq;
using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.Telemetry.Hardware;
using Microsoft.Reactive.Testing;
using Xunit;

namespace Crystal.Service.Sensors.Tests;

public class SensorMonitorTests {
  private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

  [Fact]
  public void Emits_a_snapshot_on_each_poll_interval() {
    var scheduler = new TestScheduler();
    var source = new FakeSensorTelemetrySource(new[] {
      FakeSensorTelemetrySource.Reading(HardwareType.Cpu, "CPU Package"),
    });
    using var monitor = new SensorMonitor(source, OneSecond, scheduler);

    var received = new List<SensorSnapshot>();
    using var subscription = monitor.Snapshots.Subscribe(received.Add);

    scheduler.AdvanceBy(OneSecond.Ticks * 3);

    Assert.Equal(3, received.Count);
    Assert.All(received, s => Assert.Single(s.Cpu));
  }

  [Fact]
  public void Does_not_poll_until_subscribed() {
    var scheduler = new TestScheduler();
    var source = new FakeSensorTelemetrySource(Array.Empty<SensorReading>());
    using var monitor = new SensorMonitor(source, OneSecond, scheduler);

    scheduler.AdvanceBy(OneSecond.Ticks * 5);

    Assert.Equal(0, source.ReadCount);
  }

  [Fact]
  public void Stops_polling_after_last_unsubscribe() {
    var scheduler = new TestScheduler();
    var source = new FakeSensorTelemetrySource(Array.Empty<SensorReading>());
    using var monitor = new SensorMonitor(source, OneSecond, scheduler);

    var subscription = monitor.Snapshots.Subscribe(_ => { });
    scheduler.AdvanceBy(OneSecond.Ticks * 2);
    subscription.Dispose();

    var countAfterUnsubscribe = source.ReadCount;
    scheduler.AdvanceBy(OneSecond.Ticks * 5);

    Assert.Equal(2, countAfterUnsubscribe);
    Assert.Equal(countAfterUnsubscribe, source.ReadCount);
  }

  [Fact]
  public void Each_snapshot_reflects_freshly_sampled_values() {
    var scheduler = new TestScheduler();
    var source = new FakeSensorTelemetrySource(poll => new[] {
      FakeSensorTelemetrySource.Reading(HardwareType.Cpu, "CPU Package", value: poll),
    });
    using var monitor = new SensorMonitor(source, OneSecond, scheduler);

    var received = new List<SensorSnapshot>();
    using var subscription = monitor.Snapshots.Subscribe(received.Add);
    scheduler.AdvanceBy(OneSecond.Ticks * 3);

    Assert.Equal(new float?[] { 0f, 1f, 2f },
                 received.Select(s => s.Cpu[0].Value).ToArray());
  }

  [Fact]
  public void Dispose_disposes_the_underlying_source() {
    var source = new FakeSensorTelemetrySource(Array.Empty<SensorReading>());
    var monitor = new SensorMonitor(source, OneSecond, new TestScheduler());

    monitor.Dispose();

    Assert.True(source.Disposed);
  }

  [Fact]
  public void Constructor_rejects_null_source() {
    Assert.Throws<ArgumentNullException>(() => new SensorMonitor(null!));
  }
}
