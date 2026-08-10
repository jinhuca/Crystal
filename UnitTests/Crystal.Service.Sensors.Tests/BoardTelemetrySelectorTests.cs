using Crystal.Infrastructure.DataStructures.Sensors;
using Xunit;

namespace Crystal.Service.Sensors.Tests;

public class BoardTelemetrySelectorTests {
  private static SensorReading Board(string name, SensorType type, float? value) =>
      FakeSensorTelemetrySource.Reading(HardwareType.SuperIO, name, type, value);

  [Fact]
  public void Picks_cmos_voltage_by_name() {
    var snapshot = new SensorSnapshot([
        Board("VBAT", SensorType.Voltage, 3.12f),
        Board("+5V", SensorType.Voltage, 5.02f),
    ]);

    Assert.Equal(3.12f, BoardTelemetrySelector.Select(snapshot).CmosVoltage);
  }

  [Fact]
  public void Prefers_system_named_board_temperature() {
    var snapshot = new SensorSnapshot([
        Board("Temperature #2", SensorType.Temperature, 40f),
        Board("System", SensorType.Temperature, 34f),
    ]);

    Assert.Equal(34f, BoardTelemetrySelector.Select(snapshot).BoardTemperature);
  }

  [Fact]
  public void Falls_back_to_first_board_temperature_when_none_named() {
    var snapshot = new SensorSnapshot([
        Board("Temperature #1", SensorType.Temperature, 38f),
        Board("Temperature #2", SensorType.Temperature, 42f),
    ]);

    Assert.Equal(38f, BoardTelemetrySelector.Select(snapshot).BoardTemperature);
  }

  [Fact]
  public void Chassis_fan_excludes_the_cpu_fan() {
    var snapshot = new SensorSnapshot([
        Board("CPU Fan", SensorType.Fan, 1500f),
        Board("Chassis Fan #1", SensorType.Fan, 700f),
    ]);

    Assert.Equal(700f, BoardTelemetrySelector.Select(snapshot).ChassisFanRpm);
  }

  [Fact]
  public void Chassis_fan_takes_fastest_when_unnamed() {
    var snapshot = new SensorSnapshot([
        Board("Fan #2", SensorType.Fan, 500f),
        Board("Fan #3", SensorType.Fan, 900f),
    ]);

    Assert.Equal(900f, BoardTelemetrySelector.Select(snapshot).ChassisFanRpm);
  }

  private static SensorReading Fan(string name, float? value, float? max) =>
      new("SuperIO", HardwareType.SuperIO, name, SensorType.Fan, value, null, max, "RPM");

  [Fact]
  public void ChassisFanRow_prefers_a_chassis_named_fan_over_the_cpu_fan() {
    var rows = new SensorSnapshot([
        Board("CPU Fan", SensorType.Fan, 1600f),
        Fan("Chassis Fan #1", 700f, 1400f),
    ])[SensorCategory.Motherboard];

    Assert.Equal("Chassis Fan #1", BoardTelemetrySelector.ChassisFanRow(rows)!.SensorName);
  }

  [Fact]
  public void ChassisFanRow_keeps_the_same_fan_across_a_stall() {
    // The chassis fan (highest observed Max) has stalled to 0 while a smaller fan still spins.
    // Selection is by capacity, so the stalled fan stays chosen rather than the pick jumping.
    var rows = new SensorSnapshot([
        Fan("Fan #2", 600f, 800f),
        Fan("Fan #1", 0f, 1500f),
    ])[SensorCategory.Motherboard];

    var pick = BoardTelemetrySelector.ChassisFanRow(rows)!;
    Assert.Equal("Fan #1", pick.SensorName);
    Assert.Equal(0f, pick.Value);
  }

  [Fact]
  public void ChassisFanRow_is_null_when_only_the_cpu_fan_is_present() {
    var rows = new SensorSnapshot([
        Board("CPU Fan", SensorType.Fan, 1600f),
    ])[SensorCategory.Motherboard];

    Assert.Null(BoardTelemetrySelector.ChassisFanRow(rows));
  }

  [Fact]
  public void Rails_match_their_nominal_voltage_without_crosstalk() {
    var snapshot = new SensorSnapshot([
        Board("+3.3V", SensorType.Voltage, 3.31f),
        Board("+5V", SensorType.Voltage, 5.01f),
        Board("+12V", SensorType.Voltage, 12.05f),
    ]);

    var t = BoardTelemetrySelector.Select(snapshot);
    Assert.Equal(3.31f, t.Rail3V3.Value);
    Assert.Equal(5.01f, t.Rail5V.Value);
    Assert.Equal(12.05f, t.Rail12V.Value);
  }

  [Fact]
  public void The_5V_query_does_not_match_the_3_3V_rail() {
    // "+3.3V" contains no standalone "5"; and the "3" query must not pick up "+3.3V" as 12V etc.
    var snapshot = new SensorSnapshot([
        Board("+3.3V", SensorType.Voltage, 3.31f),
    ]);

    var t = BoardTelemetrySelector.Select(snapshot);
    Assert.Equal(3.31f, t.Rail3V3.Value);
    Assert.Null(t.Rail5V.Value);
    Assert.Null(t.Rail12V.Value);
  }

  [Fact]
  public void The_12V_query_does_not_pick_up_the_negative_12V_rail() {
    // A board exposing both −12V and +12V must headline the positive rail on the tile, not the
    // negative one — the leading minus is a sign, not a rail boundary.
    var snapshot = new SensorSnapshot([
        Board("-12V", SensorType.Voltage, -12.1f),
        Board("+12V", SensorType.Voltage, 12.05f),
    ]);

    Assert.Equal(12.05f, BoardTelemetrySelector.Select(snapshot).Rail12V.Value);
  }

  [Fact]
  public void The_negative_12V_rail_alone_is_not_taken_as_the_positive_rail() {
    var snapshot = new SensorSnapshot([
        Board("-12V", SensorType.Voltage, -12.1f),
    ]);

    Assert.Null(BoardTelemetrySelector.Select(snapshot).Rail12V.Value);
  }

  [Fact]
  public void Rail_carries_the_sensor_running_min_and_max() {
    var snapshot = new SensorSnapshot([
        new SensorReading("SuperIO", HardwareType.SuperIO, "+12V", SensorType.Voltage,
            Value: 12.05f, Min: 11.90f, Max: 12.13f, Unit: "V"),
    ]);

    var rail = BoardTelemetrySelector.Select(snapshot).Rail12V;
    Assert.Equal(12.05f, rail.Value);
    Assert.Equal(11.90f, rail.Min);
    Assert.Equal(12.13f, rail.Max);
  }

  [Fact]
  public void An_absent_rail_is_the_none_reading() {
    var rail = BoardTelemetrySelector.Select(new SensorSnapshot([])).Rail5V;
    Assert.Same(RailReading.None, rail);
  }

  [Fact]
  public void Ignores_readings_from_other_categories() {
    var snapshot = new SensorSnapshot([
        FakeSensorTelemetrySource.Reading(HardwareType.Cpu, "VBAT", SensorType.Voltage, 3.0f),
        FakeSensorTelemetrySource.Reading(HardwareType.Cpu, "CPU Package", SensorType.Temperature, 65f),
    ]);

    var t = BoardTelemetrySelector.Select(snapshot);
    Assert.Null(t.CmosVoltage);
    Assert.Null(t.BoardTemperature);
  }

  [Fact]
  public void Returns_empty_for_a_snapshot_with_no_board_sensors() {
    var t = BoardTelemetrySelector.Select(new SensorSnapshot([]));

    Assert.Equal(BoardTelemetry.Empty, t);
  }

  [Theory]
  [InlineData("+3.3V", 3.3f)]
  [InlineData("3.3V", 3.3f)]
  [InlineData("+5V", 5f)]
  [InlineData("+12V", 12f)]
  [InlineData("-12V", -12f)]    // negative rail keeps its sign
  [InlineData("3VSB", 3.3f)]    // 3.3V standby
  [InlineData("AVCC", 3.3f)]    // SuperIO analog 3.3V supply
  [InlineData("3VCC", 3.3f)]
  [InlineData("5VSB", 5f)]      // 5V standby
  public void RailNominal_maps_a_rail_name_to_its_nominal_voltage(string name, float expected) =>
      Assert.Equal(expected, BoardTelemetrySelector.RailNominal(name));

  [Theory]
  [InlineData("VCore")]         // variable rail — no universal nominal
  [InlineData("VDIMM")]
  [InlineData("Temperature #1")]
  [InlineData(null)]
  public void RailNominal_is_null_for_names_that_are_not_fixed_rails(string? name) =>
      Assert.Null(BoardTelemetrySelector.RailNominal(name));

  [Theory]
  [InlineData("VBAT")]
  [InlineData("CMOS Battery")]
  [InlineData("Battery")]
  public void IsCmosRail_recognizes_the_coin_cell_names(string name) =>
      Assert.True(BoardTelemetrySelector.IsCmosRail(name));

  [Theory]
  [InlineData("+12V")]
  [InlineData("VCore")]
  [InlineData(null)]
  public void IsCmosRail_rejects_other_names(string? name) =>
      Assert.False(BoardTelemetrySelector.IsCmosRail(name));
}
