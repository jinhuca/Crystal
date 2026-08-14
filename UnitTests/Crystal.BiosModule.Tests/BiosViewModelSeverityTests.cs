using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Crystal.BiosModule.Models;
using Crystal.BiosModule.ViewModels;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Service.Bios;
using Crystal.Service.Sensors;
using Prism.Events;
using Xunit;

namespace Crystal.BiosModule.Tests;

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

  private static BiosViewModel CreateVm(out FakeBiosModel model, Func<DateTimeOffset> clock) {
    model = new FakeBiosModel();
    return new BiosViewModel(model, new EventAggregator(), clock);
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
  public void Board_health_summary_counts_offenders_by_severity_criticals_first() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("VBAT", SensorType.Voltage, 2.6f),   // CMOS warning
        Board("+5V", SensorType.Voltage, 5.4f),    // +8% → warning
        Board("+12V", SensorType.Voltage, 10.4f),  // -13% → critical
    ]);

    Assert.Equal("1 critical · 2 warnings", vm.BoardHealthSummary);
  }

  [Fact]
  public void Board_health_summary_singularizes_a_lone_warning_and_is_empty_when_healthy() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([Board("+5V", SensorType.Voltage, 5.4f)]);  // one warning
    Assert.Equal("1 warning", vm.BoardHealthSummary);

    model.ReadingsSubject.OnNext([Board("+5V", SensorType.Voltage, 5.01f)]); // recovers
    Assert.Equal("", vm.BoardHealthSummary);
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

  private static SensorReading Board(string name, SensorType type, float? value, float? min, float? max) =>
      new("SuperIO", HardwareType.SuperIO, name, type, value, min, max, null);

  [Fact]
  public void Row_min_max_are_graded_so_a_recovered_dip_still_shows_in_the_column() {
    var vm = CreateVm(out var model);

    // +12V is in spec now, but dipped to 10.4 (critical) and peaked at 12.1 (in spec) earlier.
    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 12.02f, 10.4f, 12.1f)]);

    var row = vm.BoardSensors.Single(r => r.Name == "+12V");
    Assert.Equal(ReadingSeverity.Normal, row.Severity);      // live value is fine
    Assert.Equal(ReadingSeverity.Critical, row.MinSeverity); // the recorded dip is not
    Assert.Equal(ReadingSeverity.Normal, row.MaxSeverity);
  }

  [Fact]
  public void Row_max_is_graded_for_temperatures() {
    var vm = CreateVm(out var model);

    // Board temp idles fine now but peaked at 72 (critical).
    model.ReadingsSubject.OnNext([Board("System", SensorType.Temperature, 40f, 35f, 72f)]);

    var row = vm.BoardSensors.Single(r => r.Name == "System");
    Assert.Equal(ReadingSeverity.Normal, row.Severity);
    Assert.Equal(ReadingSeverity.Normal, row.MinSeverity);
    Assert.Equal(ReadingSeverity.Critical, row.MaxSeverity);
  }

  [Fact]
  public void Fan_min_max_are_never_graded() {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 65f, CmosVoltage: 3.0f, ChassisFanRpm: 0f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(12.02f)));
    // A fan whose recorded Min is 0 RPM: that's an idle extreme, not a judgeable stall.
    model.ReadingsSubject.OnNext([Board("Chassis Fan", SensorType.Fan, 0f, 0f, 1400f)]);

    var row = vm.BoardSensors.Single(r => r.Name == "Chassis Fan");
    Assert.Equal(ReadingSeverity.Critical, row.Severity);    // live stall while hot → flagged
    Assert.Equal(ReadingSeverity.Normal, row.MinSeverity);   // but the 0-RPM extreme is not
    Assert.Equal(ReadingSeverity.Normal, row.MaxSeverity);
  }

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

  [Fact]
  public void Board_rows_default_to_worst_severity_first() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("+3.3V", SensorType.Voltage, 3.31f),  // normal
        Board("VBAT", SensorType.Voltage, 2.6f),    // warning
        Board("+12V", SensorType.Voltage, 10.4f),   // critical
        Board("+5V", SensorType.Voltage, 5.01f),    // normal
    ]);

    var order = vm.BoardSensors.Select(r => r.Name).ToList();
    Assert.Equal("+12V", order[0]);   // critical first
    Assert.Equal("VBAT", order[1]);   // then warning
    // Remaining two are normal, ordered by name.
    Assert.Equal(["+3.3V", "+5V"], order.Skip(2));
  }

  [Fact]
  public void Row_sort_keys_are_numeric_and_missing_readings_are_nan() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 12.0f, 11.9f, null)]);

    var row = vm.BoardSensors.Single(r => r.Name == "+12V");
    Assert.Equal(12.0, row.ValueSort, 3);
    Assert.Equal(11.9, row.MinSort, 3);
    Assert.True(double.IsNaN(row.MaxSort));
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

  [Theory]
  [InlineData(40f, ReadingSeverity.Normal)]
  [InlineData(62f, ReadingSeverity.Warning)]
  [InlineData(72f, ReadingSeverity.Critical)]
  public void Board_temperature_severity_is_exposed_for_the_trend(float celsius, ReadingSeverity expected) {
    var vm = CreateVm(out var model);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: celsius, CmosVoltage: 3.0f, ChassisFanRpm: 800f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(12.02f)));

    Assert.Equal(expected, vm.BoardTemperatureSeverity);
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
  public void Rail_graph_markers_track_the_session_low_and_high() => StaRunner.Run(() => {
    var vm = CreateVm(out var model);
    var g3 = new PerformanceGraph();
    var g5 = new PerformanceGraph();
    var g12 = new PerformanceGraph();
    vm.AttachRailGraphs(g3, g5, g12);

    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 3.0f, ChassisFanRpm: 800f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(12.20f)));
    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 3.0f, ChassisFanRpm: 800f,
        Rail3V3: Rail(3.28f), Rail5V: Rail(5.05f), Rail12V: Rail(11.90f)));

    // +12V saw 12.20 then 11.90 → low marker at the dip, high marker at the peak.
    Assert.Equal(11.90, g12.LowMarker, 3);
    Assert.Equal(12.20, g12.HighMarker, 3);
  });

  [Fact]
  public void Attaching_a_fresh_graph_resets_its_markers() => StaRunner.Run(() => {
    var vm = CreateVm(out var model);
    var g3 = new PerformanceGraph { LowMarker = 1.0, HighMarker = 9.0 };
    var g5 = new PerformanceGraph();
    var g12 = new PerformanceGraph();

    vm.AttachRailGraphs(g3, g5, g12);

    Assert.True(double.IsNaN(g3.LowMarker));
    Assert.True(double.IsNaN(g3.HighMarker));
  });

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
  public void An_out_of_spec_reading_leaves_a_health_event_after_it_recovers() {
    var now = new DateTimeOffset(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);
    var vm = CreateVm(out var model, () => now);

    // +12V goes critical, then recovers on the next tick.
    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 10.4f)]);
    Assert.True(vm.HasHealthEvents);
    Assert.True(vm.HealthEvents.Single().Ongoing);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 12.02f)]);

    // The live table is clean, but the episode persists as recovered history.
    Assert.Equal(ReadingSeverity.Normal, vm.BoardHealth);
    var e = vm.HealthEvents.Single();
    Assert.Equal("+12V", e.SensorName);
    Assert.Equal(ReadingSeverity.Critical, e.Severity);
    Assert.Equal("10.4", e.PeakValue);  // the reading that triggered the peak (this fake row carries no unit)
    Assert.False(e.Ongoing);
  }

  [Fact]
  public void Clear_history_empties_the_event_log_and_resets_graph_markers() => StaRunner.Run(() => {
    var now = new DateTimeOffset(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);
    var vm = CreateVm(out var model, () => now);
    var g3 = new PerformanceGraph();
    var g5 = new PerformanceGraph();
    var g12 = new PerformanceGraph();
    vm.AttachRailGraphs(g3, g5, g12);

    // A dip records an event and moves the +12V graph's session markers off NaN.
    model.TelemetrySubject.OnNext(new BoardTelemetry(
        BoardTemperature: 35f, CmosVoltage: 3.0f, ChassisFanRpm: 800f,
        Rail3V3: Rail(3.31f), Rail5V: Rail(5.01f), Rail12V: Rail(11.90f)));
    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 10.4f)]);
    Assert.True(vm.HasHealthEvents);
    Assert.False(double.IsNaN(g12.LowMarker));

    vm.ClearHistoryCommand.Execute(null);

    Assert.False(vm.HasHealthEvents);
    Assert.Empty(vm.HealthEvents);
    Assert.True(double.IsNaN(g12.LowMarker));
    Assert.True(double.IsNaN(g12.HighMarker));
  });

  [Fact]
  public void A_healthy_board_records_no_health_events() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 12.02f)]);

    Assert.False(vm.HasHealthEvents);
    Assert.Empty(vm.HealthEvents);
  }

  [Fact]
  public void Health_events_export_as_tab_separated_text_with_a_header() {
    var now = new DateTimeOffset(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);
    var vm = CreateVm(out var model, () => now);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 10.4f)]);  // ongoing critical

    var lines = vm.HealthEventsAsText()
        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(l => l.TrimEnd('\r'))
        .ToArray();

    Assert.Equal($"# Exported {now.LocalDateTime:yyyy-MM-dd HH:mm:ss}", lines[0]);
    Assert.Equal("# 1 event(s), 1 ongoing", lines[1]);
    Assert.Equal("Started\tSensor\tPeak\tReading\tPeak at\tDuration", lines[2]);
    var cols = lines[3].Split('\t');
    Assert.Equal("+12V", cols[1]);
    Assert.Equal("Critical", cols[2]);
    Assert.Equal("10.4", cols[3]);
    Assert.Equal(now.LocalDateTime.ToString("HH:mm:ss"), cols[4]);  // peak captured at the first (only) tick
    Assert.StartsWith("ongoing", cols[5]);
  }

  [Fact]
  public void Cap_note_surfaces_once_the_retention_limit_evicts_older_events() {
    var vm = CreateVm(out var model);

    Assert.Equal("", vm.HealthEventsCapNote);           // nothing dropped yet

    // Open then recover 60 distinct rails (a critical dip, then healthy) → 60 closed episodes, of
    // which the log keeps 50 and reports 10 dropped.
    for (int i = 0; i < 60; i++) {
      model.ReadingsSubject.OnNext([Board($"+12V#{i}", SensorType.Voltage, 10.4f)]);  // critical
      model.ReadingsSubject.OnNext([Board($"+12V#{i}", SensorType.Voltage, 12.02f)]); // recovers
    }

    Assert.Equal("+10 older dropped", vm.HealthEventsCapNote);
  }

  [Fact]
  public void Export_states_it_is_truncated_once_the_cap_drops_older_events() {
    var vm = CreateVm(out var model);

    for (int i = 0; i < 60; i++) {
      model.ReadingsSubject.OnNext([Board($"+12V#{i}", SensorType.Voltage, 10.4f)]);  // critical
      model.ReadingsSubject.OnNext([Board($"+12V#{i}", SensorType.Voltage, 12.02f)]); // recovers
    }

    var text = vm.HealthEventsAsText();

    Assert.Contains("# 10 older recovered event(s) dropped by the retention cap", text);
  }

  [Fact]
  public void Export_omits_the_truncation_note_when_nothing_was_dropped() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 10.4f)]);

    Assert.DoesNotContain("dropped by the retention cap", vm.HealthEventsAsText());
  }

  [Fact]
  public void Filtered_export_is_flagged_and_carries_only_the_shown_rows() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("+5V", SensorType.Voltage, 5.4f),    // warning
        Board("+12V", SensorType.Voltage, 10.4f),  // critical
    ]);
    vm.ShowCriticalOnly = true;

    var lines = vm.HealthEventsAsText()
        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(l => l.TrimEnd('\r'))
        .ToArray();

    Assert.StartsWith("# Exported ", lines[0]);                        // provenance stamp first
    Assert.Equal("# 1 event(s), 1 ongoing", lines[1]);                 // then the row count (the critical row is ongoing)
    Assert.Equal("# Filtered view: critical events only", lines[2]);   // then the filter note
    Assert.Equal("Started\tSensor\tPeak\tReading\tPeak at\tDuration", lines[3]);
    var row = Assert.Single(lines.Skip(4));                            // only the critical row exported
    Assert.Contains("+12V", row);
    Assert.DoesNotContain("+5V", string.Join("\n", lines));
  }

  [Fact]
  public void Export_leads_with_the_capture_timestamp_from_the_clock() {
    var now = new DateTimeOffset(2026, 8, 8, 14, 30, 0, TimeSpan.Zero);
    var vm = CreateVm(out var model, () => now);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 10.4f)]);

    var text = vm.HealthEventsAsText();

    Assert.StartsWith($"# Exported {now.LocalDateTime:yyyy-MM-dd HH:mm:ss}", text);
  }

  [Fact]
  public void Export_summarizes_the_row_count_and_how_many_are_ongoing() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("+5V", SensorType.Voltage, 5.4f),    // warning, stays open → ongoing
        Board("+12V", SensorType.Voltage, 10.4f),  // critical, stays open → ongoing
    ]);
    model.ReadingsSubject.OnNext([
        Board("+5V", SensorType.Voltage, 5.02f),   // +5V recovers → closed
        Board("+12V", SensorType.Voltage, 10.4f),  // +12V still critical → ongoing
    ]);

    var text = vm.HealthEventsAsText();

    Assert.Contains("# 2 event(s), 1 ongoing", text);
  }

  [Fact]
  public void Export_count_omits_the_ongoing_clause_when_all_events_have_recovered() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 10.4f)]);   // critical, open
    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 12.02f)]);  // recovers → closed

    var text = vm.HealthEventsAsText();

    Assert.Contains("# 1 event(s)", text);
    Assert.DoesNotContain("ongoing", text);
  }

  [Fact]
  public void Health_events_export_is_empty_when_the_log_is_empty() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 12.02f)]);  // healthy → no events

    Assert.Equal("", vm.HealthEventsAsText());
  }

  [Fact]
  public void Health_events_summary_counts_total_and_ongoing() {
    var now = new DateTimeOffset(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);
    var vm = CreateVm(out var model, () => now);

    // Two distinct rails go out of spec and stay there → two ongoing episodes.
    model.ReadingsSubject.OnNext([
        Board("+12V", SensorType.Voltage, 10.4f),
        Board("+5V", SensorType.Voltage, 5.4f),
    ]);

    Assert.Equal("2 events · 2 ongoing", vm.HealthEventsSummary);
  }

  [Fact]
  public void Health_events_summary_drops_the_ongoing_clause_once_everything_recovers() {
    var now = new DateTimeOffset(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);
    var vm = CreateVm(out var model, () => now);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 10.4f)]);  // one ongoing
    Assert.Equal("1 event · 1 ongoing", vm.HealthEventsSummary);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 12.02f)]); // recovers → closed

    Assert.Equal("1 event", vm.HealthEventsSummary);
  }

  [Fact]
  public void Health_events_summary_is_empty_when_the_log_is_empty() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 12.02f)]);  // healthy

    Assert.Equal("", vm.HealthEventsSummary);
  }

  [Fact]
  public void Session_peak_names_the_worst_episode_and_survives_recovery() {
    var now = new DateTimeOffset(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);
    var vm = CreateVm(out var model, () => now);

    // +12V critical outranks the +5V warning → it's the session peak.
    model.ReadingsSubject.OnNext([
        Board("+5V", SensorType.Voltage, 5.4f),    // warning
        Board("+12V", SensorType.Voltage, 10.4f),  // critical
    ]);
    Assert.Equal("+12V 10.4", vm.SessionPeak);
    Assert.Equal(ReadingSeverity.Critical, vm.SessionPeakSeverity);

    // Everything recovers: the live rollup clears, but the session peak persists as a record.
    model.ReadingsSubject.OnNext([
        Board("+5V", SensorType.Voltage, 5.01f),
        Board("+12V", SensorType.Voltage, 12.02f),
    ]);
    Assert.Equal(ReadingSeverity.Normal, vm.BoardHealth);
    Assert.Equal("+12V 10.4", vm.SessionPeak);
    Assert.Equal(ReadingSeverity.Critical, vm.SessionPeakSeverity);
  }

  [Fact]
  public void Session_peak_is_empty_on_a_healthy_board() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 12.02f)]);

    Assert.Equal("", vm.SessionPeak);
    Assert.Equal(ReadingSeverity.Normal, vm.SessionPeakSeverity);
  }

  [Fact]
  public void Critical_only_filter_hides_warning_rows_but_keeps_the_full_count_and_peak() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("+5V", SensorType.Voltage, 5.4f),    // warning
        Board("+12V", SensorType.Voltage, 10.4f),  // critical
    ]);
    Assert.Equal(2, vm.HealthEvents.Count);

    vm.ShowCriticalOnly = true;

    // Only the critical row remains in the table...
    var shown = Assert.Single(vm.HealthEvents);
    Assert.Equal("+12V", shown.SensorName);
    Assert.Equal(ReadingSeverity.Critical, shown.Severity);
    // ...but the headline count and the tile peak still reflect the full log.
    Assert.Equal("2 events · 2 ongoing", vm.HealthEventsSummary);
    Assert.Equal("+12V 10.4", vm.SessionPeak);
    // ...and a hint reports what's being suppressed, so the table doesn't read as empty.
    Assert.Equal("1 hidden", vm.HealthEventsFilterHint);
  }

  [Fact]
  public void Filter_hint_is_empty_when_the_filter_is_off_or_hides_nothing() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("+5V", SensorType.Voltage, 5.4f),    // warning
        Board("+12V", SensorType.Voltage, 10.4f),  // critical
    ]);
    Assert.Equal("", vm.HealthEventsFilterHint);   // filter off → no hint

    vm.ShowCriticalOnly = true;
    Assert.Equal("1 hidden", vm.HealthEventsFilterHint);

    vm.ShowCriticalOnly = false;
    Assert.Equal("", vm.HealthEventsFilterHint);   // back off → hint clears
  }

  [Fact]
  public void Filter_hint_is_empty_when_all_rows_are_critical() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([Board("+12V", SensorType.Voltage, 10.4f)]);  // critical only
    vm.ShowCriticalOnly = true;

    Assert.Single(vm.HealthEvents);
    Assert.Equal("", vm.HealthEventsFilterHint);   // filter on but nothing hidden
  }

  [Fact]
  public void Toggling_the_filter_off_restores_the_hidden_rows() {
    var vm = CreateVm(out var model);

    model.ReadingsSubject.OnNext([
        Board("+5V", SensorType.Voltage, 5.4f),    // warning
        Board("+12V", SensorType.Voltage, 10.4f),  // critical
    ]);

    vm.ShowCriticalOnly = true;
    Assert.Single(vm.HealthEvents);

    vm.ShowCriticalOnly = false;
    Assert.Equal(2, vm.HealthEvents.Count);
  }

  private static FirmwareSnapshot FirmwareWith(params FirmwareComponent[] inventory) =>
      new(Manufacturer: null, Version: null, ReleaseDate: null, SerialNumber: null,
          SmbiosSpecVersion: null, PrimaryBios: null, Status: null, RomSizeBytes: null,
          IsUefi: null, BiosRevision: null, EmbeddedControllerRevision: null, Capabilities: null,
          System: null, Baseboard: null, Chassis: null, HardwareSecurity: null,
          SecureBoot: SecureBootInfo.Unknown, Tpm: TpmInfo.Absent, Boot: null,
          FirmwareInventory: inventory);

  private static FirmwareComponent Component(string name, ulong imageSizeBytes) =>
      new(ComponentName: name, Version: "1.0", ReleaseDate: null, Manufacturer: null,
          LowestSupportedVersion: null, ImageSizeBytes: imageSizeBytes,
          State: FirmwareComponentState.Enabled);

  [Fact]
  public void Firmware_component_image_size_scales_to_kb_and_mb() {
    var vm = CreateVm(out var model);

    model.FirmwareSubject.OnNext(FirmwareWith(
        Component("Small", 4096),          // 4 KB
        Component("Large", 8 * 1024 * 1024))); // 8 MB

    Assert.Equal("4 KB", vm.FirmwareInventory[0].ImageSize);
    Assert.Equal("8 MB", vm.FirmwareInventory[1].ImageSize);
  }

  [Fact]
  public void Firmware_component_without_a_reported_size_shows_a_dash() {
    var vm = CreateVm(out var model);

    model.FirmwareSubject.OnNext(FirmwareWith(Component("Unsized", 0)));

    Assert.Equal("—", vm.FirmwareInventory[0].ImageSize);
  }

  [Fact]
  public void Firmware_component_count_pluralizes_and_singularizes() {
    var vm = CreateVm(out var model);

    model.FirmwareSubject.OnNext(FirmwareWith(Component("Only", 4096)));
    Assert.True(vm.HasFirmwareInventory);
    Assert.Equal("1 component", vm.FirmwareComponentCount);

    model.FirmwareSubject.OnNext(FirmwareWith(Component("A", 4096), Component("B", 4096)));
    Assert.Equal("2 components", vm.FirmwareComponentCount);
  }

  [Fact]
  public void Firmware_component_count_is_a_dash_when_the_inventory_is_empty() {
    var vm = CreateVm(out var model);

    model.FirmwareSubject.OnNext(FirmwareWith());

    Assert.False(vm.HasFirmwareInventory);
    Assert.Equal("—", vm.FirmwareComponentCount);
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
