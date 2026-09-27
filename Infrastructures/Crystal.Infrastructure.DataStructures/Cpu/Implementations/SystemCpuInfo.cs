using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

namespace Crystal.Infrastructure.DataStructures.Cpu.Implementations;

public class SystemCpuInfo : ISystemCpuInfo {
  public SystemCpuInfo(IReadOnlyList<ICpuInfo> sockets) => Sockets = sockets;

  public IReadOnlyList<ICpuInfo> Sockets { get; }
}
