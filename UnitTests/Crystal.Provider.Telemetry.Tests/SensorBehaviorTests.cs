using Crystal.Provider.Telemetry.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

// Covers Sensor behaviour beyond the value/min-max ratchet already pinned by SensorTests: settings
// interaction (name seeding + history round-trip on close), the history time-window, ClearValues,
// visitor dispatch, parameter construction/traversal, and the disableHistory path.
public class SensorBehaviorTests {
  private static Sensor NewSensor(
      MockSettings settings,
      string name = "Core #1",
      ParameterDescription[]? parameters = null,
      bool disableHistory = false) =>
      new(name, 0, false, SensorType.Temperature, new TestHardware(settings), parameters, settings, disableHistory);

  [Fact]
  public void Name_is_seeded_from_settings_over_the_default() {
    var settings = new MockSettings();
    // Sensor identifier is /test/hardware/temperature/0; the persisted name lives under .../name.
    settings.SetValue("/test/hardware/temperature/0/name", "Persisted Label");

    var sensor = NewSensor(settings, name: "Core #1");

    Assert.Equal("Persisted Label", sensor.Name);
  }

  [Fact]
  public void Setting_the_name_persists_it_to_settings() {
    var settings = new MockSettings();
    var sensor = NewSensor(settings);

    sensor.Name = "Custom Name";

    Assert.Equal("Custom Name", settings.GetValue("/test/hardware/temperature/0/name", ""));
  }

  [Fact]
  public void Setting_ValuesTimeWindow_to_zero_clears_the_history() {
    var settings = new MockSettings();
    var sensor = NewSensor(settings);
    for (int i = 0; i < 4; i++) sensor.Value = 10f; // emits one history sample
    Assert.NotEmpty(sensor.Values);

    sensor.ValuesTimeWindow = TimeSpan.Zero;

    Assert.Empty(sensor.Values);
    Assert.Equal(TimeSpan.Zero, sensor.ValuesTimeWindow);
  }

  [Fact]
  public void ClearValues_drops_history_but_keeps_the_current_value() {
    var settings = new MockSettings();
    var sensor = NewSensor(settings);
    for (int i = 0; i < 4; i++) sensor.Value = 20f;
    Assert.NotEmpty(sensor.Values);

    sensor.ClearValues();

    Assert.Empty(sensor.Values);
    Assert.Equal(20f, sensor.Value);
  }

  [Fact]
  public void Accept_dispatches_to_the_visitor() {
    var settings = new MockSettings();
    var sensor = NewSensor(settings);
    ISensor? visited = null;
    var visitor = new SensorVisitor(s => visited = s);

    sensor.Accept(visitor);

    Assert.Same(sensor, visited);
  }

  [Fact]
  public void Accept_null_visitor_throws() {
    var sensor = NewSensor(new MockSettings());

    Assert.Throws<ArgumentNullException>(() => sensor.Accept(null!));
  }

  [Fact]
  public void Parameters_are_built_from_their_descriptions() {
    var settings = new MockSettings();
    var descriptions = new[] {
      new ParameterDescription("Offset", "Temperature offset", 5f),
      new ParameterDescription("Scale", "Multiplier", 2f),
    };

    var sensor = NewSensor(settings, parameters: descriptions);

    Assert.Equal(2, sensor.Parameters.Count);
    Assert.Equal("Offset", sensor.Parameters[0].Name);
    Assert.Equal(5f, sensor.Parameters[0].DefaultValue);
    Assert.Equal("Scale", sensor.Parameters[1].Name);
  }

  [Fact]
  public void Traverse_visits_every_parameter() {
    var settings = new MockSettings();
    var descriptions = new[] {
      new ParameterDescription("Offset", "d", 0f),
      new ParameterDescription("Scale", "d", 1f),
    };
    var sensor = NewSensor(settings, parameters: descriptions);
    var visited = new List<string>();
    var visitor = new SensorVisitor(_ => { }); // parameters call VisitParameter, not the sensor handler

    // Traverse forwards to each parameter's Accept; assert all parameters are reached via the
    // visitor by counting parameter visits through a recording visitor.
    var recorder = new RecordingVisitor(visited);
    sensor.Traverse(recorder);

    Assert.Equal(new[] { "Offset", "Scale" }, visited);
  }

  [Fact]
  public void DisableHistory_stops_min_max_tracking_and_history() {
    var settings = new MockSettings();
    var sensor = NewSensor(settings, disableHistory: true);

    for (int i = 0; i < 8; i++) sensor.Value = 50f;

    Assert.Null(sensor.Min);
    Assert.Null(sensor.Max);
    Assert.Empty(sensor.Values);
    Assert.Equal(TimeSpan.Zero, sensor.ValuesTimeWindow);
  }

  [Fact]
  public void History_survives_a_close_and_reload_round_trip_through_settings() {
    var settings = new MockSettings();
    var hardware = new TestHardware(settings);
    var sensor = new Sensor("Core #1", 0, false, SensorType.Temperature, hardware, null, settings);
    for (int i = 0; i < 4; i++) sensor.Value = 30f; // one persisted sample
    float saved = sensor.Values.Single().Value;

    hardware.Close(); // fires Closing -> Sensor serializes its history into settings

    // A fresh sensor on the same settings reads the persisted history back on construction.
    var reloaded = new Sensor("Core #1", 0, false, SensorType.Temperature, new TestHardware(settings), null, settings);

    Assert.Contains(reloaded.Values, v => v.Value == saved);
  }

  // Visitor that records the names of every parameter it visits.
  private sealed class RecordingVisitor : IVisitor {
    private readonly List<string> _names;
    public RecordingVisitor(List<string> names) => _names = names;
    public void VisitComputer(IComputer computer) { }
    public void VisitHardware(IHardware hardware) { }
    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) => _names.Add(parameter.Name);
  }
}
