using System.Collections.ObjectModel;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

namespace Crystal.CpuModule.ViewModels.Interfaces;

/// <summary>
/// Static CPU inventory shown in the header row and the instruction-set grid:
/// brand/vendor/family, topology, cache sizes, virtualization and the ISA flags.
/// Populated once from the specs stream.
/// </summary>
public interface ICpuSpecsViewModel {
  string? Vendor { get; }
  string? Brand { get; }
  int? Socket { get; }
  int? PhysicalCores { get; }
  int? LogicalCores { get; }
  int? Family { get; }
  int? Model { get; }
  int? Stepping { get; }
  float? BaseSpeedMHz { get; }
  float? BusSpeedMHz { get; }
  bool? Virtualization { get; }

  double L1CacheKb { get; }
  double L2CacheKb { get; }
  double L3CacheKb { get; }
  int LineSizeBytes { get; }

  ObservableCollection<InstructionFlag> InstructionSet { get; }

  /// <summary>Populates every property from the socket's static specs.</summary>
  void Update(ISystemCpuInfo info);
}
