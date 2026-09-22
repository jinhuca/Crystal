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
  /// <summary>
  /// The open hardware session; only the motherboard and controller trees are enabled.
  /// </summary>
  private readonly Computer _computer;

  /// <summary>
  /// Guards against closing the hardware session more than once.
  /// </summary>
  private bool _disposed;

  /// <summary>
  /// Opens the hardware session with only the motherboard (SuperIO + embedded controller) and
  /// controller (cooler) trees enabled, keeping bus work per poll to the minimum this source's
  /// consumers actually read. See the type remarks for why the tree is deliberately kept minimal.
  /// </summary>
  public TelemetrySensorSource() {
    _computer = new Computer {
      IsMotherboardEnabled = true,
      IsControllerEnabled = true,
    };
    _computer.Open();
  }

  /// <summary>
  /// Refreshes every enabled hardware node (and its sub-hardware) and returns a flat snapshot of the
  /// current sensor readings, projected onto the neutral <see cref="SensorReading"/>. Called once per
  /// poll by the owning <see cref="SensorMonitor"/>.
  /// </summary>
  /// <returns>All readings collected this poll, ungrouped.</returns>
  public IReadOnlyList<SensorReading> Read() {
    var readings = new List<SensorReading>();

    foreach (var hardware in _computer.Hardware) {
      hardware.Update();
      Collect(hardware, readings);
    }

    return readings;
  }

  /// <summary>
  /// Appends every sensor on <paramref name="hardware"/> to <paramref name="readings"/>, then recurses
  /// into its sub-hardware (updating each first) so nested nodes — e.g. SuperIO chips hung off the
  /// motherboard — are included. The caller must have already called <c>Update()</c> on the top node.
  /// </summary>
  /// <param name="hardware">The hardware node to harvest.</param>
  /// <param name="readings">The accumulator the readings are appended to.</param>
  private static void Collect(IHardware hardware, List<SensorReading> readings) {
    foreach (var sensor in hardware.Sensors)
      readings.Add(TelemetryReadingMapper.ToReading(sensor, hardware.Name, hardware.HardwareType));

    foreach (var sub in hardware.SubHardware) {
      sub.Update();
      Collect(sub, readings);
    }
  }

  /// <summary>
  /// Closes the hardware session, releasing the ring-0 driver handle. Idempotent.
  /// </summary>
  public void Dispose() {
    if (_disposed) return;
    _disposed = true;
    _computer.Close();
  }
}
