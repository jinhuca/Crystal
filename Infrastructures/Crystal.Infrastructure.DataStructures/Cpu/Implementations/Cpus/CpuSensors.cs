using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Infrastructure.DataStructures.Sensors;

namespace Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;

public class CpuSensors : ICpuSensors {
  private static readonly SensorReading Empty =
      new(string.Empty, HardwareType.Cpu, string.Empty, SensorType.Load, null, null, null, null);

  public SensorReading CpuSpeed { get; set; } = Empty;
  public SensorReading CpuEffectiveSpeed { get; set; } = Empty;
  public SensorReading BusSpeed { get; set; } = Empty;
  public SensorReading Voltage { get; set; } = Empty;
  public SensorReading SocVoltage { get; set; } = Empty;
  public SensorReading PlatformPower { get; set; } = Empty;
  public SensorReading PackagePower { get; set; } = Empty;
  public SensorReading MemoryPower { get; set; } = Empty;
  public SensorReading CoresPower { get; set; } = Empty;
  public SensorReading PackageTemperature { get; set; } = Empty;
  public SensorReading CoreMaxTemperature { get; set; } = Empty;
  public SensorReading CoreAvgTemperature { get; set; } = Empty;
  public SensorReading MinDistanceToTjMax { get; set; } = Empty;
  public SensorReading ThermalThrottling { get; set; } = Empty;
  public SensorReading PowerLimitThrottling { get; set; } = Empty;
  public SensorReading Prochot { get; set; } = Empty;
  public SensorReading PowerLimitLong { get; set; } = Empty;
  public SensorReading PowerLimitShort { get; set; } = Empty;
  public SensorReading Tdc { get; set; } = Empty;
  public SensorReading Edc { get; set; } = Empty;
  public SensorReading PackageC2Residency { get; set; } = Empty;
  public SensorReading PackageC3Residency { get; set; } = Empty;
  public SensorReading PackageC6Residency { get; set; } = Empty;
  public SensorReading PackageC7Residency { get; set; } = Empty;
  public SensorReading TotalLoad { get; set; } = Empty;
  public SensorReading CoreMaxLoad { get; set; } = Empty;
}
