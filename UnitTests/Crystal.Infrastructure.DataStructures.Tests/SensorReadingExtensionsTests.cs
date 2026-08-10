using Crystal.Infrastructure.DataStructures.Sensors;
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
}
