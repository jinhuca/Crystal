using System.Collections.ObjectModel;
using CpuModule.ViewModels.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Definitions;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;

namespace CpuModule.ViewModels.Implementations;

public sealed class CpuSpecsViewModel : BindableBase, ICpuSpecsViewModel {
  private string? _vendor;
  private string? _brand;
  private int? _socket;
  private int? _physicalCores;
  private int? _logicalCores;
  private int? _family;
  private int? _model;
  private int? _stepping;
  private float? _baseSpeedMHz;
  private float? _busSpeedMHz;
  private bool? _virtualization;
  private double _l1CacheKb;
  private double _l2CacheKb;
  private double _l3CacheKb;
  private int _lineSizeBytes;

  public string? Vendor { get => _vendor; private set => SetProperty(ref _vendor, value); }
  public string? Brand { get => _brand; private set => SetProperty(ref _brand, value); }
  public int? Socket { get => _socket; private set => SetProperty(ref _socket, value); }
  public int? PhysicalCores { get => _physicalCores; private set => SetProperty(ref _physicalCores, value); }
  public int? LogicalCores { get => _logicalCores; private set => SetProperty(ref _logicalCores, value); }
  public int? Family { get => _family; private set => SetProperty(ref _family, value); }
  public int? Model { get => _model; private set => SetProperty(ref _model, value); }
  public int? Stepping { get => _stepping; private set => SetProperty(ref _stepping, value); }
  public float? BaseSpeedMHz { get => _baseSpeedMHz; private set => SetProperty(ref _baseSpeedMHz, value); }
  public float? BusSpeedMHz { get => _busSpeedMHz; private set => SetProperty(ref _busSpeedMHz, value); }
  public bool? Virtualization { get => _virtualization; private set => SetProperty(ref _virtualization, value); }
  public double L1CacheKb { get => _l1CacheKb; private set => SetProperty(ref _l1CacheKb, value); }
  public double L2CacheKb { get => _l2CacheKb; private set => SetProperty(ref _l2CacheKb, value); }
  public double L3CacheKb { get => _l3CacheKb; private set => SetProperty(ref _l3CacheKb, value); }
  public int LineSizeBytes { get => _lineSizeBytes; private set => SetProperty(ref _lineSizeBytes, value); }

  public ObservableCollection<InstructionFlag> InstructionSet { get; } = [];

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
      // CPUID/SMBIOS report L1..L3 as totals in KB; the SMBIOS "Line Size" field is
      // per-cache, so surface the L1 line size as the representative value.
      L1CacheKb = cache.L1_cache_size;
      L2CacheKb = cache.L2_cache_size;
      L3CacheKb = cache.L3_cache_size;
      LineSizeBytes = cache.L1_cache_line_size;
    }

    PopulateInstructionSet(s.InstructionSet);
  }

  private void PopulateInstructionSet(CpuInstructionInfo? isa) {
    InstructionSet.Clear();
    if (isa is not { } i) return;

    // Fixed display order mirrors the reference screenshot's column-major grid.
    foreach (var (name, available) in Enumerate(i))
      InstructionSet.Add(new InstructionFlag(name, available));
  }

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
