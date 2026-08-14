using Crystal.Provider.Telemetry.Hardware;
using AppCoreType = Crystal.Infrastructure.DataStructures.Cpu.Definitions.CoreType;
using AppHardwareType = Crystal.Infrastructure.DataStructures.Sensors.HardwareType;
using AppSensorType = Crystal.Infrastructure.DataStructures.Sensors.SensorType;
using SensorReading = Crystal.Infrastructure.DataStructures.Sensors.SensorReading;
using SensorReadingExtensions = Crystal.Infrastructure.DataStructures.Sensors.SensorReadingExtensions;

namespace Crystal.Service.Cpu;

/// <summary>
/// Boundary adapter that projects provider telemetry types onto the neutral Infrastructure types.
/// The provider enums are mirrored by Infrastructure copies with identical member order, so the
/// conversions are ordinal casts; this keeps the Telemetry package standalone (it never references
/// Infrastructure).
/// </summary>
internal static class CpuTelemetryReadingMapper {
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

  public static AppCoreType ToAppCoreType(CoreType coreType) => coreType switch {
    CoreType.Performance => AppCoreType.Performance,
    CoreType.Efficient => AppCoreType.Efficient,
    _ => AppCoreType.Unknown,
  };
}
