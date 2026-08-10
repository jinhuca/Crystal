using Crystal.Provider.Telemetry.Hardware;
using AppSensorType = Crystal.Infrastructure.DataStructures.Sensors.SensorType;
using AppHardwareType = Crystal.Infrastructure.DataStructures.Sensors.HardwareType;
using SensorReading = Crystal.Infrastructure.DataStructures.Sensors.SensorReading;
using SensorReadingExtensions = Crystal.Infrastructure.DataStructures.Sensors.SensorReadingExtensions;

namespace Crystal.Service.Sensors;

/// <summary>
/// Boundary adapter that projects a provider <see cref="ISensor"/> onto the neutral
/// <see cref="SensorReading"/>. The provider's <see cref="SensorType"/>/<see cref="HardwareType"/>
/// are mirrored by the Infrastructure copies with identical member order, so the conversion is an
/// ordinal cast; this keeps the Telemetry package standalone (it never references Infrastructure).
/// </summary>
internal static class TelemetryReadingMapper {
  public static SensorReading ToReading(ISensor? sensor, string hardwareName, HardwareType hardwareType) {
    var appType = sensor is null ? AppSensorType.Load : (AppSensorType)(int)sensor.SensorType;
    return new SensorReading(
        hardwareName,
        (AppHardwareType)(int)hardwareType,
        sensor?.Name ?? string.Empty,
        appType,
        sensor?.Value,
        sensor?.Min,
        sensor?.Max,
        sensor is null ? null : SensorReadingExtensions.UnitFor(appType));
  }
}
