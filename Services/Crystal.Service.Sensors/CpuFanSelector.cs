using System;
using System.Collections.Generic;
using System.Linq;
using Crystal.Infrastructure.DataStructures.Sensors;

namespace Crystal.Service.Sensors;

/// <summary>
/// Picks the CPU fan's RPM out of a <see cref="SensorSnapshot"/>. CPU hardware emits no fan
/// sensors; fans come from the motherboard's SuperIO chip (or an attached cooler) and are told
/// apart only by their name string (e.g. "CPU Fan", "CPU Fan #1" vs. "Chassis Fan"). So the CPU
/// fan is identified primarily by a name heuristic over the Motherboard/Cooler fan readings, with
/// a fallback for boards whose SuperIO tables name every header generically ("Fan #1", "Fan #2").
/// </summary>
public static class CpuFanSelector {
  /// <summary>
  /// Returns the RPM of the CPU fan in <paramref name="snapshot"/>.
  /// <para>
  /// Prefers a fan whose name identifies it as a CPU fan; when several match (e.g. "CPU Fan #1" /
  /// "#2") the highest reading wins — the spinning fan over an idle/unpopulated header reading 0.
  /// </para>
  /// <para>
  /// When no fan is named for the CPU (some boards report generic "Fan #1"/"Fan #2"), falls back to
  /// the highest-RPM spinning fan on the motherboard/cooler — the CPU fan is almost always the
  /// fastest header — so the readout still works. Returns null only when no fan is spinning at all.
  /// </para>
  /// </summary>
  public static float? SelectRpm(SensorSnapshot snapshot) {
    if (snapshot is null) return null;

    float? named = null;
    float? fastestSpinning = null;
    foreach (var reading in FanReadings(snapshot)) {
      if (reading.Value is not { } rpm) continue;
      if (IsCpuFan(reading.SensorName)) {
        if (named is null || rpm > named) named = rpm;
      }
      if (rpm > 0 && (fastestSpinning is null || rpm > fastestSpinning)) fastestSpinning = rpm;
    }
    return named ?? fastestSpinning;
  }

  /// <summary>
  /// Returns the CPU fan speed as a percentage (PWM duty) in <paramref name="snapshot"/>, or null
  /// when no fan control is reported.
  /// <para>
  /// Laptops (typically HP/Lenovo) expose no fan tachometer; their fan sits behind the ACPI
  /// embedded controller and reports only a duty percentage (see the NBFC-backed embedded-controller
  /// path), surfaced as a <see cref="SensorType.Control"/> reading. This is the fallback readout when
  /// <see cref="SelectRpm"/> finds no RPM. Prefers a CPU-named control; otherwise the highest-duty
  /// control (the active fan over an idle secondary header).
  /// </para>
  /// </summary>
  public static float? SelectPercent(SensorSnapshot snapshot) {
    if (snapshot is null) return null;

    float? named = null;
    float? highest = null;
    foreach (var reading in FanControlReadings(snapshot)) {
      if (reading.Value is not { } pct) continue;
      if (IsCpuFan(reading.SensorName)) {
        if (named is null || pct > named) named = pct;
      }
      if (highest is null || pct > highest) highest = pct;
    }
    return named ?? highest;
  }

  private static IEnumerable<SensorReading> FanReadings(SensorSnapshot snapshot) =>
      snapshot[SensorCategory.Motherboard]
          .Concat(snapshot[SensorCategory.Cooler])
          .Where(r => r.SensorType == SensorType.Fan);

  private static IEnumerable<SensorReading> FanControlReadings(SensorSnapshot snapshot) =>
      snapshot[SensorCategory.Motherboard]
          .Concat(snapshot[SensorCategory.Cooler])
          .Where(r => r.SensorType == SensorType.Control);

  // The SuperIO chip tables label the CPU header "CPU Fan" / "CPU Fan #n"; a chassis/system fan
  // never contains "CPU". A plain substring match is enough to separate the two.
  private static bool IsCpuFan(string? sensorName) =>
      sensorName is not null && sensorName.Contains("CPU", StringComparison.OrdinalIgnoreCase);
}
