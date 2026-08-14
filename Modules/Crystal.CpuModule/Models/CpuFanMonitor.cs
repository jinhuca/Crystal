using System.Reactive.Linq;
using Crystal.Service.Sensors;

namespace Crystal.CpuModule.Models;

/// <summary>
/// Projects the CPU fan's RPM out of the shared system-wide <see cref="SensorMonitor"/> stream.
/// The CPU service exposes no fan (CPU hardware emits none); the fan lives on the motherboard's
/// SuperIO chip, so this taps the general sensor snapshot and picks the CPU fan by name via
/// <see cref="CpuFanSelector"/>. Emits null when no CPU fan is present or readable.
/// <para>
/// A thin projection over <see cref="SensorMonitor"/>: it adds no timer of its own and inherits
/// that monitor's cold, ref-counted 1-second cadence.
/// </para>
/// </summary>
public sealed class CpuFanMonitor {
  private readonly IObservable<float?> _rpm;
  private readonly IObservable<float?> _percent;

  public CpuFanMonitor(SensorMonitor monitor) {
    ArgumentNullException.ThrowIfNull(monitor);
    _rpm = monitor.Snapshots.Select(CpuFanSelector.SelectRpm);
    _percent = monitor.Snapshots.Select(CpuFanSelector.SelectPercent);
  }

  /// <summary>CPU fan RPM on each poll; null when no CPU fan tachometer is detected.</summary>
  public IObservable<float?> Rpm => _rpm;

  /// <summary>CPU fan speed as a percentage on each poll; null when no fan control is detected.
  /// The fallback readout for laptops that expose fan duty (via the embedded controller) but no RPM.</summary>
  public IObservable<float?> Percent => _percent;
}
