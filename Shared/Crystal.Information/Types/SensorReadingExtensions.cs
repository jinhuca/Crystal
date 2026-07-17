using Crystal.Telemetry.Hardware;
namespace Crystal.Information.Types;


public static class SensorReadingExtensions {
  public static string? UnitFor(SensorType sensorType) => sensorType switch {
    SensorType.Voltage => "V",
    SensorType.Clock => "MHz",
    SensorType.Temperature => "°C",
    SensorType.Load => "%",
    SensorType.Power => "W",
    SensorType.Fan => "RPM",
    SensorType.Flow => "L/h",
    SensorType.Control => "%",
    SensorType.Level => "%",
    SensorType.Factor => string.Empty,
    SensorType.Data => "GB",
    SensorType.SmallData => "MB",
    SensorType.Throughput => "B/s",
    SensorType.Frequency => "Hz",
    SensorType.Energy => "mWh",
    SensorType.Current => "A",
    SensorType.Humidity => "%",
    _ => null
  };

  public static SensorReading ToReading(ISensor? sensor, string hardwareName, HardwareType hardwareType) =>
    new SensorReading(
      hardwareName,
      hardwareType,
      sensor?.Name ?? string.Empty,
      sensor?.SensorType ?? SensorType.Load,
      sensor?.Value,
      sensor?.Min,
      sensor?.Max,
      sensor != null ? UnitFor(sensor.SensorType) : null
    );
}
