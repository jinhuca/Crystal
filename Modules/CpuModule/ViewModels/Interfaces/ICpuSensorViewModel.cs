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

  /// <summary>Hands the view model the four history graphs it should feed on each update.</summary>
  void AttachGraphs(PerformanceGraph utilization, PerformanceGraph voltage,
                    PerformanceGraph clock, PerformanceGraph power, PerformanceGraph temperature);

  /// <summary>Reads the socket's live sensors and pushes samples into the attached graphs.</summary>
  void Update(ISystemCpuInfo info);
}
