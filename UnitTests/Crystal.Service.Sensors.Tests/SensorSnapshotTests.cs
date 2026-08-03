using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Service.Sensors.Tests;

public class SensorSnapshotTests {
  [Fact]
  public void Groups_readings_by_category() {
    var readings = new[] {
      FakeSensorTelemetrySource.Reading(HardwareType.Cpu, "CPU Package"),
      FakeSensorTelemetrySource.Reading(HardwareType.Cpu, "CPU Total", SensorType.Load),
      FakeSensorTelemetrySource.Reading(HardwareType.GpuNvidia, "GPU Core"),
      FakeSensorTelemetrySource.Reading(HardwareType.Memory, "Used Memory", SensorType.Data),
    };

    var snapshot = new SensorSnapshot(readings);

    Assert.Equal(2, snapshot.Cpu.Count);
    Assert.Single(snapshot.Gpu);
    Assert.Single(snapshot.Memory);
    Assert.Equal(4, snapshot.Readings.Count);
  }

  [Fact]
  public void Folds_all_gpu_vendors_into_single_gpu_group() {
    var readings = new[] {
      FakeSensorTelemetrySource.Reading(HardwareType.GpuNvidia, "GPU Core"),
      FakeSensorTelemetrySource.Reading(HardwareType.GpuAmd, "GPU Core"),
      FakeSensorTelemetrySource.Reading(HardwareType.GpuIntel, "GPU Core"),
    };

    var snapshot = new SensorSnapshot(readings);

    Assert.Equal(3, snapshot.Gpu.Count);
    Assert.Equal(3, snapshot[SensorCategory.Gpu].Count);
  }

  [Fact]
  public void Motherboard_category_absorbs_superio_and_ec() {
    var readings = new[] {
      FakeSensorTelemetrySource.Reading(HardwareType.Motherboard, "Board"),
      FakeSensorTelemetrySource.Reading(HardwareType.SuperIO, "Fan #1", SensorType.Fan),
      FakeSensorTelemetrySource.Reading(HardwareType.EmbeddedController, "EC Temp"),
    };

    var snapshot = new SensorSnapshot(readings);

    Assert.Equal(3, snapshot.Motherboard.Count);
  }

  [Fact]
  public void Missing_category_returns_empty_not_null() {
    var snapshot = new SensorSnapshot(Array.Empty<SensorReading>());

    Assert.NotNull(snapshot.Cpu);
    Assert.Empty(snapshot.Cpu);
    Assert.Empty(snapshot[SensorCategory.Storage]);
    Assert.Empty(snapshot.Readings);
  }

  [Fact]
  public void Null_readings_treated_as_empty() {
    var snapshot = new SensorSnapshot(null!);

    Assert.Empty(snapshot.Readings);
    Assert.Empty(snapshot.ByCategory);
  }
}
