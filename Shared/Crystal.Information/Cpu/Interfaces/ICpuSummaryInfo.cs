using Crystal.Information.Cpu.Implementations;

namespace Crystal.Information.Cpu.Interfaces;

public interface ICpuSummaryInfo {
  string? BrandName { get; set; }
  string? VendorName { get; set; }
  int? FamilyId { get; set; }
  int? ModelId { get; set; }
  int? SteppingId { get; set; }
  float? BaseSpeed { get; set; }
  float? BusSpeed { get; set; }
  int? SocketNum { get; set; }
  int? PhysicalCoreNum { get; set; }
  int? LogicalCoreNum { get; set; }
  bool? Virtualization { get; set; }
  CpuCacheInfo? CacheInfo { get; set; }
  CpuInstructionInfo? InstructionSet { get; set; }
}
