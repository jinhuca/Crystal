using Crystal.DataStructures.Sensors;

namespace Crystal.DataStructures.Cpu.Interfaces.CpuCore; 
public interface ICoreSensors {
  string Name { get; set; }
  SensorReading Voltage { get; set; }
  SensorReading Speed { get; set; }
  SensorReading Temperature { get; set; }
  SensorReading Load { get; set; }
}
