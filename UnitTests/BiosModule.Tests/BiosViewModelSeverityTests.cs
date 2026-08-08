using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using BiosModule.Models;
using BiosModule.ViewModels;
using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.Telemetry.Hardware;
using Crystal.Service.Bios;
using Crystal.Service.Sensors;
using Prism.Events;
using Xunit;

namespace BiosModule.Tests;

public class BiosViewModelSeverityTests {
  private sealed class FakeBiosModel : IBiosModel {
    public Subject<FirmwareSnapshot> FirmwareSubject { get; } = new();
    public Subject<BoardTelemetry> TelemetrySubject { get; } = new();
    public Subject<IReadOnlyList<SensorReading>> ReadingsSubject { get; } = new();
    public IObservable<FirmwareSnapshot> Firmware => FirmwareSubject;
    public IObservable<BoardTelemetry> BoardTelemetry => TelemetrySubject;
    public IObservable<IReadOnlyList<SensorReading>> BoardReadings => ReadingsSubject;
    public bool BoardSensorDriverInstalled => true;
    public bool BoardSensorDriverAccessible => true;
  }

  private static BiosViewModel CreateVm(out FakeBiosModel model) {
    model = new FakeBiosModel();
    return new BiosViewModel(model, new EventAggregator());
  }

  private static RailReading Rail(float value) => new(value, null, null);

  [Fact]
  public void Healthy_telemetry_leaves_every_reading_normal() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 3.0f, ChassisFanRpm: 800f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(12.02f)));

    Assert.Equal(ReadingSeverity.Normal, vm.CmosSeverity);
    Assert.Equal(ReadingSeverity.Normal, vm.Rail3V3Severity);
    Assert.Equal(ReadingSeverity.Normal, vm.Rail5VSeverity);
    Assert.Equal(ReadingSeverity.Normal, vm.Rail12VSeverity);
  }

  [Fact]
  public void Out_of_spec_readings_flag_the_matching_severity() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 2.6f, ChassisFanRpm: 800f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.4f), Rail12V: Rail(10.4f)));

    Assert.Equal(ReadingSeverity.Warning, vm.CmosSeverity);   // 2.6 V weak cell
    Assert.Equal(ReadingSeverity.Normal, vm.Rail3V3Severity); // in spec
    Assert.Equal(ReadingSeverity.Warning, vm.Rail5VSeverity); // +8%
    Assert.Equal(ReadingSeverity.Critical, vm.Rail12VSeverity); // -13%
  }

  [Fact]
  public void Rail_range_formats_min_max_when_both_known_and_is_empty_otherwise() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 3.0f, ChassisFanRpm: 800f,
        Rail3V3: new RailReading(3.31f, 3.28f, 3.34f),  // both bounds → formatted
        Rail5V: new RailReading(5.01f, null, 5.05f),    // partial → empty
        Rail12V: RailReading.None));                    // absent → empty

    Assert.Equal("3.28–3.34", vm.Rail3V3Range);
    Assert.Equal("", vm.Rail5VRange);
    Assert.Equal("", vm.Rail12VRange);
  }

  [Fact]
  public void Board_health_is_the_worst_severity_across_the_graded_rails() {
    var vm = CreateVm(out var model);

    // Everything healthy except +12V at -13% (critical) → whole-board rollup is Critical.
    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 3.0f, ChassisFanRpm: 800f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.4f), Rail12V: Rail(10.4f)));

    Assert.Equal(ReadingSeverity.Critical, vm.BoardHealth);
  }

  [Fact]
  public void Board_health_reports_warning_when_nothing_is_critical() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 2.6f, ChassisFanRpm: 800f,  // weak cell → warning
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(12.02f)));

    Assert.Equal(ReadingSeverity.Warning, vm.BoardHealth);
  }

  [Fact]
  public void Board_health_is_normal_when_every_rail_is_in_spec() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 3.0f, ChassisFanRpm: 800f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(12.02f)));

    Assert.Equal(ReadingSeverity.Normal, vm.BoardHealth);
  }

  [Fact]
  public void Board_health_detail_names_offending_rails_worst_first() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 2.6f, ChassisFanRpm: 800f,  // CMOS warning
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.4f), Rail12V: Rail(10.4f))); // +5V warning, +12V critical

    Assert.Equal("+12V critical · +5V warning · CMOS warning", vm.BoardHealthDetail);
  }

  [Fact]
  public void Board_health_detail_is_empty_when_everything_is_in_spec() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 3.0f, ChassisFanRpm: 800f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(12.02f)));

    Assert.Equal("", vm.BoardHealthDetail);
  }

  [Fact]
  public void Missing_readings_stay_normal() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(BoardTelemetry.Empty);

    Assert.Equal(ReadingSeverity.Normal, vm.CmosSeverity);
    Assert.Equal(ReadingSeverity.Normal, vm.Rail12VSeverity);
  }

  private static SensorReading Board(string name, SensorType type, float? value) =>
      new("SuperIO", HardwareType.SuperIO, name, type, value, null, null, null);

  [Fact]
  public void Board_rows_grade_only_recognized_rails() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("+12V", SensorType.Voltage, 10.4f),      // -13% → critical rail
        Board("+5V", SensorType.Voltage, 5.02f),       // in spec
        Board("VBAT", SensorType.Voltage, 2.6f),       // weak CMOS cell → warning
        Board("VCore", SensorType.Voltage, 0.3f),      // unknown rail → not graded
        Board("System", SensorType.Temperature, 90f),  // temps are never graded
    ]);

    ReadingSeverity Row(string name) =>
        vm.BoardSensors.Single(r => r.Name == name).Severity;

    Assert.Equal(ReadingSeverity.Critical, Row("+12V"));
    Assert.Equal(ReadingSeverity.Normal, Row("+5V"));
    Assert.Equal(ReadingSeverity.Warning, Row("VBAT"));
    Assert.Equal(ReadingSeverity.Normal, Row("VCore"));
    Assert.Equal(ReadingSeverity.Normal, Row("System"));
  }
}
