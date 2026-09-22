using Crystal.Provider.Telemetry.Hardware;
using AppHardwareType = Crystal.Infrastructure.DataStructures.Sensors.HardwareType;
using AppSensorType = Crystal.Infrastructure.DataStructures.Sensors.SensorType;
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
  /// <summary>
  /// Converts one provider <paramref name="sensor"/> into a neutral <see cref="SensorReading"/>. The
  /// provider and Infrastructure enums share member order, so the type conversion is an ordinal cast.
  /// A null sensor represents hardware present but with no readable value: it maps to a
  /// <see cref="AppSensorType.Load"/> reading with null value/min/max and no unit, so the hardware
  /// still appears in the snapshot.
  /// </summary>
  /// <param name="sensor">The provider sensor, or null for a value-less placeholder reading.</param>
  /// <param name="hardwareName">The owning hardware's display name, carried onto the reading.</param>
  /// <param name="hardwareType">The provider hardware type, ordinally cast to the Infrastructure copy.</param>
  /// <returns>The projected neutral reading.</returns>
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
