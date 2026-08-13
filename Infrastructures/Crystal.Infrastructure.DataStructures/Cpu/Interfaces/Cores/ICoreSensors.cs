using Crystal.Infrastructure.DataStructures.Sensors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;

public interface ICoreSensors {
  string Name { get; set; }
  SensorReading Voltage { get; set; }
  SensorReading Speed { get; set; }
  SensorReading EffectiveSpeed { get; set; }
  SensorReading Multiplier { get; set; }
  SensorReading Temperature { get; set; }

  /// <summary>This core's distance to TjMax in °C (thermal headroom). Intel-only; empty otherwise.</summary>
  SensorReading DistanceToTjMax { get; set; }

  /// <summary>This core's package power in W. AMD-only (from the SMU); empty otherwise.</summary>
  SensorReading Power { get; set; }

  SensorReading Load { get; set; }

  /// <summary>Per-logical-thread load (%). One entry per thread on this core; a single-threaded
  /// core has one entry equal to the core load.</summary>
  IReadOnlyList<SensorReading> ThreadLoads { get; set; }
}