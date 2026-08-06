using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using System.Collections.Generic;

namespace Crystal.Infrastructure.DataStructures.Cpu.Implementations;

public class SystemCpuInfo : ISystemCpuInfo {
  public SystemCpuInfo(IReadOnlyList<ICpuInfo> sockets) => Sockets = sockets;

  public IReadOnlyList<ICpuInfo> Sockets { get; }
}
