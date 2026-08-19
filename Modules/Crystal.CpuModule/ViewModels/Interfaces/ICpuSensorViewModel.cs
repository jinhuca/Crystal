using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.Models;
using Crystal.CpuModule.ViewModels;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using System.Collections.ObjectModel;

namespace Crystal.CpuModule.ViewModels.Interfaces;

/// <summary>
/// Live CPU readings driving the gauges (Load / Voltage / Speed / Power / Temperature)
/// and their history graphs. Refreshed on every sensor-stream emission.
/// <para>
/// The graphs are ring buffers owned by <see cref="PerformanceGraph"/>, so the view
/// exposes those control instances directly and the view model pushes samples into them
/// via <see cref="AttachGraph"/> — a value-plus-a-graph pairing that a plain bound
/// property can't express.
/// </para>
/// </summary>
public interface ICpuSensorViewModel {
  /// <summary>
  /// Current CPU load percentage.
  /// </summary>
  double Load { get; }

  /// <summary>
  /// Current core voltage in V. Zero when not exposed.
  /// </summary>
  double Voltage { get; }

  /// <summary>
  /// SoC/uncore voltage rail (AMD), distinct from core <see cref="Voltage"/>. Zero when not exposed.
  /// </summary>
  double SocVoltage { get; }

  double SpeedGhz { get; }

  /// <summary>
  /// C-state-weighted effective clock in GHz; lower than <see cref="SpeedGhz"/> when cores idle. Zero when not exposed.
  /// </summary>
  double EffectiveSpeedGhz { get; }

  /// <summary>
  /// Reference/base clock (BCLK) in MHz — the ~100 MHz bus the core multiplier scales. Zero when not exposed.
  /// </summary>
  double BusSpeedMHz { get; }

  /// <summary>
  /// Current package power in W. Zero when not exposed.
  /// </summary>
  double Power { get; }

  /// <summary>
  /// Configured sustained package power limit (PL1) in W. Intel-only; zero when not exposed.
  /// </summary>
  double PowerLimitLongW { get; }

  /// <summary>
  /// Configured burst package power limit (PL2) in W. Intel-only; zero when not exposed.
  /// </summary>
  double PowerLimitShortW { get; }

  /// <summary>
  /// Thermal Design Current (TDC) in A. AMD-only; zero when not exposed.
  /// </summary>
  double TdcAmps { get; }

  /// <summary>
  /// Electrical Design Current (EDC) in A. AMD-only; zero when not exposed.
  /// </summary>
  double EdcAmps { get; }

  /// <summary>
  /// Package C2 idle residency as a percentage of the last poll. Zero when not exposed.
  /// </summary>
  double PackageC2Pct { get; }

  /// <summary>
  /// Package C3 idle residency as a percentage of the last poll. Zero when not exposed.
  /// </summary>
  double PackageC3Pct { get; }

  /// <summary>
  /// Package C6 idle residency as a percentage of the last poll. Zero when not exposed.
  /// </summary>
  double PackageC6Pct { get; }

  /// <summary>
  /// Package C7 idle residency as a percentage of the last poll. Zero when not exposed.
  /// </summary>
  double PackageC7Pct { get; }

  /// <summary>
  /// Hottest core's temperature in °C. Zero when not exposed.
  /// </summary>
  double Temperature { get; }

  /// <summary>
  /// Hottest core's headroom to TjMax in °C (lower = closer to throttling). Intel-only;
  /// zero when not exposed.
  /// </summary>
  double DistanceToTjMax { get; }

  /// <summary>
  /// True when the package is currently throttling for any reason (thermal / power-limit /
  /// PROCHOT). Falls back to a thermal-headroom check when the provider flags are unavailable.
  /// </summary>
  bool IsThrottling { get; }

  /// <summary>
  /// Human-readable throttle reason(s), e.g. "THROTTLING: Thermal, Power Limit"; empty when
  /// not throttling.
  /// </summary>
  string ThrottleStatus { get; }

  /// <summary>
  /// CPU fan speed in RPM, sourced from the motherboard SuperIO fan headers by name.
  /// </summary>
  int FanRpm { get; }

  /// <summary>
  /// True once a CPU fan tachometer reading has been seen; drives whether the RPM readout shows.
  /// </summary>
  bool HasCpuFan { get; }

  /// <summary>
  /// CPU fan speed as a percentage (PWM duty), used for laptops whose fan sits behind the
  /// embedded controller and reports no tachometer. Sourced via an NBFC config; see the embedded-controller path.
  /// </summary>
  double FanPercent { get; }

  /// <summary>
  /// True once a CPU fan percentage reading has been seen; the RPM readout takes precedence when both exist.
  /// </summary>
  bool HasCpuFanPercent { get; }

  /// <summary>
  /// True once any MSR-backed reading (voltage, power, temperature, clock) has produced a
  /// value. These come from the ring-0 PawnIO driver; when it is absent (or the process is
  /// unelevated) they stay empty, so this remains false and the view surfaces a notice.
  /// </summary>
  bool MsrSensorsAvailable { get; }

  /// <summary>
  /// Per-physical-core load, one entry per core on the first socket. Rows are created
  /// once (on the first emission that reports cores) and updated in place thereafter.
  /// </summary>
  ObservableCollection<CoreLoadViewModel> CoreLoads { get; }

  /// <summary>
  /// Registers a history graph to be fed on each update, keyed by its <c>GraphIdentity.Id</c>
  /// (e.g. "Cpu.Utilization"). Each metric sub-view self-registers its own graph on load, so the
  /// view model feeds only the graphs a given consumer chose to realize.
  /// </summary>
  void AttachGraph(string id, PerformanceGraph graph);

  /// <summary>
  /// Reads the socket's live sensors and pushes samples into the attached graphs.
  /// </summary>
  void Update(ISystemCpuInfo info);

  /// <summary>
  /// Updates the CPU fan RPM readout. Null means no CPU fan tachometer was detected this poll.
  /// </summary>
  void UpdateFan(float? rpm);

  /// <summary>
  /// Updates the CPU fan percentage readout. Null means no fan control was detected this poll.
  /// </summary>
  void UpdateFanPercent(float? percent);
}
