using Crystal.Infrastructure.DataStructures.Sensors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;

public interface ICoreSensors {
  string Name { get; set; }
  SensorReading Voltage { get; set; }
  SensorReading Speed { get; set; }
  SensorReading Temperature { get; set; }
  SensorReading Load { get; set; }
}