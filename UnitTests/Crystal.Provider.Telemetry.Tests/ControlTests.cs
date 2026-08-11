using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

// Control wraps a fan/pump element whose value can be left at hardware default or overridden with a
// software value. It seeds its mode/value from ISettings on construction and writes them back on
// change, firing change events. These tests pin that state machine without any hardware.
public class ControlTests {
  private static Control NewControl(MockSettings settings, float min = 0f, float max = 100f) {
    var sensor = new MockSensor(new Identifier("cpu", "0"));
    return new Control(sensor, settings, min, max);
  }

  [Fact]
  public void Ctor_derives_identifier_from_the_sensor_and_keeps_the_software_range() {
    var control = NewControl(new MockSettings(), min: 20f, max: 90f);

    Assert.Equal("/cpu/0/control", control.Identifier.ToString());
    Assert.Equal(20f, control.MinSoftwareValue);
    Assert.Equal(90f, control.MaxSoftwareValue);
  }

  [Fact]
  public void Ctor_defaults_to_undefined_mode_and_zero_value_without_settings() {
    var control = NewControl(new MockSettings());

    Assert.Equal(ControlMode.Undefined, control.ControlMode);
    Assert.Equal(0f, control.SoftwareValue);
  }

  [Fact]
  public void Ctor_seeds_mode_and_value_from_settings() {
    var settings = new MockSettings();
    settings.SetValue("/cpu/0/control/mode", ((int)ControlMode.Software).ToString());
    settings.SetValue("/cpu/0/control/value", "42.5");

    var control = NewControl(settings);

    Assert.Equal(ControlMode.Software, control.ControlMode);
    Assert.Equal(42.5f, control.SoftwareValue);
  }

  [Fact]
  public void Ctor_falls_back_to_zero_and_undefined_when_settings_are_unparseable() {
    var settings = new MockSettings();
    settings.SetValue("/cpu/0/control/mode", "not-an-int");
    settings.SetValue("/cpu/0/control/value", "not-a-float");

    var control = NewControl(settings);

    Assert.Equal(ControlMode.Undefined, control.ControlMode);
    Assert.Equal(0f, control.SoftwareValue);
  }

  [Fact]
  public void SetDefault_switches_mode_and_persists_it() {
    var settings = new MockSettings();
    var control = NewControl(settings);

    control.SetDefault();

    Assert.Equal(ControlMode.Default, control.ControlMode);
    Assert.Equal(((int)ControlMode.Default).ToString(), settings.GetValue("/cpu/0/control/mode", ""));
  }

  [Fact]
  public void SetSoftware_sets_software_mode_and_value_and_persists_both() {
    var settings = new MockSettings();
    var control = NewControl(settings);

    control.SetSoftware(75f);

    Assert.Equal(ControlMode.Software, control.ControlMode);
    Assert.Equal(75f, control.SoftwareValue);
    Assert.Equal(((int)ControlMode.Software).ToString(), settings.GetValue("/cpu/0/control/mode", ""));
    Assert.Equal("75", settings.GetValue("/cpu/0/control/value", ""));
  }

  [Fact]
  public void SetDefault_raises_ControlModeChanged_once_per_actual_change() {
    var control = NewControl(new MockSettings());
    int changes = 0;
    control.ControlModeChanged += _ => changes++;

    control.SetDefault();
    control.SetDefault(); // already Default -> no event

    Assert.Equal(1, changes);
  }

  [Fact]
  public void SetSoftware_raises_SoftwareControlValueChanged_only_when_the_value_moves() {
    var control = NewControl(new MockSettings());
    int valueChanges = 0;
    control.SoftwareControlValueChanged += _ => valueChanges++;

    control.SetSoftware(30f);
    control.SetSoftware(30f); // unchanged value -> no event
    control.SetSoftware(60f);

    Assert.Equal(2, valueChanges);
    Assert.Equal(60f, control.SoftwareValue);
  }

  [Fact]
  public void Sensor_is_the_one_passed_to_the_constructor() {
    var sensor = new MockSensor(new Identifier("gpu", "1"));

    var control = new Control(sensor, new MockSettings(), 0f, 100f);

    Assert.Same(sensor, control.Sensor);
  }
}
