using System.Linq;
using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

// CompositeSensor derives its Value by folding a reducer over a set of component sensors (e.g. the
// Corsair PSU's "Total Output" = sum of per-rail power). These tests pin the fold behaviour.
public class CompositeSensorTests {
  private static CompositeSensor Composite(
      System.Func<float, ISensor, float> reducer, float seed, params float?[] componentValues) {
    var settings = new MockSettings();
    var hardware = new TestHardware(settings);
    var components = componentValues
        .Select((v, i) => {
          var s = new Sensor($"C{i}", i, SensorType.Power, hardware, settings);
          s.Value = v;
          return (ISensor)s;
        })
        .ToArray();
    return new CompositeSensor("Total", 99, SensorType.Power, hardware, settings, components, reducer, seed);
  }

  [Fact]
  public void Value_sums_component_values() {
    var composite = Composite((acc, s) => acc + (s.Value ?? 0), seed: 0f, 10f, 20f, 30f);

    Assert.Equal(60f, composite.Value);
  }

  [Fact]
  public void Value_respects_a_nonzero_seed() {
    var composite = Composite((acc, s) => acc + (s.Value ?? 0), seed: 100f, 1f, 2f);

    Assert.Equal(103f, composite.Value);
  }

  [Fact]
  public void Value_recomputes_when_a_component_changes() {
    var settings = new MockSettings();
    var hardware = new TestHardware(settings);
    var component = new Sensor("C0", 0, SensorType.Power, hardware, settings) { Value = 5f };
    var composite = new CompositeSensor(
        "Total", 1, SensorType.Power, hardware, settings, [component],
        (acc, s) => acc + (s.Value ?? 0), 0f);

    Assert.Equal(5f, composite.Value);

    component.Value = 25f;
    // The composite reads live from its components, so a later change is reflected without rebuild.
    Assert.Equal(25f, composite.Value);
  }

  [Fact]
  public void Value_of_no_components_is_the_seed() {
    var composite = Composite((acc, s) => acc + (s.Value ?? 0), seed: 7f);

    Assert.Equal(7f, composite.Value);
  }

  [Fact]
  public void Reducer_can_compute_a_max_fold() {
    var composite = Composite(
        (acc, s) => System.Math.Max(acc, s.Value ?? float.MinValue), seed: float.MinValue, 12f, 40f, 7f);

    Assert.Equal(40f, composite.Value);
  }
}
