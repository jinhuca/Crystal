using System.Collections.Generic;
using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Service.Bios;
using Crystal.Service.Sensors;

namespace Crystal.BiosModule.Models;

/// <summary>Adapts the static <see cref="BiosMonitor"/> and the live <see cref="BoardSensorMonitor"/>
/// into the module's <see cref="IBiosModel"/>: forwards the replayed firmware stream alongside the
/// board telemetry streams, and owns the firmware monitor's lifetime.</summary>
public sealed class BiosModel : IBiosModel, IDisposable {
  private readonly BiosMonitor _monitor;
  private readonly BoardSensorMonitor _board;

  public BiosModel(BiosMonitor monitor, BoardSensorMonitor board) {
    ArgumentNullException.ThrowIfNull(monitor);
    ArgumentNullException.ThrowIfNull(board);
    _monitor = monitor;
    _board = board;
  }

  public IObservable<FirmwareSnapshot> Firmware => _monitor.Firmware;
  public IObservable<BoardTelemetry> BoardTelemetry => _board.Telemetry;
  public IObservable<IReadOnlyList<SensorReading>> BoardReadings => _board.Readings;
  public bool BoardSensorDriverInstalled => _board.DriverInstalled;
  public bool BoardSensorDriverAccessible => _board.DriverAccessible;

  public void Dispose() => _monitor.Dispose();
}
