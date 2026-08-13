using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;
using Crystal.Infrastructure.DataStructures.Sensors;
using System.Collections.Generic;

namespace Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cores;

public class CoreSensors : ICoreSensors {
  private static readonly SensorReading Empty =
      new(string.Empty, HardwareType.Cpu, string.Empty, SensorType.Load, null, null, null, null);

  public string Name { get; set; } = string.Empty;
  public SensorReading Voltage { get; set; } = Empty;
  public SensorReading Speed { get; set; } = Empty;
  public SensorReading EffectiveSpeed { get; set; } = Empty;
  public SensorReading Multiplier { get; set; } = Empty;
  public SensorReading Temperature { get; set; } = Empty;
  public SensorReading DistanceToTjMax { get; set; } = Empty;
  public SensorReading Power { get; set; } = Empty;
  public SensorReading Load { get; set; } = Empty;
  public IReadOnlyList<SensorReading> ThreadLoads { get; set; } = [];
}
