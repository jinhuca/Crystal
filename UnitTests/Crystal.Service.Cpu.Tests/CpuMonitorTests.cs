using Crystal.Provider.CpuId;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;
using Microsoft.Reactive.Testing;
using System.Reactive.Linq;
using Xunit;

namespace Crystal.Service.Cpu.Tests;

public class CpuMonitorTests {
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  private static readonly CpuIdRawData Cpuid =
      new("Test CPU", "TestVendor", 6, 1, 2, 3600, 100, 8, 16, true, true, null, null);

  private static CpuInfoBuilder Builder(FakeCpuTelemetrySource telemetry) =>
      new(new FakeCpuIdProvider(Cpuid),
          new FakeSmbiosProcessorProvider(
              [new SmbiosProcessorInfo("CPU0", MaxSpeedMHz: 4200, ExternalClockMHz: 100,
                                       LogicalCoreCount: 8, CacheInfo: null)]),
          new FakeWmiHardwareProvider([FakeWmiHardwareProvider.ProcessorRow("CPU0", 16, 8)]),
          new CpuSpecsResolver(),
          telemetry);

  [Fact]
  public async Task Specs_emits_the_built_snapshot() {
    using var monitor = new CpuMonitor(Builder(new FakeCpuTelemetrySource()));

    var info = await monitor.Specs.FirstAsync();

    Assert.Equal("CPU0", Assert.Single(info.Sockets).SocketDesignation);
  }

  [Fact]
  public async Task Specs_replays_same_snapshot_to_late_subscribers() {
    using var monitor = new CpuMonitor(Builder(new FakeCpuTelemetrySource()));

    var first = await monitor.Specs.FirstAsync();
    var second = await monitor.Specs.FirstAsync();

    // Replay(1) caches the single build result; both subscribers see the same instance.
    Assert.Same(first, second);
  }

  [Fact]
  public void Sensors_rebuilds_once_per_interval_while_subscribed() {
    var scheduler = new TestScheduler();
    var telemetry = new FakeCpuTelemetrySource();
    using var monitor = new CpuMonitor(Builder(telemetry), Interval, scheduler);
    // The eager Specs build already read the one socket once; count only what the poll adds.
    telemetry.RequestedSensorIndices.Clear();

    using var _ = monitor.Sensors.Subscribe();
    scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);

    // One socket rebuilt on each of 3 poll ticks → 3 GetSensors calls.
    Assert.Equal(3, telemetry.RequestedSensorIndices.Count);
  }

  [Fact]
  public void Sensors_is_cold_until_subscribed() {
    var scheduler = new TestScheduler();
    var telemetry = new FakeCpuTelemetrySource();
    using var monitor = new CpuMonitor(Builder(telemetry), Interval, scheduler);
    telemetry.RequestedSensorIndices.Clear();

    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

    // No poll fired, so the sensor stream never asked the telemetry source for a socket.
    Assert.Empty(telemetry.RequestedSensorIndices);
  }

  [Fact]
  public void Ctor_throws_on_null_builder() =>
      Assert.Throws<ArgumentNullException>(() => new CpuMonitor(null!));
}
