using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Infrastructure.DataStructures.Tests;

public class SensorReadingExtensionsTests {
  [Theory]
  [InlineData(SensorType.Voltage, "V")]
  [InlineData(SensorType.Clock, "MHz")]
  [InlineData(SensorType.Temperature, "°C")]
  [InlineData(SensorType.Load, "%")]
  [InlineData(SensorType.Power, "W")]
  [InlineData(SensorType.Fan, "RPM")]
  [InlineData(SensorType.Flow, "L/h")]
  [InlineData(SensorType.Control, "%")]
  [InlineData(SensorType.Level, "%")]
  [InlineData(SensorType.Factor, "")]
  [InlineData(SensorType.Data, "GB")]
  [InlineData(SensorType.SmallData, "MB")]
  [InlineData(SensorType.Throughput, "B/s")]
  [InlineData(SensorType.Frequency, "Hz")]
  [InlineData(SensorType.Energy, "mWh")]
  [InlineData(SensorType.Current, "A")]
  [InlineData(SensorType.Humidity, "%")]
  public void UnitFor_KnownSensorType_ReturnsExpectedUnit(SensorType type, string expected) =>
    Assert.Equal(expected, SensorReadingExtensions.UnitFor(type));

  [Fact]
  public void UnitFor_UnmappedSensorType_ReturnsNull() =>
    // TimeSpan isn't in the switch; the default arm returns null.
    Assert.Null(SensorReadingExtensions.UnitFor(SensorType.TimeSpan));

  [Fact]
  public void ToReading_NullSensor_ProducesEmptyNameAndLoadDefault() {
    var reading = SensorReadingExtensions.ToReading(null, "CPU", HardwareType.Cpu);

    Assert.Equal("CPU", reading.HardwareName);
    Assert.Equal(HardwareType.Cpu, reading.HardwareType);
    Assert.Equal(string.Empty, reading.SensorName);
    Assert.Equal(SensorType.Load, reading.SensorType);
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
      SensorType = SensorType.Temperature,
      Value = 55.5f,
      Min = 30f,
      Max = 80f,
    };

    var reading = SensorReadingExtensions.ToReading(sensor, "CPU", HardwareType.Cpu);

    Assert.Equal("Core #1", reading.SensorName);
    Assert.Equal(SensorType.Temperature, reading.SensorType);
    Assert.Equal(55.5f, reading.Value);
    Assert.Equal(30f, reading.Min);
    Assert.Equal(80f, reading.Max);
    Assert.Equal("°C", reading.Unit);
  }

  [Fact]
  public void ToReading_SensorWithUnmappedType_HasNullUnitButKeepsType() {
    var sensor = new StubSensor { SensorType = SensorType.TimeSpan };

    var reading = SensorReadingExtensions.ToReading(sensor, "GPU", HardwareType.GpuNvidia);

    Assert.Equal(SensorType.TimeSpan, reading.SensorType);
    Assert.Null(reading.Unit);
  }
}
