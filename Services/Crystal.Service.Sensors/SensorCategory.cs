using Crystal.Infrastructure.DataStructures.Sensors;

namespace Crystal.Service.Sensors;

/// <summary>
/// Coarse grouping a <see cref="Crystal.Infrastructure.DataStructures.Sensors.SensorReading"/>
/// falls into. Folds the three vendor-specific GPU hardware types into a single
/// <see cref="Gpu"/> bucket and maps everything else onto its hardware category.
/// </summary>
public enum SensorCategory {
  /// <summary>
  /// Processor package sensors (temperatures, clocks, load, power).
  /// </summary>
  Cpu,
  /// <summary>
  /// Graphics adapter sensors, merged from the NVIDIA/AMD/Intel hardware types.
  /// </summary>
  Gpu,
  /// <summary>
  /// System RAM sensors (used/available memory, load).
  /// </summary>
  Memory,
  /// <summary>
  /// Motherboard sensors, including the SuperIO chip and embedded controller (board temps, voltages, fans).
  /// </summary>
  Motherboard,
  /// <summary>
  /// Drive sensors (temperature, activity, wear).
  /// </summary>
  Storage,
  /// <summary>
  /// Network adapter sensors (throughput, utilization).
  /// </summary>
  Network,
  /// <summary>
  /// Liquid-cooler / fan-controller sensors.
  /// </summary>
  Cooler,
  /// <summary>
  /// Battery sensors (charge level, rate, capacity).
  /// </summary>
  Battery,
  /// <summary>
  /// Power-supply-unit sensors (rail voltages, efficiency, output).
  /// </summary>
  Psu,
  /// <summary>
  /// External power-monitor device sensors.
  /// </summary>
  PowerMonitor,
  /// <summary>
  /// Anything not mapped to a specific category above.
  /// </summary>
  Other,
}

/// <summary>
/// Maps a provider <see cref="HardwareType"/> onto the coarser <see cref="SensorCategory"/>
/// buckets the UI groups by.
/// </summary>
public static class SensorCategoryExtensions {
  /// <summary>
  /// Folds a <see cref="HardwareType"/> into its <see cref="SensorCategory"/>. The three vendor GPU
  /// types collapse into <see cref="SensorCategory.Gpu"/>, and SuperIO / embedded-controller hardware
  /// joins <see cref="SensorCategory.Motherboard"/>; unrecognized types fall through to
  /// <see cref="SensorCategory.Other"/>.
  /// </summary>
  /// <param name="hardwareType">The provider hardware type to classify.</param>
  /// <returns>The category the hardware type belongs to.</returns>
  public static SensorCategory ToCategory(this HardwareType hardwareType) => hardwareType switch {
    HardwareType.Cpu => SensorCategory.Cpu,
    HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel => SensorCategory.Gpu,
    HardwareType.Memory => SensorCategory.Memory,
    HardwareType.Motherboard or HardwareType.SuperIO or HardwareType.EmbeddedController => SensorCategory.Motherboard,
    HardwareType.Storage => SensorCategory.Storage,
    HardwareType.Network => SensorCategory.Network,
    HardwareType.Cooler => SensorCategory.Cooler,
    HardwareType.Battery => SensorCategory.Battery,
    HardwareType.Psu => SensorCategory.Psu,
    HardwareType.PowerMonitor => SensorCategory.PowerMonitor,
    _ => SensorCategory.Other,
  };
}
