using System.Globalization;
using Crystal.Telemetry.Hardware;
using Xunit;

namespace Crystal.Telemetry.Tests;

public class ParameterTests {
  private static MockSensor MakeSensor() => new(new Identifier("cpu", "0"));

  private static Parameter MakeParameter(ISettings settings, float defaultValue = 5f, string name = "Offset [°C]") {
    var description = new ParameterDescription(name, "Temperature offset", defaultValue);
    return new Parameter(description, MakeSensor(), settings);
  }

  [Fact]
  public void Ctor_NoStoredValue_UsesDefaultAndIsDefault() {
    var settings = new MockSettings();
    var parameter = MakeParameter(settings, 5f);

    Assert.True(parameter.IsDefault);
    Assert.Equal(5f, parameter.Value);
    Assert.Equal(5f, parameter.DefaultValue);
  }

  [Fact]
  public void Ctor_StoredValue_LoadsFromSettings() {
    var settings = new MockSettings();
    var sensor = MakeSensor();
    var description = new ParameterDescription("Offset [°C]", "desc", 5f);
    // Compute the identifier the parameter will use, then seed the store.
    var expectedId = new Identifier(sensor.Identifier, "parameter", "offset[°c]");
    settings.SetValue(expectedId.ToString(), (12.5f).ToString(CultureInfo.InvariantCulture));

    var parameter = new Parameter(description, sensor, settings);

    Assert.False(parameter.IsDefault);
    Assert.Equal(12.5f, parameter.Value);
  }

  [Fact]
  public void Ctor_UnparseableStoredValue_FallsBackToDefault() {
    var settings = new MockSettings();
    var sensor = MakeSensor();
    var description = new ParameterDescription("Offset [°C]", "desc", 7f);
    var id = new Identifier(sensor.Identifier, "parameter", "offset[°c]");
    settings.SetValue(id.ToString(), "not-a-number");

    var parameter = new Parameter(description, sensor, settings);

    Assert.Equal(7f, parameter.Value);
  }

  [Fact]
  public void Identifier_DerivedFromSensorAndNormalizedName() {
    var settings = new MockSettings();
    var parameter = MakeParameter(settings, name: "My Offset");

    // Spaces stripped and lower-cased: "My Offset" -> "myoffset".
    Assert.Equal("/cpu/0/parameter/myoffset", parameter.Identifier.ToString());
  }

  [Fact]
  public void SetValue_PersistsToSettingsAndClearsDefault() {
    var settings = new MockSettings();
    var parameter = MakeParameter(settings, 5f);

    parameter.Value = 20f;

    Assert.False(parameter.IsDefault);
    Assert.Equal(20f, parameter.Value);
    Assert.True(settings.Contains(parameter.Identifier.ToString()));
    Assert.Equal((20f).ToString(CultureInfo.InvariantCulture), settings.GetValue(parameter.Identifier.ToString(), "0"));
  }

  [Fact]
  public void SetIsDefaultTrue_ResetsValueAndRemovesFromSettings() {
    var settings = new MockSettings();
    var parameter = MakeParameter(settings, 5f);
    parameter.Value = 20f;
    Assert.True(settings.Contains(parameter.Identifier.ToString()));

    parameter.IsDefault = true;

    Assert.True(parameter.IsDefault);
    Assert.Equal(5f, parameter.Value);
    Assert.False(settings.Contains(parameter.Identifier.ToString()));
  }

  [Fact]
  public void NameAndDescription_ExposedFromDescription() {
    var settings = new MockSettings();
    var description = new ParameterDescription("MyName", "MyDescription", 1f);
    var parameter = new Parameter(description, MakeSensor(), settings);

    Assert.Equal("MyName", parameter.Name);
    Assert.Equal("MyDescription", parameter.Description);
  }
}
