using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

public interface ISystemCpuInfo {
  IReadOnlyList<ICpuInfo> Sockets { get; }
}