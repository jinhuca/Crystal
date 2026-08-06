using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Sensors;

/// <summary>
/// Coarse grouping a <see cref="Crystal.Infrastructure.DataStructures.Sensors.SensorReading"/>
/// falls into. Folds the three vendor-specific GPU hardware types into a single
/// <see cref="Gpu"/> bucket and maps everything else onto its hardware category.
/// </summary>
public enum SensorCategory {
  Cpu,
  Gpu,
  Memory,
  Motherboard,
  Storage,
  Network,
  Cooler,
  Battery,
  Psu,
  PowerMonitor,
  Other,
}

public static class SensorCategoryExtensions {
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
