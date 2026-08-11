using System.Linq;
using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

// Exercises the pure value-tracking logic of Sensor.Value — the min/max ratchet (with NaN/Infinity
// guards), reset behaviour, and the display-name fallback — without any hardware. Sensor needs a
// concrete Hardware to subscribe to its Closing event, so TestHardware supplies one.
public class SensorTests {
  private static Sensor NewSensor(string name = "Core #1", MockSettings? settings = null) {
    settings ??= new MockSettings();
    return new Sensor(name, 0, SensorType.Temperature, new TestHardware(settings), settings);
  }

  [Fact]
  public void Value_setter_stores_current_value() {
    var sensor = NewSensor();

    sensor.Value = 42f;

    Assert.Equal(42f, sensor.Value);
  }

  [Fact]
  public void Min_and_max_ratchet_across_successive_readings() {
    var sensor = NewSensor();

    sensor.Value = 50f;
    sensor.Value = 30f;
    sensor.Value = 70f;
    sensor.Value = 45f;

    Assert.Equal(30f, sensor.Min);
    Assert.Equal(70f, sensor.Max);
    Assert.Equal(45f, sensor.Value);
  }

  [Fact]
  public void NaN_reading_does_not_disturb_min_max() {
    var sensor = NewSensor();

    sensor.Value = 40f;
    sensor.Value = float.NaN;

    // NaN is a "no reading" sentinel; it must not become the new min/max.
    Assert.Equal(40f, sensor.Min);
    Assert.Equal(40f, sensor.Max);
  }

  [Fact]
  public void Infinity_reading_does_not_disturb_min_max() {
    var sensor = NewSensor();

    sensor.Value = 40f;
    sensor.Value = float.PositiveInfinity;
    sensor.Value = float.NegativeInfinity;

    Assert.Equal(40f, sensor.Min);
    Assert.Equal(40f, sensor.Max);
  }

  [Fact]
  public void Null_reading_leaves_min_max_untouched() {
    var sensor = NewSensor();

    sensor.Value = 40f;
    sensor.Value = null;

    Assert.Equal(40f, sensor.Min);
    Assert.Equal(40f, sensor.Max);
    Assert.Null(sensor.Value);
  }

  [Fact]
  public void ResetMin_and_ResetMax_clear_the_tracked_extremes() {
    var sensor = NewSensor();
    sensor.Value = 10f;
    sensor.Value = 90f;

    sensor.ResetMin();
    sensor.ResetMax();

    Assert.Null(sensor.Min);
    Assert.Null(sensor.Max);
  }

  [Fact]
  public void Min_max_start_null_before_any_reading() {
    var sensor = NewSensor();

    Assert.Null(sensor.Min);
    Assert.Null(sensor.Max);
    Assert.Null(sensor.Value);
  }

  [Fact]
  public void Name_defaults_to_the_constructor_name() {
    Assert.Equal("Core #1", NewSensor(name: "Core #1").Name);
  }

  [Fact]
  public void Setting_empty_name_falls_back_to_the_default_name() {
    var sensor = NewSensor(name: "Package");

    sensor.Name = "Custom";
    Assert.Equal("Custom", sensor.Name);

    sensor.Name = "";
    // An empty assignment reverts to the original default rather than blanking the label.
    Assert.Equal("Package", sensor.Name);
  }

  [Fact]
  public void Identifier_is_derived_from_hardware_type_and_index() {
    var sensor = NewSensor();

    // "/test/hardware" + "/temperature" + "/0".
    Assert.Equal("/test/hardware/temperature/0", sensor.Identifier.ToString());
  }

  [Fact]
  public void Value_accumulates_a_rolling_average_sample_every_four_readings() {
    var sensor = NewSensor();

    // The setter buffers 4 readings then appends their average as one history sample.
    sensor.Value = 10f;
    sensor.Value = 20f;
    sensor.Value = 30f;
    sensor.Value = 40f;

    var sample = Assert.Single(sensor.Values);
    Assert.Equal(25f, sample.Value);   // (10+20+30+40)/4
  }

  [Fact]
  public void Value_buffers_below_four_readings_without_emitting_a_sample() {
    var sensor = NewSensor();

    sensor.Value = 10f;
    sensor.Value = 20f;
    sensor.Value = 30f;

    Assert.Empty(sensor.Values);
  }
}
