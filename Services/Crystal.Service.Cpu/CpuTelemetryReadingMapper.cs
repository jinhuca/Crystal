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
  /// <summary>
  /// Projects a single provider <see cref="ISensor"/> onto a neutral <see cref="SensorReading"/>, carrying
  /// its value/min/max plus the derived display unit. A <see langword="null"/> <paramref name="sensor"/>
  /// (no matching hardware sensor found) yields an empty reading tagged as a load sensor with no unit, so
  /// callers always receive a well-formed row rather than having to null-check.
  /// </summary>
  /// <param name="sensor">The provider sensor to convert, or <see langword="null"/> when none matched.</param>
  /// <param name="hardwareName">The owning hardware's display name, stamped onto the reading.</param>
  /// <param name="hardwareType">The provider hardware type, ordinally cast to the neutral enum.</param>
  /// <returns>A neutral sensor reading; empty (no value/unit) when <paramref name="sensor"/> is null.</returns>
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

  /// <summary>
  /// Maps the provider's hybrid-topology <see cref="CoreType"/> onto the neutral app enum, translating
  /// performance ("P") and efficient ("E") cores and falling back to <see cref="AppCoreType.Unknown"/>
  /// for any value the provider does not classify.
  /// </summary>
  /// <param name="coreType">The provider-reported core type.</param>
  /// <returns>The equivalent neutral core type.</returns>
  public static AppCoreType ToAppCoreType(CoreType coreType) => coreType switch {
    CoreType.Performance => AppCoreType.Performance,
    CoreType.Efficient => AppCoreType.Efficient,
    _ => AppCoreType.Unknown,
  };
}
