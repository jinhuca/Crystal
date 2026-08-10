namespace Crystal.Infrastructure.DataStructures.Sensors;

/// <summary>
/// Application-neutral hardware category. Mirrors the telemetry provider's own hardware-type
/// enumeration so the provider (a LibreHardwareMonitor fork) stays a standalone package with no
/// dependency on this layer; the telemetry-aware service maps provider values onto these at the
/// boundary. Member names are kept identical to the provider's so the mapping is 1:1.
/// </summary>
public enum HardwareType {
  /// <summary>The main circuit board of the computer, connecting all other components.</summary>
  Motherboard,

  /// <summary>
  /// A Super I/O chip on the motherboard responsible for low-speed peripheral
  /// interfaces such as fan control, voltage monitoring, and temperature sensors.
  /// </summary>
  SuperIO,

  /// <summary>The central processing unit (CPU), the primary processor of the computer.</summary>
  Cpu,

  /// <summary>System RAM (random-access memory) installed in the computer.</summary>
  Memory,

  /// <summary>A graphics processing unit (GPU) manufactured by NVIDIA.</summary>
  GpuNvidia,

  /// <summary>A graphics processing unit (GPU) manufactured by AMD.</summary>
  GpuAmd,

  /// <summary>
  /// A graphics processing unit (GPU) manufactured by Intel,
  /// including integrated and discrete Intel Arc GPUs.
  /// </summary>
  GpuIntel,

  /// <summary>A storage device such as an HDD, SSD, or NVMe drive.</summary>
  Storage,

  /// <summary>A network interface card (NIC) or other network adapter.</summary>
  Network,

  /// <summary>A cooling device such as a liquid cooling controller or fan hub.</summary>
  Cooler,

  /// <summary>
  /// An embedded controller (EC), typically found in laptops, responsible for
  /// power management, thermal regulation, and keyboard input.
  /// </summary>
  EmbeddedController,

  /// <summary>A power supply unit (PSU) with monitoring capabilities.</summary>
  Psu,

  /// <summary>A battery, typically found in laptops or UPS devices.</summary>
  Battery,

  /// <summary>A hardware power monitoring device that measures system power consumption.</summary>
  PowerMonitor,
}
