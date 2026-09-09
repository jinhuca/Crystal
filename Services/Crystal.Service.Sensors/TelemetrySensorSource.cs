using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Sensors;

/// <summary>
/// Reads live board sensors from the Telemetry provider (a LibreHardwareMonitor fork) and projects
/// them onto the neutral <see cref="SensorReading"/> type.
/// <para>
/// Only the Motherboard (SuperIO + embedded controller) and Controller (liquid-cooler) groups are
/// enabled: the shared <see cref="SensorMonitor"/> that drives this source is consumed solely by the
/// BIOS board tile and the CPU-fan readout, both of which read only the Motherboard and Cooler
/// categories. CPU/GPU/Memory/Storage/Network each have their own dedicated telemetry session, so
/// enabling them here would re-enumerate and re-poll the same hardware every second for data nobody
/// reads off this stream — wasted work whose co-scheduled bus I/O widens the window in which our
/// once-per-second embedded-controller read overlaps external EC access, amplifying the ACPI EC
/// "race condition possible" trace. Keep the tree minimal so the EC read is the poll's only bus work.
/// </para>
/// <para>
/// Board temperatures, voltages and fan RPM come from the SuperIO/EC via the ring-0 driver, so they
/// read as empty unless the process is elevated with the PawnIO driver installed.
/// </para>
/// </summary>
public sealed class TelemetrySensorSource : ISensorTelemetrySource {
  private readonly Computer _computer;
  private bool _disposed;

  public TelemetrySensorSource() {
    _computer = new Computer {
      IsMotherboardEnabled = true,
      IsControllerEnabled = true,
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
