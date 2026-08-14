using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Service.Bios;
using Crystal.Service.Sensors;
using System.Collections.Generic;

namespace Crystal.BiosModule.Models;

/// <summary>Firmware identity as a replayed static stream, plus live motherboard telemetry
/// (the BIOS tile doubles as the board's home).</summary>
public interface IBiosModel {
  /// <summary>Static firmware identity, built once and replayed.</summary>
  IObservable<FirmwareSnapshot> Firmware { get; }

  /// <summary>Compact live board readings for the summary tile (1s cadence).</summary>
  IObservable<BoardTelemetry> BoardTelemetry { get; }

  /// <summary>Every live motherboard-category reading, for the detail sensor table.</summary>
  IObservable<IReadOnlyList<SensorReading>> BoardReadings { get; }

  /// <summary>Whether the PawnIO driver serving board sensors is installed (registry check).</summary>
  bool BoardSensorDriverInstalled { get; }

  /// <summary>Whether the PawnIO driver device can actually be opened now (installed + running +
  /// elevated) — the authoritative "board sensors are readable" signal.</summary>
  bool BoardSensorDriverAccessible { get; }
}
