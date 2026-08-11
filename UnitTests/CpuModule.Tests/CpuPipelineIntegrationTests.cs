using System.Collections.Frozen;
using System.Reactive.Linq;
using CpuModule.Models;
using CpuModule.ViewModels.Implementations;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cores;
using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;
using Crystal.Service.Cpu;
using Crystal.Service.Sensors;
using Microsoft.Reactive.Testing;
using Xunit;

namespace CpuModule.Tests;

// End-to-end tests over the real CPU pipeline: fake providers -> real CpuInfoBuilder -> real
// CpuMonitor (driven by a TestScheduler) -> real CpuModel -> real module view models, wired the
// same way CpuViewModel wires them. Unlike CpuMonitorTests (which stops at the monitor) and the
// per-VM tests (which feed pre-built trees), these exercise the whole service->module seam: the
// builder correlating provider rows, the monitor's replay/refcount cadence, and the VM's unit
// conversions all run for real. The VMs are driven synchronously (the test runs on the STA/UI
// thread, so UiThreadMarshaller executes inline) so no dispatcher pumping is needed.
public class CpuPipelineIntegrationTests {
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  private static readonly CpuIdRawData Cpuid =
      new("Test CPU", "TestVendor", 6, 1, 2, 3600, 100, 8, 16, true, true, null, null);

  // A telemetry source whose per-poll readings are supplied by a caller-controlled callback, so a
  // test can make successive polls return changing values (mirroring live re-sampling).
  private sealed class ScriptedTelemetry : ICpuTelemetrySource {
    private readonly Func<int, ICpuSensors> _sensors;
    private readonly Func<int, IReadOnlyList<ICoreInfo>> _cores;
    public int RefreshCount { get; private set; }

    public ScriptedTelemetry(Func<int, ICpuSensors> sensors, Func<int, IReadOnlyList<ICoreInfo>>? cores = null) {
      _sensors = sensors;
      _cores = cores ?? (_ => []);
    }

    public void Refresh() => RefreshCount++;
    public ICpuSensors GetSensors(int socketIndex) => _sensors(socketIndex);
    public IReadOnlyList<ICoreInfo> GetCores(int socketIndex) => _cores(socketIndex);
    public void Dispose() { }
  }

  private static SensorReading CpuReading(SensorType type, float? value) =>
      new(string.Empty, HardwareType.Cpu, string.Empty, type, value, null, null, null);

  private static ICpuSensors Sensors(float? load = null, float? voltage = null,
                                      float? speedMHz = null, float? power = null, float? temp = null) =>
      new CpuSensors {
        TotalLoad = CpuReading(SensorType.Load, load),
        Voltage = CpuReading(SensorType.Voltage, voltage),
        CpuSpeed = CpuReading(SensorType.Clock, speedMHz),
        PackagePower = CpuReading(SensorType.Power, power),
        PackageTemperature = CpuReading(SensorType.Temperature, temp),
      };

  private static CpuInfoBuilder Builder(ICpuTelemetrySource telemetry) =>
      new(new FakeCpuIdProvider(Cpuid),
          new FakeSmbiosProcessorProvider(
              [new SmbiosProcessorInfo("CPU0", MaxSpeedMHz: 4200, ExternalClockMHz: 100,
                                       LogicalCoreCount: 8, CacheInfo: null)]),
          new FakeWmiHardwareProvider([FakeWmiHardwareProvider.ProcessorRow("CPU0", 16, 8)]),
          new CpuSpecsResolver(),
          telemetry);

  [Fact]
  public void Specs_flow_from_providers_through_the_model_into_the_specs_view_model() {
    var telemetry = new ScriptedTelemetry(_ => Sensors());
    using var monitor = new CpuMonitor(Builder(telemetry));
    using var model = new CpuModel(monitor);
    var specsVm = new CpuSpecsViewModel();

    // Wire the VM to the model's specs stream the way CpuViewModel does.
    using var sub = model.Specs.Subscribe(specsVm.Update);

    // Specs are eager + Replay(1); the subscription above receives the already-built snapshot.
    Assert.Equal("TestVendor", specsVm.Vendor);
    Assert.Equal(8, specsVm.PhysicalCores);
    Assert.Equal(16, specsVm.LogicalCores);
  }

  [Fact]
  public void Live_sensor_readings_reach_the_sensor_view_model_with_mhz_to_ghz_conversion() {
    // Each poll returns a fresh reading; the second poll bumps clock + load.
    int poll = 0;
    var telemetry = new ScriptedTelemetry(_ => {
      poll++;
      return poll == 1
          ? Sensors(load: 20, voltage: 1.1f, speedMHz: 3600, power: 45, temp: 55)
          : Sensors(load: 90, voltage: 1.3f, speedMHz: 4200, power: 95, temp: 72);
    });
    // Specs build performs the first BuildAsync (poll==1); reset so the poll stream starts clean.
    var scheduler = new TestScheduler();
    using var monitor = new CpuMonitor(Builder(telemetry), Interval, scheduler);
    using var model = new CpuModel(monitor);
    var sensorsVm = new CpuSensorsViewModel();
    using var sub = model.Sensors.Subscribe(sensorsVm.Update);

    scheduler.AdvanceBy(Interval.Ticks);      // first poll after the eager specs build
    scheduler.AdvanceBy(Interval.Ticks);      // second poll

    // The VM reflects the most recent poll, with MHz->GHz applied by the VM layer.
    Assert.Equal(90, sensorsVm.Load);
    Assert.Equal(1.3, sensorsVm.Voltage, 3);
    Assert.Equal(4.2, sensorsVm.SpeedGhz, 3);   // 4200 MHz / 1000
    Assert.Equal(95, sensorsVm.Power);
    Assert.Equal(72, sensorsVm.Temperature);
    Assert.True(sensorsVm.MsrSensorsAvailable);
  }

  [Fact]
  public void Core_loads_propagate_end_to_end_and_refresh_in_place() {
    IReadOnlyList<ICoreInfo> Cores(float l0, float l1) => [
      new CoreInfo(new CoreSpecs(), new CoreSensors { Load = CpuReading(SensorType.Load, l0) }),
      new CoreInfo(new CoreSpecs(), new CoreSensors { Load = CpuReading(SensorType.Load, l1) }),
    ];
    int poll = 0;
    var telemetry = new ScriptedTelemetry(
        _ => Sensors(load: 50),
        _ => { poll++; return poll <= 1 ? Cores(10, 20) : Cores(80, 90); });
    var scheduler = new TestScheduler();
    using var monitor = new CpuMonitor(Builder(telemetry), Interval, scheduler);
    using var model = new CpuModel(monitor);
    var sensorsVm = new CpuSensorsViewModel();
    using var sub = model.Sensors.Subscribe(sensorsVm.Update);

    scheduler.AdvanceBy(Interval.Ticks);
    var firstRow = sensorsVm.CoreLoads[0];
    Assert.Equal(2, sensorsVm.CoreLoads.Count);
    Assert.Equal("C00", firstRow.Label);

    scheduler.AdvanceBy(Interval.Ticks);

    // Rows are updated in place, not rebuilt: same count, same instance, new values.
    Assert.Equal(2, sensorsVm.CoreLoads.Count);
    Assert.Same(firstRow, sensorsVm.CoreLoads[0]);
    Assert.Equal(80, sensorsVm.CoreLoads[0].Load);
    Assert.Equal(90, sensorsVm.CoreLoads[1].Load);
  }

  [Fact]
  public void The_sensor_poll_is_cold_until_the_view_model_subscribes() {
    var telemetry = new ScriptedTelemetry(_ => Sensors(load: 10));
    var scheduler = new TestScheduler();
    using var monitor = new CpuMonitor(Builder(telemetry), Interval, scheduler);
    using var model = new CpuModel(monitor);
    var sensorsVm = new CpuSensorsViewModel();
    int refreshAfterSpecs = telemetry.RefreshCount; // the eager specs build already refreshed once

    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks); // no subscriber yet -> no polling

    Assert.Equal(refreshAfterSpecs, telemetry.RefreshCount);
    Assert.Equal(0, sensorsVm.Load); // VM never updated

    using var sub = model.Sensors.Subscribe(sensorsVm.Update);
    scheduler.AdvanceBy(Interval.Ticks);

    Assert.True(telemetry.RefreshCount > refreshAfterSpecs);
    Assert.Equal(10, sensorsVm.Load);
  }

  [Fact]
  public void Msr_availability_stays_latched_once_a_reading_arrives_then_goes_empty() {
    int poll = 0;
    var telemetry = new ScriptedTelemetry(_ => {
      poll++;
      // The eager specs build consumes poll #1; the first sensor poll is poll #2. Both carry an
      // MSR-backed voltage, later polls report none (driver hiccup) so the latch can be exercised.
      return poll <= 2 ? Sensors(load: 30, voltage: 1.2f) : Sensors(load: 30);
    });
    var scheduler = new TestScheduler();
    using var monitor = new CpuMonitor(Builder(telemetry), Interval, scheduler);
    using var model = new CpuModel(monitor);
    var sensorsVm = new CpuSensorsViewModel();
    using var sub = model.Sensors.Subscribe(sensorsVm.Update);

    scheduler.AdvanceBy(Interval.Ticks);
    Assert.True(sensorsVm.MsrSensorsAvailable);

    scheduler.AdvanceBy(Interval.Ticks); // voltage now null

    // The latch must hold so the "MSR unavailable" notice doesn't flicker back on.
    Assert.True(sensorsVm.MsrSensorsAvailable);
  }

  // ---- Fan chain: SensorMonitor -> CpuFanMonitor -> sensor VM ----

  private sealed class FakeSensorSource : ISensorTelemetrySource {
    private readonly Func<IReadOnlyList<SensorReading>> _read;
    public FakeSensorSource(Func<IReadOnlyList<SensorReading>> read) => _read = read;
    public IReadOnlyList<SensorReading> Read() => _read();
    public void Dispose() { }
  }

  private static SensorReading Fan(string name, float rpm) =>
      new(name, HardwareType.Motherboard, string.Empty, SensorType.Fan, rpm, null, null, null);

  [Fact]
  public void Cpu_fan_rpm_flows_from_the_sensor_monitor_through_the_fan_monitor_into_the_vm() {
    var source = new FakeSensorSource(() => [Fan("CPU Fan", 1200), Fan("Chassis Fan", 800)]);
    var scheduler = new TestScheduler();
    using var sensorMonitor = new SensorMonitor(source, Interval, scheduler);
    var fanMonitor = new CpuFanMonitor(sensorMonitor);
    var sensorsVm = new CpuSensorsViewModel();
    using var sub = fanMonitor.Rpm.Subscribe(sensorsVm.UpdateFan);

    scheduler.AdvanceBy(Interval.Ticks);

    // CpuFanSelector picks "CPU Fan" over "Chassis Fan"; the VM latches HasCpuFan and truncates RPM.
    Assert.True(sensorsVm.HasCpuFan);
    Assert.Equal(1200, sensorsVm.FanRpm);
  }
}
