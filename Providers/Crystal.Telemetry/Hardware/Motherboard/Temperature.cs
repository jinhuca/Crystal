namespace Crystal.Telemetry.Hardware.Motherboard;

internal class Temperature {
  public readonly int Index;
  public readonly string Name;

  public Temperature(string name, int index) {
    Name = name;
    Index = index;
  }
}