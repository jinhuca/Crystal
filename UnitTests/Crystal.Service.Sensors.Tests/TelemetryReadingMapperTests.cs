using Xunit;
using AppHardwareType = Crystal.Infrastructure.DataStructures.Sensors.HardwareType;
using AppSensorType = Crystal.Infrastructure.DataStructures.Sensors.SensorType;
using ProviderHardwareType = Crystal.Provider.Telemetry.Hardware.HardwareType;
using ProviderSensorType = Crystal.Provider.Telemetry.Hardware.SensorType;

namespace Crystal.Service.Sensors.Tests;

public class TelemetryReadingMapperTests {
  [Fact]
  public void ToReading_NullSensor_ProducesEmptyNameAndLoadDefault() {
    var reading = TelemetryReadingMapper.ToReading(null, "CPU", ProviderHardwareType.Cpu);

    Assert.Equal("CPU", reading.HardwareName);
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

    var reading = TelemetryReadingMapper.ToReading(sensor, "CPU", ProviderHardwareType.Cpu);

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

    var reading = TelemetryReadingMapper.ToReading(sensor, "GPU", ProviderHardwareType.GpuNvidia);

    Assert.Equal(AppSensorType.TimeSpan, reading.SensorType);
    Assert.Equal(AppHardwareType.GpuNvidia, reading.HardwareType);
    Assert.Null(reading.Unit);
  }

  // The Infrastructure enums mirror the provider's with identical member order; the mapper relies on
  // that for its ordinal cast. Guard it so a divergent add on either side is caught here.
  [Theory]
  [InlineData(ProviderSensorType.Voltage, AppSensorType.Voltage)]
  [InlineData(ProviderSensorType.Load, AppSensorType.Load)]
  [InlineData(ProviderSensorType.Humidity, AppSensorType.Humidity)]
  public void ToReading_MapsProviderSensorTypeOntoMatchingAppType(ProviderSensorType provider, AppSensorType expected) {
    var reading = TelemetryReadingMapper.ToReading(
        new StubSensor { SensorType = provider }, "HW", ProviderHardwareType.Motherboard);
    Assert.Equal(expected, reading.SensorType);
  }

  [Theory]
  [InlineData(ProviderHardwareType.Motherboard, AppHardwareType.Motherboard)]
  [InlineData(ProviderHardwareType.GpuIntel, AppHardwareType.GpuIntel)]
  [InlineData(ProviderHardwareType.PowerMonitor, AppHardwareType.PowerMonitor)]
  public void ToReading_MapsProviderHardwareTypeOntoMatchingAppType(ProviderHardwareType provider, AppHardwareType expected) {
    var reading = TelemetryReadingMapper.ToReading(null, "HW", provider);
    Assert.Equal(expected, reading.HardwareType);
  }
}
