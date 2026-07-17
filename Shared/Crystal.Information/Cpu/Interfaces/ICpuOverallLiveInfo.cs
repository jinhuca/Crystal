using Crystal.Information.TypeDefinitions;
using Crystal.Information.Types;

namespace Crystal.Information.Cpu.Interfaces;

public interface ICpuOverallLiveInfo {
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
