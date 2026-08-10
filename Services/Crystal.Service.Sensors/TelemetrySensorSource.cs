using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Sensors;

/// <summary>
/// Reads live sensors for every enabled hardware category from the Telemetry
/// provider (a LibreHardwareMonitor fork) and projects them onto the neutral
/// <see cref="SensorReading"/> type.
/// <para>
/// Temperature, clock, voltage and power for CPU/GPU come from MSRs via the
/// ring-0 driver, so they read as empty unless the process is elevated.
/// </para>
/// </summary>
public sealed class TelemetrySensorSource : ISensorTelemetrySource {
  private readonly Computer _computer;
  private bool _disposed;

  public TelemetrySensorSource() {
    _computer = new Computer {
      IsCpuEnabled = true,
      IsGpuEnabled = true,
      IsMemoryEnabled = true,
      IsMotherboardEnabled = true,
      IsStorageEnabled = true,
      IsNetworkEnabled = true,
      IsControllerEnabled = true,
      IsBatteryEnabled = true,
      IsPsuEnabled = true,
    };
    _computer.Open();
  }

  public IReadOnlyList<SensorReading> Read() {
    var readings = new List<SensorReading>();

    foreach (var hardware in _computer.Hardware) {
      hardware.Update();
      Collect(hardware, readings);
    }

    return readings;
  }

  private static void Collect(IHardware hardware, List<SensorReading> readings) {
    foreach (var sensor in hardware.Sensors)
      readings.Add(TelemetryReadingMapper.ToReading(sensor, hardware.Name, hardware.HardwareType));

    foreach (var sub in hardware.SubHardware) {
      sub.Update();
      Collect(sub, readings);
    }
  }

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
