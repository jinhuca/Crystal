namespace Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

public interface ISystemCpuInfo {
  IReadOnlyList<ICpuInfo> Sockets { get; }
}