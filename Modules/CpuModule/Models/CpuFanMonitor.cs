using System.Reactive.Linq;
using Crystal.Service.Sensors;

namespace CpuModule.Models;

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

  public CpuFanMonitor(SensorMonitor monitor) {
    ArgumentNullException.ThrowIfNull(monitor);
    _rpm = monitor.Snapshots.Select(CpuFanSelector.SelectRpm);
  }

  /// <summary>CPU fan RPM on each poll; null when no CPU fan is detected.</summary>
  public IObservable<float?> Rpm => _rpm;
}
