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

  [Fact]
  public void Healthy_telemetry_leaves_every_reading_normal() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 3.0f, ChassisFanRpm: 800f,
        Rail3V3: 3.31f, Rail5V: 5.01f, Rail12V: 12.02f));

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
        Rail3V3: 3.31f, Rail5V: 5.4f, Rail12V: 10.4f));

    Assert.Equal(ReadingSeverity.Warning, vm.CmosSeverity);   // 2.6 V weak cell
    Assert.Equal(ReadingSeverity.Normal, vm.Rail3V3Severity); // in spec
    Assert.Equal(ReadingSeverity.Warning, vm.Rail5VSeverity); // +8%
    Assert.Equal(ReadingSeverity.Critical, vm.Rail12VSeverity); // -13%
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
