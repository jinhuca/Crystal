using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;

namespace Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

public interface ICpuInfo {
  int SocketIndex { get; }              // 0, 1, 2... — stable ordinal, safe to use as a dictionary/array key
  string? SocketDesignation { get; }    // SMBIOS Type 4's "Socket Designation" - e.g. "CPU1", "Proc 2" - UI label
  ICpuSpecs Specs { get; }
  ICpuSensors Sensors { get; }
  IReadOnlyList<ICoreInfo> Cores { get; }
}
