using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using System.Collections.Generic;

namespace Crystal.Infrastructure.DataStructures.Cpu.Implementations;

public class CpuInfo : ICpuInfo {
  public CpuInfo(
      int socketIndex,
      string? socketDesignation,
      ICpuSpecs specs,
      ICpuSensors sensors,
      IReadOnlyList<ICoreInfo> cores) {
    SocketIndex = socketIndex;
    SocketDesignation = socketDesignation;
    Specs = specs;
    Sensors = sensors;
    Cores = cores;
  }

  public int SocketIndex { get; }
  public string? SocketDesignation { get; }
  public ICpuSpecs Specs { get; }
  public ICpuSensors Sensors { get; }
  public IReadOnlyList<ICoreInfo> Cores { get; }
}
