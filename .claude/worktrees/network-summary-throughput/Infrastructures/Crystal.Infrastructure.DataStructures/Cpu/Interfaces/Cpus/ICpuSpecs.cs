using Crystal.Infrastructure.DataStructures.Cpu.Definitions;

namespace Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus; 

public interface ICpuSpecs {
  string? BrandName { get; init; }
  string? VendorName { get; init; }
  int? FamilyId { get; init; }
  int? ModelId { get; init; }
  int? SteppingId { get; init; }
  float? BaseSpeed { get; init; }
  float? BusSpeed { get; init; }
  int? SocketNum { get; init; }
  int? PhysicalCoreNum { get; init; }
  int? LogicalCoreNum { get; init; }
  bool? VirtualizationSupported { get; init; }
  bool? VirtualizationEnabled { get; init; }
  CpuCacheInfo? CacheInfo { get; init; }
  CpuInstructionInfo? InstructionSet { get; init; }
}
