using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using System.Collections.ObjectModel;

namespace Crystal.CpuModule.ViewModels.Interfaces;

/// <summary>
/// Static CPU inventory shown in the header row and the instruction-set grid:
/// brand/vendor/family, topology, cache sizes, virtualization and the ISA flags.
/// Populated once from the specs stream.
/// </summary>
public interface ICpuSpecsViewModel {
  /// <summary>
  /// The CPU vendor string, e.g. "GenuineIntel" or "AuthenticAMD".
  /// </summary>
  string? Vendor { get; }

  /// <summary>
  /// The CPU brand string, e.g. "Intel(R) Core(TM) i7-9700K CPU @ 3.60GHz".
  /// </summary>
  string? Brand { get; }

  /// <summary>
  /// The CPU socket number, e.g. 0 for a single-socket system, 1 for the second socket in a dual-socket system, etc.
  /// </summary>
  int? Socket { get; }

  /// <summary>
  /// The number of physical cores this CPU reports. Zero when not exposed.
  /// </summary>
  int? PhysicalCores { get; }

  /// <summary>
  /// The number of logical cores this CPU reports. Zero when not exposed.
  /// </summary>
  int? LogicalCores { get; }

  /// <summary>
  /// The CPU family number, e.g. 6 for Intel Core and AMD Ryzen. Zero when not exposed.
  /// </summary>
  int? Family { get; }

  /// <summary>
  /// The CPU model number, e.g. 158 for Intel Coffee Lake. Zero when not exposed.
  /// </summary>
  int? Model { get; }

  /// <summary>
  /// The CPU stepping number, e.g. 10 for Intel Coffee Lake. Zero when not exposed.
  /// </summary>
  int? Stepping { get; }

  /// <summary>
  /// The CPU base clock in MHz, e.g. 3600 for a 3.6 GHz CPU. Zero when not exposed.
  /// </summary>
  float? BaseSpeedMHz { get; }

  /// <summary>
  /// The CPU bus clock in MHz, e.g. 100 for a 3.6 GHz CPU with a 36x multiplier. Zero when not exposed.
  /// </summary>
  float? BusSpeedMHz { get; }

  /// <summary>
  /// True if this CPU reports that it supports virtualization (Intel VT-x or AMD-V). False if it reports that it does not. 
  /// Null if the information is not exposed.
  /// </summary>
  bool? Virtualization { get; }

  /// <summary>
  /// The CPU cache sizes in KB. Zero when not exposed.
  /// </summary>
  double L1CacheKb { get; }

  /// <summary>
  /// The CPU cache sizes in KB. Zero when not exposed.
  /// </summary>
  double L2CacheKb { get; }

  /// <summary>
  /// The CPU cache sizes in KB. Zero when not exposed.
  /// </summary>
  double L3CacheKb { get; }

  /// <summary>
  /// The CPU cache line size in bytes. Zero when not exposed.
  /// </summary>
  int LineSizeBytes { get; }

  /// <summary>
  /// The instruction set flags this CPU reports, in a grid of name + availability.
  /// </summary>
  ObservableCollection<InstructionFlag> InstructionSet { get; }

  /// <summary>
  /// Populates every property from the socket's static specs.
  /// </summary>
  void Update(ISystemCpuInfo info);
}
