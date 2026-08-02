using Crystal.DataStructures.Cpu.Definitions;
using Crystal.DataStructures.Cpu.Interfaces.CpuCore;

namespace Crystal.DataStructures.Cpu.Interfaces; 
public interface ICpuSpecs {
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
  ICoreSpecs? CoreSpecs { get; set; }
  CpuCacheInfo? CacheInfo { get; set; }
  CpuInstructionInfo? InstructionSet { get; set; }
}
