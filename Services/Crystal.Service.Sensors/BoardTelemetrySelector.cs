using System;
using System.Collections.Generic;
using System.Linq;
using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Sensors;

/// <summary>A voltage-rail reading with the running min/max the sensor has observed this session,
/// so the tile can hint at rail stability alongside the current value.</summary>
public sealed record RailReading(float? Value, float? Min, float? Max) {
  public static RailReading None { get; } = new(null, null, null);
}

/// <summary>Board-level live telemetry the BIOS tile headlines, picked out of a
/// <see cref="SensorSnapshot"/>'s <see cref="SensorCategory.Motherboard"/> readings.</summary>
public sealed record BoardTelemetry(
    float? BoardTemperature,
    float? CmosVoltage,
    float? ChassisFanRpm,
    RailReading Rail3V3,
    RailReading Rail5V,
    RailReading Rail12V) {
  public static BoardTelemetry Empty { get; } =
      new(null, null, null, RailReading.None, RailReading.None, RailReading.None);
}

/// <summary>
/// Picks the handful of board readings the BIOS summary shows out of a <see cref="SensorSnapshot"/>.
/// The motherboard's SuperIO/EC chip labels sensors only by name string, so each field is found by
/// a name heuristic over the <see cref="SensorCategory.Motherboard"/> readings of the right
/// <see cref="SensorType"/>. Any field is null when no matching sensor is present or readable.
/// </summary>
public static class BoardTelemetrySelector {
  public static BoardTelemetry Select(SensorSnapshot snapshot) {
    if (snapshot is null) return BoardTelemetry.Empty;

    var board = snapshot[SensorCategory.Motherboard];
    return new BoardTelemetry(
        BoardTemperature: BoardTemp(board),
        CmosVoltage: Cmos(board),
        ChassisFanRpm: ChassisFan(board),
        Rail3V3: Rail(board, "3.3"),
        Rail5V: Rail(board, "5"),
        Rail12V: Rail(board, "12"));
  }

  // Prefer a sensor named for the system/board; fall back to the first board temperature.
  private static float? BoardTemp(IReadOnlyList<SensorReading> board) {
    var temps = board.Where(r => r.SensorType == SensorType.Temperature && r.Value is not null).ToList();
    var named = temps.FirstOrDefault(r =>
        Contains(r.SensorName, "System") || Contains(r.SensorName, "Motherboard") ||
        Contains(r.SensorName, "Mainboard") || Contains(r.SensorName, "Board"));
    return (named ?? temps.FirstOrDefault())?.Value;
  }

  // CMOS coin-cell rail: named VBAT / CMOS / (3V)BAT depending on the SuperIO table.
  private static float? Cmos(IReadOnlyList<SensorReading> board) =>
      board.FirstOrDefault(r => r.SensorType == SensorType.Voltage && r.Value is not null &&
          (Contains(r.SensorName, "VBAT") || Contains(r.SensorName, "CMOS") ||
           Contains(r.SensorName, "Battery")))?.Value;

  // Chassis/system fan, excluding the CPU header (that lives on the CPU tile). Fastest spinning wins.
  private static float? ChassisFan(IReadOnlyList<SensorReading> board) {
    var fans = board.Where(r => r.SensorType == SensorType.Fan && r.Value is > 0 &&
        !Contains(r.SensorName, "CPU")).ToList();
    var named = fans.Where(r => Contains(r.SensorName, "Chassis") || Contains(r.SensorName, "System"))
        .Select(r => r.Value).DefaultIfEmpty(null).Max();
    return named ?? fans.Select(r => r.Value).DefaultIfEmpty(null).Max();
  }

  /// <summary>The nominal rail voltage a sensor name denotes, or null when it names no fixed-voltage
  /// rail we can grade. Covers the three main ATX rails and their standby/auxiliary siblings —
  /// +3.3V/+5V/+12V, the −12V rail, standby rails (3VSB/5VSB), and the SuperIO analog 3.3V supply
  /// (AVCC/3VCC) — all of which sit at a known voltage. Variable rails whose "nominal" depends on
  /// the platform (VCore, DRAM/VDIMM, VTT, VCCSA…) return null: there is no universal target to
  /// judge them against, so callers leave them ungraded rather than flag a false fault.</summary>
  public static float? RailNominal(string? name) {
    if (name is null) return null;
    // −12V first: its "12" would otherwise match the +12V query (the leading '-' reads as a rail
    // boundary), grading a healthy −12V rail against +12 and always flagging it critical.
    if (RailMatches(name, "-12")) return -12f;
    // Standby / analog 3.3V supplies that RailMatches' "+3.3V"-style rule doesn't catch by shape.
    if (Contains(name, "3VSB") || Contains(name, "AVCC") || Contains(name, "3VCC")) return 3.3f;
    if (Contains(name, "5VSB")) return 5f;
    if (RailMatches(name, "3.3")) return 3.3f;
    if (RailMatches(name, "5")) return 5f;
    if (RailMatches(name, "12")) return 12f;
    return null;
  }

  /// <summary>Whether a sensor name denotes the CMOS coin-cell rail (VBAT / CMOS / battery).</summary>
  public static bool IsCmosRail(string? name) =>
      Contains(name, "VBAT") || Contains(name, "CMOS") || Contains(name, "Battery");

  // A voltage rail named for its nominal value (e.g. "+12V", "+3.3V", "+5V"), with the
  // sensor's running min/max carried alongside the current value.
  private static RailReading Rail(IReadOnlyList<SensorReading> board, string nominal) {
    var reading = board.FirstOrDefault(r => r.SensorType == SensorType.Voltage && r.Value is not null &&
        RailMatches(r.SensorName, nominal));
    return reading is null ? RailReading.None : new RailReading(reading.Value, reading.Min, reading.Max);
  }

  // Match "+12V"/"12V" without also matching "3.3V" for the "3" query or "5V" inside "3.5V".
  private static bool RailMatches(string? name, string nominal) {
    if (name is null) return false;
    int idx = name.IndexOf(nominal, StringComparison.OrdinalIgnoreCase);
    while (idx >= 0) {
      char before = idx > 0 ? name[idx - 1] : '\0';
      int after = idx + nominal.Length;
      char afterCh = after < name.Length ? name[after] : '\0';
      bool boundaryBefore = !char.IsDigit(before) && before != '.';
      bool endsRail = afterCh is 'V' or 'v';
      if (boundaryBefore && endsRail) return true;
      idx = name.IndexOf(nominal, idx + 1, StringComparison.OrdinalIgnoreCase);
    }
    return false;
  }

  private static bool Contains(string? name, string term) =>
      name is not null && name.Contains(term, StringComparison.OrdinalIgnoreCase);
}
