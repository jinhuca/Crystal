using Crystal.Infrastructure.DataStructures.Sensors;

namespace Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;

public interface ICpuSensors {
  SensorReading CpuSpeed { get; set; }
  SensorReading CpuEffectiveSpeed { get; set; }

  /// <summary>Reference/base clock (BCLK) in MHz — the ~100 MHz bus the core multiplier scales.
  /// Empty when not exposed.</summary>
  SensorReading BusSpeed { get; set; }
  SensorReading Voltage { get; set; }
  SensorReading SocVoltage { get; set; }
  SensorReading PlatformPower { get; set; }
  SensorReading PackagePower { get; set; }
  SensorReading MemoryPower { get; set; }
  SensorReading CoresPower { get; set; }
  SensorReading PackageTemperature { get; set; }
  SensorReading CoreMaxTemperature { get; set; }
  SensorReading CoreAvgTemperature { get; set; }

  /// <summary>Smallest per-core distance to TjMax in °C — the hottest core's thermal headroom
  /// before throttling. Intel-only; empty on parts that don't expose it.</summary>
  SensorReading MinDistanceToTjMax { get; set; }

  /// <summary>Package thermal-throttling flag (1 = active, 0 = inactive). Intel-only; empty when
  /// not exposed.</summary>
  SensorReading ThermalThrottling { get; set; }

  /// <summary>Package power-limit-throttling flag (PL1/PL2; 1 = active). Intel-only; empty when
  /// not exposed.</summary>
  SensorReading PowerLimitThrottling { get; set; }

  /// <summary>PROCHOT#/FORCEPR# event flag (1 = active). Intel-only; empty when not exposed.</summary>
  SensorReading Prochot { get; set; }

  /// <summary>Configured RAPL long-term/sustained package power limit (PL1) in W. Intel-only;
  /// empty when not exposed.</summary>
  SensorReading PowerLimitLong { get; set; }

  /// <summary>Configured RAPL short-term/burst package power limit (PL2) in W. Intel-only;
  /// empty when not exposed.</summary>
  SensorReading PowerLimitShort { get; set; }

  /// <summary>Thermal Design Current (TDC) in A — sustained current the VRM delivers. AMD-only
  /// (from the SMU); empty otherwise.</summary>
  SensorReading Tdc { get; set; }

  /// <summary>Electrical Design Current (EDC) in A — peak/burst current the VRM delivers. AMD-only
  /// (from the SMU); empty otherwise.</summary>
  SensorReading Edc { get; set; }

  /// <summary>Package C2 idle-state residency as a percentage of the last poll interval. Empty
  /// on parts that don't expose the residency counter.</summary>
  SensorReading PackageC2Residency { get; set; }

  /// <summary>Package C3 idle-state residency as a percentage of the last poll interval. Empty
  /// on parts that don't expose the residency counter.</summary>
  SensorReading PackageC3Residency { get; set; }

  /// <summary>Package C6 idle-state residency as a percentage of the last poll interval. Empty
  /// on parts that don't expose the residency counter.</summary>
  SensorReading PackageC6Residency { get; set; }

  /// <summary>Package C7 idle-state residency as a percentage of the last poll interval. Empty
  /// on parts that don't expose the residency counter.</summary>
  SensorReading PackageC7Residency { get; set; }

  SensorReading TotalLoad { get; set; }
  SensorReading CoreMaxLoad { get; set; }
}
