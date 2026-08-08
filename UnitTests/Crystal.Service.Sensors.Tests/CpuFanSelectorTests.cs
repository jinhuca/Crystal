using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Service.Sensors.Tests;

public class CpuFanSelectorTests {
  private static SensorReading Fan(HardwareType hardware, string name, float? rpm) =>
      FakeSensorTelemetrySource.Reading(hardware, name, SensorType.Fan, rpm);

  [Fact]
  public void Picks_the_cpu_fan_over_chassis_fans() {
    var snapshot = new SensorSnapshot([
        Fan(HardwareType.SuperIO, "Chassis Fan #1", 800f),
        Fan(HardwareType.SuperIO, "CPU Fan", 1450f),
        Fan(HardwareType.SuperIO, "System Fan", 600f),
    ]);

    Assert.Equal(1450f, CpuFanSelector.SelectRpm(snapshot));
  }

  [Fact]
  public void Also_matches_cooler_hosted_cpu_fans() {
    var snapshot = new SensorSnapshot([
        Fan(HardwareType.Cooler, "CPU Fan #1", 1200f),
    ]);

    Assert.Equal(1200f, CpuFanSelector.SelectRpm(snapshot));
  }

  [Fact]
  public void Highest_reading_wins_when_several_cpu_fans_match() {
    // A dual-header board reports "CPU Fan #1" spinning and "#2" idle (0) on an empty header.
    var snapshot = new SensorSnapshot([
        Fan(HardwareType.SuperIO, "CPU Fan #1", 0f),
        Fan(HardwareType.SuperIO, "CPU Fan #2", 1350f),
    ]);

    Assert.Equal(1350f, CpuFanSelector.SelectRpm(snapshot));
  }

  [Fact]
  public void A_named_cpu_fan_wins_even_when_a_generic_fan_spins_faster() {
    // The name match takes priority: a slower "CPU Fan" is still the CPU fan, not the faster chassis fan.
    var snapshot = new SensorSnapshot([
        Fan(HardwareType.SuperIO, "Chassis Fan #1", 1800f),
        Fan(HardwareType.SuperIO, "CPU Fan", 1200f),
    ]);

    Assert.Equal(1200f, CpuFanSelector.SelectRpm(snapshot));
  }

  [Fact]
  public void Falls_back_to_the_fastest_fan_when_none_is_named_cpu() {
    // Some boards name every header generically ("Fan #1"/"Fan #2"). The CPU fan is almost always
    // the fastest-spinning one, so fall back to it rather than showing nothing.
    var snapshot = new SensorSnapshot([
        Fan(HardwareType.SuperIO, "Fan #1", 800f),
        Fan(HardwareType.SuperIO, "Fan #2", 1450f),
        Fan(HardwareType.SuperIO, "Fan #3", 600f),
    ]);

    Assert.Equal(1450f, CpuFanSelector.SelectRpm(snapshot));
  }

  [Fact]
  public void Fallback_ignores_idle_headers_reporting_zero() {
    var snapshot = new SensorSnapshot([
        Fan(HardwareType.SuperIO, "Fan #1", 0f),
        Fan(HardwareType.SuperIO, "Fan #2", 900f),
    ]);

    Assert.Equal(900f, CpuFanSelector.SelectRpm(snapshot));
  }

  [Fact]
  public void Returns_null_when_no_fan_is_spinning() {
    var snapshot = new SensorSnapshot([
        Fan(HardwareType.SuperIO, "Fan #1", 0f),
        Fan(HardwareType.SuperIO, "Fan #2", null),
    ]);

    Assert.Null(CpuFanSelector.SelectRpm(snapshot));
  }

  [Fact]
  public void Ignores_non_fan_cpu_sensors() {
    // A CPU temperature reading must not be mistaken for a fan just because its name says "CPU".
    var snapshot = new SensorSnapshot([
        FakeSensorTelemetrySource.Reading(HardwareType.Cpu, "CPU Package", SensorType.Temperature, 65f),
    ]);

    Assert.Null(CpuFanSelector.SelectRpm(snapshot));
  }

  [Fact]
  public void Skips_cpu_fan_headers_reporting_no_value() {
    var snapshot = new SensorSnapshot([
        Fan(HardwareType.SuperIO, "CPU Fan", null),
    ]);

    Assert.Null(CpuFanSelector.SelectRpm(snapshot));
  }
}
