using Crystal.Infrastructure.DataStructures.Cpu.Definitions;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;

namespace Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cores;

public class CoreSpecs : ICoreSpecs {
  public int CoreIndex { get; init; }
  public int? ApicId { get; init; }
  public CoreType? Type { get; init; }
  public int? ThreadCount { get; init; }
  public float? MaxTurboFrequency { get; init; }
}
