using Crystal.DataStructures.Sensors;

namespace Crystal.DataStructures.Cpu.Interfaces.Cpus;

public interface ICpuSensors {
  SensorReading BusSpeed { get; set; }
  SensorReading CpuSpeed { get; set; }
  SensorReading Voltage { get; set; }
  SensorReading PlatformPower { get; set; }
  SensorReading PackagePower { get; set; }
  SensorReading MemoryPower { get; set; }
  SensorReading CoresPower { get; set; }
  SensorReading PackageTemperature { get; set; }
  SensorReading CoreMaxTemperature { get; set; }
  SensorReading CoreAvgTemperature { get; set; }
  SensorReading TotalLoad { get; set; }
  SensorReading CoreMaxLoad { get; set; }
}
