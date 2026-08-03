using Crystal.Controls.PerformanceGraphs;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

namespace CpuModule.ViewModels.Interfaces;

/// <summary>
/// Live CPU readings driving the gauges (Load / Voltage / Speed / Power / Temperature)
/// and their history graphs. Refreshed on every sensor-stream emission.
/// <para>
/// The graphs are ring buffers owned by <see cref="PerformanceGraph"/>, so the view
/// exposes those control instances directly and the view model pushes samples into them
/// via <see cref="AttachGraphs"/> — a value-plus-a-graph pairing that a plain bound
/// property can't express.
/// </para>
/// </summary>
public interface ICpuSensorViewModel {
  double Load { get; }
  double Voltage { get; }
  double SpeedGhz { get; }
  double Power { get; }
  double Temperature { get; }

  /// <summary>
  /// True once any MSR-backed reading (voltage, power, temperature, clock) has produced a
  /// value. These come from the ring-0 PawnIO driver; when it is absent (or the process is
  /// unelevated) they stay empty, so this remains false and the view surfaces a notice.
  /// </summary>
  bool MsrSensorsAvailable { get; }

  /// <summary>
  /// Hands the view model the history graphs it should feed on each update. Every graph is
  /// optional so a compact consumer (e.g. the dashboard summary tile) can attach only the
  /// utilization plot while the full detail view attaches all five.
  /// </summary>
  void AttachGraphs(PerformanceGraph? utilization = null, PerformanceGraph? voltage = null,
                    PerformanceGraph? clock = null, PerformanceGraph? power = null,
                    PerformanceGraph? temperature = null);

  /// <summary>Reads the socket's live sensors and pushes samples into the attached graphs.</summary>
  void Update(ISystemCpuInfo info);
}
