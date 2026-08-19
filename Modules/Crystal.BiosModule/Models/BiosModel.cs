using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Service.Bios;
using Crystal.Service.Sensors;
using System.Collections.Generic;

namespace Crystal.BiosModule.Models;

/// <summary>
/// Adapts the static <see cref="BiosMonitor"/> and the live <see cref="BoardSensorMonitor"/>
/// into the module's <see cref="IBiosModel"/>: forwards the replayed firmware stream alongside the
/// board telemetry streams, and owns the firmware monitor's lifetime.
/// </summary>
public sealed class BiosModel : IBiosModel, IDisposable {
  /// <summary>
  /// The service that owns the polling lifetime and the firmware replay cache.
  /// </summary>
  private readonly BiosMonitor _monitor;

  /// <summary>
  /// The service that owns the board telemetry streams and the board sensor driver lifetime.
  /// </summary>
  private readonly BoardSensorMonitor _board;

  /// <summary>
  /// Initializes a new instance of the <see cref="BiosModel"/> class, forwarding the
  /// </summary>
  /// <param name="monitor">The BIOS monitor.</param>
  /// <param name="board">The board sensor monitor.</param>
  public BiosModel(BiosMonitor monitor, BoardSensorMonitor board) {
    ArgumentNullException.ThrowIfNull(monitor);
    ArgumentNullException.ThrowIfNull(board);
    _monitor = monitor;
    _board = board;
  }

  /// <summary>
  /// Static firmware snapshot; emits once and replays to new subscribers.
  /// </summary>
  public IObservable<FirmwareSnapshot> Firmware => _monitor.Firmware;

  /// <summary>
  /// Live board telemetry; emits a fresh snapshot on each poll.
  /// </summary>
  public IObservable<BoardTelemetry> BoardTelemetry => _board.Telemetry;

  /// <summary>
  /// Live board sensor readings; emits a fresh snapshot on each poll.
  /// </summary>
  public IObservable<IReadOnlyList<SensorReading>> BoardReadings => _board.Readings;

  /// <summary>
  /// Indicates whether the board sensor driver is installed on the system.
  /// </summary>
  public bool BoardSensorDriverInstalled => _board.DriverInstalled;

  /// <summary>
  /// Indicates whether the board sensor driver is accessible and can be used to read telemetry.
  /// </summary>
  public bool BoardSensorDriverAccessible => _board.DriverAccessible;

  /// <summary>
  /// Disposes the underlying <see cref="BiosMonitor"/> and <see cref="BoardSensorMonitor"/> to stop polling and release resources.
  /// </summary>
  public void Dispose() => _monitor.Dispose();
}
