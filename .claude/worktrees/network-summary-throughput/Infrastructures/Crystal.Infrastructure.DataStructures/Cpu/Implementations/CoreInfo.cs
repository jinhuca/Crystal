using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;

namespace Crystal.Infrastructure.DataStructures.Cpu.Implementations;

public class CoreInfo : ICoreInfo {
  public CoreInfo(ICoreSpecs specs, ICoreSensors sensors) {
    Specs = specs;
    Sensors = sensors;
  }

  public ICoreSpecs Specs { get; }
  public ICoreSensors Sensors { get; }
}
