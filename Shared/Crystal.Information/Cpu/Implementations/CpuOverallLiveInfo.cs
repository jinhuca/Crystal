using Crystal.Information.Cpu.Interfaces;
using Crystal.Information.Types;

namespace Crystal.Information.Cpu.Implementations;

public class CpuOverallLiveInfo : ICpuOverallLiveInfo {
  public SensorReading BusSpeed { get; set; }
  public SensorReading CpuSpeed { get; set; }
  public SensorReading Voltage { get; set; }
  public SensorReading PlatformPower { get; set; }
  public SensorReading PackagePower { get; set; }
  public SensorReading MemoryPower { get; set; }
  public SensorReading CoresPower { get; set; }
  public SensorReading PackageTemperature { get; set; }
  public SensorReading CoreMaxTemperature { get; set; }
  public SensorReading CoreAvgTemperature { get; set; }
  public SensorReading TotalLoad { get; set; }
  public SensorReading CoreMaxLoad { get; set; }
}