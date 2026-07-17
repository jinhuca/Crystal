using Crystal.Information.TypeDefinitions;
using Crystal.Information.Types;

namespace Crystal.Information.Cpu.Interfaces;

public interface ICpuCoreLiveInfo {
  string Name { get; set; }
  SensorReading Voltage { get; set; }
  SensorReading Speed { get; set; }
  SensorReading Temperature { get; set; }
  SensorReading Load { get; set; }
}
