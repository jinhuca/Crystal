using Crystal.CpuModule.ViewModels.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Definitions;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using System.Collections.ObjectModel;

namespace Crystal.CpuModule.ViewModels.Implementations;

/// <summary>
/// The static CPU inventory view model: brand/vendor/family, topology, cache sizes, virtualization and the ISA flags.
/// </summary>
public sealed class CpuSpecsViewModel : BindableBase, ICpuSpecsViewModel {
  /// <summary>
  /// The CPU vendor name.
  /// </summary>
  private string? _vendor;

  /// <summary>
  /// The CPU brand name.
  /// </summary>
  private string? _brand;

  /// <summary>
  /// The CPU socket number, e.g. 0 for a single-socket system, 1 for the second socket in a dual-socket system, etc.
  /// </summary>
  private int? _socket;

  /// <summary>
  /// The number of physical cores this CPU reports. Zero when not exposed.
  /// </summary>
  private int? _physicalCores;

  /// <summary>
  /// The number of logical cores this CPU reports. Zero when not exposed.
  /// </summary>
  private int? _logicalCores;

  /// <summary>
  /// The CPU family number, e.g. 6 for Intel Core and AMD Ryzen. Zero when not exposed.
  /// </summary>
  private int? _family;

  /// <summary>
  /// The CPU model number, e.g. 158 for Intel Coffee Lake. Zero when not exposed.
  /// </summary>
  private int? _model;

  /// <summary>
  /// The CPU stepping number, e.g. 10 for Intel Coffee Lake. Zero when not exposed.
  /// </summary>
  private int? _stepping;

  /// <summary>
  /// The CPU base clock in MHz, e.g. 3600 for a 3.6 GHz CPU. Zero when not exposed.
  /// </summary>
  private float? _baseSpeedMHz;

  /// <summary>
  /// The CPU bus clock in MHz, e.g. 100 for a 3.6 GHz CPU with a 36x multiplier. Zero when not exposed.
  /// </summary>
  private float? _busSpeedMHz;

  /// <summary>
  /// The CPU virtualization support status. True if virtualization is supported and enabled, false if supported but disabled, 
  /// null if not supported or not exposed.
  /// </summary>
  private bool? _virtualization;

  /// <summary>
  /// The CPU cache sizes in KB. Zero when not exposed.
  /// </summary>
  private double _l1CacheKb;

  /// <summary>
  /// The CPU cache sizes in KB. Zero when not exposed.
  /// </summary>
  private double _l2CacheKb;

  /// <summary>
  /// The CPU cache sizes in KB. Zero when not exposed.
  /// </summary>
  private double _l3CacheKb;

  /// <summary>
  /// The CPU cache line size in bytes. Zero when not exposed.
  /// </summary>
  private int _lineSizeBytes;

  /// <summary>
  /// The CPU vendor name.
  /// </summary>
  public string? Vendor { get => _vendor; private set => SetProperty(ref _vendor, value); }

  /// <summary>
  /// The CPU brand name.
  /// </summary>
  public string? Brand { get => _brand; private set => SetProperty(ref _brand, value); }

  /// <summary>
  /// The CPU socket number, e.g. 0 for a single-socket system, 1 for the second socket in a dual-socket system, etc.
  /// </summary>
  public int? Socket { get => _socket; private set => SetProperty(ref _socket, value); }

  /// <summary>
  /// The number of physical cores this CPU reports. Zero when not exposed.
  /// </summary>
  public int? PhysicalCores { get => _physicalCores; private set => SetProperty(ref _physicalCores, value); }

  /// <summary>
  /// The number of logical cores this CPU reports. Zero when not exposed.
  /// </summary>
  public int? LogicalCores { get => _logicalCores; private set => SetProperty(ref _logicalCores, value); }

  /// <summary>
  /// The CPU family number, e.g. 6 for Intel Core and AMD Ryzen. Zero when not exposed.
  /// </summary>
  public int? Family { get => _family; private set => SetProperty(ref _family, value); }

  /// <summary>
  /// The CPU model number, e.g. 158 for Intel Coffee Lake. Zero when not exposed.
  /// </summary>
  public int? Model { get => _model; private set => SetProperty(ref _model, value); }

  /// <summary>
  /// The CPU stepping number, e.g. 10 for Intel Coffee Lake. Zero when not exposed.
  /// </summary>
  public int? Stepping { get => _stepping; private set => SetProperty(ref _stepping, value); }

  /// <summary>
  /// The CPU base clock in MHz, e.g. 3600 for a 3.6 GHz CPU. Zero when not exposed.
  /// </summary>
  public float? BaseSpeedMHz { get => _baseSpeedMHz; private set => SetProperty(ref _baseSpeedMHz, value); }

  /// <summary>
  /// The CPU bus clock in MHz, e.g. 100 for a 3.6 GHz CPU with a 36x multiplier. Zero when not exposed.
  /// </summary>
  public float? BusSpeedMHz { get => _busSpeedMHz; private set => SetProperty(ref _busSpeedMHz, value); }

  /// <summary>
  /// The CPU virtualization support status. True if virtualization is supported and enabled, false if supported but disabled,
  /// </summary>
  public bool? Virtualization { get => _virtualization; private set => SetProperty(ref _virtualization, value); }

  /// <summary>
  /// The CPU cache sizes in KB. Zero when not exposed.
  /// </summary>
  public double L1CacheKb { get => _l1CacheKb; private set => SetProperty(ref _l1CacheKb, value); }

  /// <summary>
  /// The CPU cache sizes in KB. Zero when not exposed.
  /// </summary>
  public double L2CacheKb { get => _l2CacheKb; private set => SetProperty(ref _l2CacheKb, value); }

  /// <summary>
  /// The CPU cache sizes in KB. Zero when not exposed.
  /// </summary>
  public double L3CacheKb { get => _l3CacheKb; private set => SetProperty(ref _l3CacheKb, value); }

  /// <summary>
  /// The CPU cache line size in bytes. Zero when not exposed.
  /// </summary>
  public int LineSizeBytes { get => _lineSizeBytes; private set => SetProperty(ref _lineSizeBytes, value); }

  /// <summary>
  /// The CPU instruction set flags, e.g. SSE, AVX, etc. Each flag is a name and a boolean indicating whether the CPU supports it.
  /// </summary>
  public ObservableCollection<InstructionFlag> InstructionSet { get; } = [];

  /// <summary>
  /// Updates the view model with the given CPU info. 
  /// This method extracts the first socket's specs and populates the properties accordingly.
  /// </summary>
  /// <param name="info"></param>
  public void Update(ISystemCpuInfo info) {
    var socket = info.Sockets.FirstOrDefault();
    if (socket is null) return;

    var s = socket.Specs;
    Vendor = s.VendorName;
    Brand = s.BrandName;
    Socket = socket.SocketIndex + 1;
    PhysicalCores = s.PhysicalCoreNum;
    LogicalCores = s.LogicalCoreNum;
    Family = s.FamilyId;
    Model = s.ModelId;
    Stepping = s.SteppingId;
    BaseSpeedMHz = s.BaseSpeed;
    BusSpeedMHz = s.BusSpeed;
    Virtualization = s.VirtualizationEnabled ?? s.VirtualizationSupported;

    if (s.CacheInfo is { } cache) {
      // CpuCacheInfo stores totals in bytes; convert to KB for display. The SMBIOS
      // "Line Size" field is per-cache, so surface the L1 line size as the representative value.
      L1CacheKb = cache.L1_cache_size / 1024.0;
      L2CacheKb = cache.L2_cache_size / 1024.0;
      L3CacheKb = cache.L3_cache_size / 1024.0;
      LineSizeBytes = cache.L1_cache_line_size;
    }

    PopulateInstructionSet(s.InstructionSet);
  }

  /// <summary>
  /// Populates the InstructionSet collection with the given CPU instruction set info.
  /// </summary>
  /// <param name="isa"></param>
  private void PopulateInstructionSet(CpuInstructionInfo? isa) {
    InstructionSet.Clear();
    if (isa is not { } i) return;

    // Fixed display order mirrors the reference screenshot's column-major grid.
    foreach (var (name, available) in Enumerate(i))
      InstructionSet.Add(new InstructionFlag(name, available));
  }

  /// <summary>
  /// Enumerates the CPU instruction set flags in a fixed order for display purposes.
  /// </summary>
  /// <param name="i">CpuInstructionInfo</param>
  /// <returns>IEnumerable<(string Name, bool Available)></returns>
  private static IEnumerable<(string Name, bool Available)> Enumerate(CpuInstructionInfo i) {
    yield return ("3DNOW", i._3DNOW);
    yield return ("3DNOWEXT", i._3DNOWEXT);
    yield return ("ABM", i.ABM);
    yield return ("ADX", i.ADX);
    yield return ("AES", i.AES);
    yield return ("AVX", i.AVX);
    yield return ("AVX2", i.AVX2);
    yield return ("AVX512CD", i.AVX512CD);
    yield return ("AVX512ER", i.AVX512ER);
    yield return ("AVX512F", i.AVX512F);
    yield return ("AVX512PF", i.AVX512PF);
    yield return ("BMI1", i.BMI1);
    yield return ("BMI2", i.BMI2);
    yield return ("CLFSH", i.CLFSH);
    yield return ("CMPXCHG16B", i.CMPXCHG16B);
    yield return ("CX8", i.CX8);
    yield return ("ERMS", i.ERMS);
    yield return ("F16C", i.F16C);
    yield return ("FMA", i.FMA);
    yield return ("FSGSBASE", i.FSGSBASE);
    yield return ("FXSR", i.FXSR);
    yield return ("HLE", i.HLE);
    yield return ("INVPCID", i.INVPCID);
    yield return ("LAHF", i.LAHF);
    yield return ("LZCNT", i.LZCNT);
    yield return ("MMX", i.MMX);
    yield return ("MMXEXT", i.MMXEXT);
    yield return ("MONITOR", i.MONITOR);
    yield return ("MOVBE", i.MOVBE);
    yield return ("MSR", i.MSR);
    yield return ("OSXSAVE", i.OSXSAVE);
    yield return ("PCLMULQDQ", i.PCLMULQDQ);
    yield return ("POPCNT", i.POPCNT);
    yield return ("PREFETCHWT1", i.PREFETCHWT1);
    yield return ("RDRAND", i.RDRAND);
    yield return ("RDSEED", i.RDSEED);
    yield return ("RDTSCP", i.RDTSCP);
    yield return ("RTM", i.RTM);
    yield return ("SEP", i.SEP);
    yield return ("SHA", i.SHA);
    yield return ("SSE", i.SSE);
    yield return ("SSE2", i.SSE2);
    yield return ("SSE3", i.SSE3);
    yield return ("SSE41", i.SSE41);
    yield return ("SSE42", i.SSE42);
    yield return ("SSE4a", i.SSE4a);
    yield return ("SSSE3", i.SSSE3);
    yield return ("SYSCALL", i.SYSCALL);
    yield return ("TBM", i.TBM);
    yield return ("XOP", i.XOP);
    yield return ("XSAVE", i.XSAVE);
  }
}
