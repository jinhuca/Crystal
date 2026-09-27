using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;

namespace Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

public interface ICoreInfo {
  ICoreSpecs Specs { get; }
  ICoreSensors Sensors { get; }
}