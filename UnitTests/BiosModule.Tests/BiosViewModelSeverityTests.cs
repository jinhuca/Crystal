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
  public void Board_health_is_the_worst_severity_across_the_graded_rows() {
    var vm = CreateVm(out var model);

    // Everything healthy except +12V at -13% (critical) → whole-board rollup is Critical.
    model.ReadingsSubject.OnNext([
        Board("+3.3V", SensorType.Voltage, 3.31f),
        Board("+5V", SensorType.Voltage, 5.4f),   // +8% → warning
        Board("+12V", SensorType.Voltage, 10.4f), // -13% → critical
    ]);

    Assert.Equal(ReadingSeverity.Critical, vm.BoardHealth);
  }

  [Fact]
  public void Board_health_reports_warning_when_nothing_is_critical() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("VBAT", SensorType.Voltage, 2.6f),   // weak cell → warning
        Board("+5V", SensorType.Voltage, 5.01f),
        Board("+12V", SensorType.Voltage, 12.02f),
    ]);

    Assert.Equal(ReadingSeverity.Warning, vm.BoardHealth);
  }

  [Fact]
  public void Board_health_is_normal_when_every_row_is_in_spec() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("+3.3V", SensorType.Voltage, 3.31f),
        Board("+5V", SensorType.Voltage, 5.01f),
        Board("+12V", SensorType.Voltage, 12.02f),
    ]);

    Assert.Equal(ReadingSeverity.Normal, vm.BoardHealth);
  }

  [Fact]
  public void Board_health_detail_names_offending_rows_worst_first() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("VBAT", SensorType.Voltage, 2.6f),  // CMOS warning
        Board("+5V", SensorType.Voltage, 5.4f),   // +8% → warning
        Board("+12V", SensorType.Voltage, 10.4f), // -13% → critical
    ]);

    Assert.Equal("+12V critical · +5V warning · VBAT warning", vm.BoardHealthDetail);
  }

  [Fact]
  public void Board_health_detail_is_empty_when_everything_is_in_spec() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("+3.3V", SensorType.Voltage, 3.31f),
        Board("+5V", SensorType.Voltage, 5.01f),
        Board("+12V", SensorType.Voltage, 12.02f),
    ]);

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

  private static SensorReading BoardFan(string name, float? value, float? max) =>
      new("SuperIO", HardwareType.SuperIO, name, SensorType.Fan, value, null, max, "RPM");

  [Fact]
  public void Board_rows_grade_only_recognized_rails() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("+12V", SensorType.Voltage, 10.4f),      // -13% → critical rail
        Board("+5V", SensorType.Voltage, 5.02f),       // in spec
        Board("VBAT", SensorType.Voltage, 2.6f),       // weak CMOS cell → warning
        Board("VCore", SensorType.Voltage, 0.3f),      // unknown rail → not graded
        Board("System", SensorType.Temperature, 40f),  // cool board temp → normal
    ]);

    ReadingSeverity Row(string name) =>
        vm.BoardSensors.Single(r => r.Name == name).Severity;

    Assert.Equal(ReadingSeverity.Critical, Row("+12V"));
    Assert.Equal(ReadingSeverity.Normal, Row("+5V"));
    Assert.Equal(ReadingSeverity.Warning, Row("VBAT"));
    Assert.Equal(ReadingSeverity.Normal, Row("VCore"));
    Assert.Equal(ReadingSeverity.Normal, Row("System"));
  }

  [Theory]
  [InlineData(40f, ReadingSeverity.Normal)]    // idle board temp
  [InlineData(59f, ReadingSeverity.Normal)]    // just under the warm mark
  [InlineData(60f, ReadingSeverity.Warning)]   // warm
  [InlineData(69f, ReadingSeverity.Warning)]
  [InlineData(70f, ReadingSeverity.Critical)]  // hot
  [InlineData(85f, ReadingSeverity.Critical)]
  public void Board_temperature_rows_grade_on_their_own_thresholds(float celsius, ReadingSeverity expected) {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([Board("VRM", SensorType.Temperature, celsius)]);

    Assert.Equal(expected, vm.BoardSensors.Single(r => r.Name == "VRM").Severity);
  }

  [Theory]
  [InlineData("3VSB", 2.9f, ReadingSeverity.Critical)]   // 3.3V standby, -12%
  [InlineData("AVCC", 3.1f, ReadingSeverity.Warning)]    // 3.3V analog supply, -6% → warning
  [InlineData("5VSB", 5.5f, ReadingSeverity.Warning)]    // 5V standby, +10% boundary → warning
  [InlineData("-12V", -10.4f, ReadingSeverity.Critical)] // negative rail, -13% on magnitude
  [InlineData("-12V", -12.1f, ReadingSeverity.Normal)]   // negative rail in spec
  public void Broadened_rails_are_graded_against_their_nominal(string name, float value, ReadingSeverity expected) {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([Board(name, SensorType.Voltage, value)]);

    Assert.Equal(expected, vm.BoardSensors.Single(r => r.Name == name).Severity);
  }

  [Fact]
  public void Stalled_fan_is_flagged_only_when_the_board_is_warm() {
    var vm = CreateVm(out var model);

    // Board hot enough that a stopped fan is a real risk.
    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 65f, CmosVoltage: 3.0f, ChassisFanRpm: 0f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(12.02f)));
    model.ReadingsSubject.OnNext([
        BoardFan("Chassis Fan", 0f, 1400f),  // spun before, now stopped, board hot → critical
    ]);

    Assert.Equal(ReadingSeverity.Critical, vm.BoardSensors.Single(r => r.Name == "Chassis Fan").Severity);
    Assert.Equal(ReadingSeverity.Critical, vm.BoardHealth);
  }

  [Fact]
  public void Idle_fan_is_not_flagged_while_the_board_is_cool() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 30f, CmosVoltage: 3.0f, ChassisFanRpm: 0f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(12.02f)));
    model.ReadingsSubject.OnNext([
        BoardFan("Chassis Fan", 0f, 1400f),  // semi-passive: intentionally stopped while cool
    ]);

    Assert.Equal(ReadingSeverity.Normal, vm.BoardSensors.Single(r => r.Name == "Chassis Fan").Severity);
  }

  [Fact]
  public void Chassis_fan_presence_and_stall_severity_are_exposed_for_the_trend() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 65f, CmosVoltage: 3.0f, ChassisFanRpm: 0f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(12.02f)));
    model.ReadingsSubject.OnNext([
        BoardFan("Chassis Fan", 0f, 1400f),  // stalled while board hot → critical
    ]);

    Assert.True(vm.HasChassisFan);
    Assert.Equal(ReadingSeverity.Critical, vm.ChassisFanSeverity);
  }

  [Fact]
  public void No_chassis_fan_is_reported_when_only_the_cpu_fan_is_present() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("CPU Fan", SensorType.Fan, 1600f),
    ]);

    Assert.False(vm.HasChassisFan);
    Assert.Equal(ReadingSeverity.Normal, vm.ChassisFanSeverity);
  }

  [Fact]
  public void Unpopulated_fan_header_reading_zero_is_never_flagged() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 70f, CmosVoltage: 3.0f, ChassisFanRpm: 0f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(12.02f)));
    model.ReadingsSubject.OnNext([
        BoardFan("Fan #3", 0f, 0f),  // never spun this session → empty connector, not a stall
    ]);

    Assert.Equal(ReadingSeverity.Normal, vm.BoardSensors.Single(r => r.Name == "Fan #3").Severity);
  }
}
