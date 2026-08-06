using Crystal.Infrastructure.DataStructures.Cpu.Definitions;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;

namespace Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;

public class CpuSpecs : ICpuSpecs {
  public string? BrandName { get; init; }
  public string? VendorName { get; init; }
  public int? FamilyId { get; init; }
  public int? ModelId { get; init; }
  public int? SteppingId { get; init; }
  public float? BaseSpeed { get; init; }
  public float? BusSpeed { get; init; }
  public int? SocketNum { get; init; }
  public int? PhysicalCoreNum { get; init; }
  public int? LogicalCoreNum { get; init; }
  public bool? VirtualizationSupported { get; init; }
  public bool? VirtualizationEnabled { get; init; }
  public CpuCacheInfo? CacheInfo { get; init; }
  public CpuInstructionInfo? InstructionSet { get; init; }
}
