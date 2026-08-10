using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Service.Sensors.Tests;

/// <summary>
/// Minimal <see cref="ISensor"/> stand-in for exercising the telemetry->reading boundary mapper.
/// Only the fields the mapper actually reads (Name, SensorType, Value, Min, Max) are meaningful;
/// the rest satisfy the interface.
/// </summary>
internal sealed class StubSensor : ISensor {
  public string Name { get; set; } = string.Empty;
  public SensorType SensorType { get; set; }
  public float? Value { get; set; }
  public float? Min { get; set; }
  public float? Max { get; set; }

  public IControl Control => null!;
  public IHardware Hardware => null!;
  public Identifier Identifier => new("stub", "sensor");
  public int Index => 0;
  public bool IsDefaultHidden => false;
  public IReadOnlyList<IParameter> Parameters => Array.Empty<IParameter>();
  public IEnumerable<SensorValue> Values => Array.Empty<SensorValue>();
  public TimeSpan ValuesTimeWindow { get; set; }

  public void ResetMin() { }
  public void ResetMax() { }
  public void ClearValues() { }
  public void Accept(IVisitor visitor) { }
  public void Traverse(IVisitor visitor) { }
}
