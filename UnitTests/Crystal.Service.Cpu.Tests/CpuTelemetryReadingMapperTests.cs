using Xunit;
using ProviderCoreType = Crystal.Provider.Telemetry.Hardware.CoreType;
using ProviderHardwareType = Crystal.Provider.Telemetry.Hardware.HardwareType;
using ProviderSensorType = Crystal.Provider.Telemetry.Hardware.SensorType;
using AppCoreType = Crystal.Infrastructure.DataStructures.Cpu.Definitions.CoreType;
using AppHardwareType = Crystal.Infrastructure.DataStructures.Sensors.HardwareType;
using AppSensorType = Crystal.Infrastructure.DataStructures.Sensors.SensorType;

namespace Crystal.Service.Cpu.Tests;

public class CpuTelemetryReadingMapperTests {
  [Fact]
  public void ToReading_NullSensor_ProducesEmptyNameAndLoadDefault() {
    var reading = CpuTelemetryReadingMapper.ToReading(null, "CPU0", ProviderHardwareType.Cpu);

    Assert.Equal("CPU0", reading.HardwareName);
    Assert.Equal(AppHardwareType.Cpu, reading.HardwareType);
    Assert.Equal(string.Empty, reading.SensorName);
    Assert.Equal(AppSensorType.Load, reading.SensorType);
    Assert.Null(reading.Value);
    Assert.Null(reading.Min);
    Assert.Null(reading.Max);
    // A null sensor short-circuits the unit lookup entirely.
    Assert.Null(reading.Unit);
  }

  [Fact]
  public void ToReading_PopulatedSensor_CopiesValuesAndResolvesUnit() {
    var sensor = new StubSensor {
      Name = "Core #1",
      SensorType = ProviderSensorType.Temperature,
      Value = 55.5f,
      Min = 30f,
      Max = 80f,
    };

    var reading = CpuTelemetryReadingMapper.ToReading(sensor, "CPU0", ProviderHardwareType.Cpu);

    Assert.Equal("Core #1", reading.SensorName);
    Assert.Equal(AppSensorType.Temperature, reading.SensorType);
    Assert.Equal(55.5f, reading.Value);
    Assert.Equal(30f, reading.Min);
    Assert.Equal(80f, reading.Max);
    Assert.Equal("°C", reading.Unit);
  }

  [Fact]
  public void ToReading_SensorWithUnmappedType_HasNullUnitButKeepsType() {
    var sensor = new StubSensor { SensorType = ProviderSensorType.TimeSpan };

    var reading = CpuTelemetryReadingMapper.ToReading(sensor, "CPU0", ProviderHardwareType.Cpu);

    Assert.Equal(AppSensorType.TimeSpan, reading.SensorType);
    Assert.Null(reading.Unit);
  }

  // The Infrastructure enums mirror the provider's with identical member order; the mapper relies on
  // that for its ordinal cast. Guard it so a divergent add on either side is caught here.
  [Theory]
  [InlineData(ProviderSensorType.Voltage, AppSensorType.Voltage)]
  [InlineData(ProviderSensorType.Clock, AppSensorType.Clock)]
  [InlineData(ProviderSensorType.Load, AppSensorType.Load)]
  [InlineData(ProviderSensorType.Power, AppSensorType.Power)]
  // TDC/EDC (Current), package C-state residency (Level) and throttle flags (Factor) rely on this
  // same ordinal cast, so guard their alignment too.
  [InlineData(ProviderSensorType.Current, AppSensorType.Current)]
  [InlineData(ProviderSensorType.Level, AppSensorType.Level)]
  [InlineData(ProviderSensorType.Factor, AppSensorType.Factor)]
  public void ToReading_MapsProviderSensorTypeOntoMatchingAppType(ProviderSensorType provider, AppSensorType expected) {
    var reading = CpuTelemetryReadingMapper.ToReading(
        new StubSensor { SensorType = provider }, "CPU0", ProviderHardwareType.Cpu);
    Assert.Equal(expected, reading.SensorType);
  }

  [Theory]
  // The units the new CPU sensors surface: current in amps, C-state residency as a percentage,
  // and the dimensionless throttle factor.
  [InlineData(ProviderSensorType.Current, "A")]
  [InlineData(ProviderSensorType.Level, "%")]
  [InlineData(ProviderSensorType.Factor, "")]
  public void ToReading_ResolvesUnitForNewCpuSensorTypes(ProviderSensorType provider, string expectedUnit) {
    var reading = CpuTelemetryReadingMapper.ToReading(
        new StubSensor { SensorType = provider, Value = 1f }, "CPU0", ProviderHardwareType.Cpu);
    Assert.Equal(expectedUnit, reading.Unit);
  }

  [Theory]
  [InlineData(ProviderCoreType.Performance, AppCoreType.Performance)]
  [InlineData(ProviderCoreType.Efficient, AppCoreType.Efficient)]
  [InlineData(ProviderCoreType.Unknown, AppCoreType.Unknown)]
  public void ToAppCoreType_MapsEachProviderCoreClass(ProviderCoreType provider, AppCoreType expected) =>
      Assert.Equal(expected, CpuTelemetryReadingMapper.ToAppCoreType(provider));
}
