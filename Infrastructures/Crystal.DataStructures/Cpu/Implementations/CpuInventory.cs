using Crystal.DataStructures.Cpu.Definitions;
using Crystal.DataStructures.Cpu.Interfaces.Cpus;

namespace Crystal.DataStructures.Cpu.Implementations;

public class CpuInventory : BindableBase, ICpuSpecs {
  private string? _brandName;
  public string? BrandName {
    get => _brandName;
    set => SetProperty(ref _brandName, value);
  }

  private string? _vendorName;
  public string? VendorName {
    get => _vendorName;
    set => SetProperty(ref _vendorName, value);
  }

  private int? _familyId;
  public int? FamilyId {
    get => _familyId;
    set => SetProperty(ref _familyId, value);
  }

  private int? _modellId;
  public int? ModelId {
    get => _modellId;
    set => SetProperty(ref _modellId, value);
  }

  private int? _steppingId;
  public int? SteppingId {
    get => _steppingId;
    set => SetProperty(ref _steppingId, value);
  }

  private float? _baseSpeed;
  public float? BaseSpeed {
    get => _baseSpeed;
    set => SetProperty(ref _baseSpeed, value);
  }

  private float? _busSpeed;
  public float? BusSpeed {
    get => _busSpeed;
    set => SetProperty(ref _busSpeed, value);
  }

  private int? _socketNum;
  public int? SocketNum {
    get => _socketNum;
    set => SetProperty(ref _socketNum, value);
  }

  private int? _physicalCoreNum;
  public int? PhysicalCoreNum {
    get => _physicalCoreNum;
    set => SetProperty(ref _physicalCoreNum, value);
  }

  private int? _logicalCoreNum;
  public int? LogicalCoreNum {
    get => _logicalCoreNum;
    set => SetProperty(ref _logicalCoreNum, value);
  }

  private bool? _virtualization;
  public bool? Virtualization {
    get => _virtualization;
    set => SetProperty(ref _virtualization, value);
  }

  private CpuCacheInfo? _cpuCacheInfo;
  public CpuCacheInfo? CacheInfo {
    get => _cpuCacheInfo;
    set => SetProperty(ref _cpuCacheInfo, value);
  }

  private CpuInstructionInfo? _cpuInstructionInfo;
  public CpuInstructionInfo? InstructionSet {
    get => _cpuInstructionInfo;
    set => SetProperty(ref _cpuInstructionInfo, value);
  }
}
