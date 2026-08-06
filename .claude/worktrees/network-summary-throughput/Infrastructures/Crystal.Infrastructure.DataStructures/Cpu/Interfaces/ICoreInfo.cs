using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

public interface ICoreInfo {
  ICoreSpecs Specs { get; }
  ICoreSensors Sensors { get; }
}