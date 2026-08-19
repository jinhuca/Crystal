using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Service.Sensors;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Crystal.BiosModule.Models;

/// <summary>
/// Live motherboard telemetry for the BIOS tile, projected from the shell's shared
/// <see cref="SensorMonitor"/>. There is no separate Motherboard module, so the BIOS tile is the
/// board's home: this taps the general sensor snapshot's <see cref="SensorCategory.Motherboard"/>
/// bucket (SuperIO/EC voltages, temperatures, fans) and exposes both the compact
/// <see cref="BoardTelemetry"/> headline and the full reading list for the detail table.
/// <para>
/// A thin projection: it adds no timer of its own and inherits the monitor's cold, ref-counted
/// 1-second cadence.
/// </para>
/// </summary>
public sealed class BoardSensorMonitor {
  /// <summary>
  /// Compact headline readings the summary tile shows, on each poll.
  /// </summary>
  private readonly IObservable<BoardTelemetry> _telemetry;

  /// <summary>
  /// Every motherboard-category reading, on each poll, for the detail sensor table.
  /// </summary>
  private readonly IObservable<IReadOnlyList<SensorReading>> _readings;

  /// <summary>
  /// Initializes a new instance of the <see cref="BoardSensorMonitor"/> class, projecting the
  /// </summary>
  /// <param name="monitor">The sensor monitor.</param>
  public BoardSensorMonitor(SensorMonitor monitor) {
    ArgumentNullException.ThrowIfNull(monitor);
    _telemetry = monitor.Snapshots.Select(BoardTelemetrySelector.Select);
    _readings = monitor.Snapshots.Select(s => s[SensorCategory.Motherboard]);
  }

  /// <summary>
  /// Compact headline readings the summary tile shows, on each poll.
  /// </summary>
  public IObservable<BoardTelemetry> Telemetry => _telemetry;

  /// <summary>
  /// Every motherboard-category reading, on each poll, for the detail sensor table.
  /// </summary>
  public IObservable<IReadOnlyList<SensorReading>> Readings => _readings;

  /// <summary>
  /// Whether the PawnIO kernel driver is installed (registry check).
  /// </summary>
  public bool DriverInstalled => SensorDriver.IsInstalled;

  /// <summary>
  /// Whether the PawnIO driver device can actually be opened now (installed + running +
  /// elevated). SuperIO/EC board sensors are read only through it, so when this is false the
  /// telemetry streams stay empty and the UI should explain why rather than show blanks.
  /// </summary>
  public bool DriverAccessible => SensorDriver.IsAccessible;
}
