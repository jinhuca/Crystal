using System.Runtime.Intrinsics.X86;
using System.Text;
using Crystal.Infrastructure.DataStructures.Cpu.Definitions;

namespace Crystal.Provider.CpuId;

/// <summary>
/// Managed CPUID provider built on <see cref="X86Base.CpuId"/>. Decodes vendor,
/// brand, family/model/stepping, clocks (leaf 0x16), virtualization support and
/// the instruction-set feature flags. Cache topology is left to SMBIOS — CPUID's
/// deterministic-cache leaf reports per-logical-core sizes that don't map cleanly
/// onto the L1/L2/L3 total the UI wants.
/// </summary>
public sealed class CpuIdProvider : ICpuIdProvider {
  public CpuIdRawData Query() {
    if (!X86Base.IsSupported) {
      return new CpuIdRawData(
          Brand: null, Vendor: null,
          FamilyId: 0, ModelId: 0, SteppingId: 0,
          BaseSpeedMHz: 0, BusSpeedMHz: 0,
          PhysicalCoreCount: 0, LogicalCoreCount: (uint)Environment.ProcessorCount,
          VirtualizationSupported: false, VirtualizationFirmwareEnabled: false,
          CacheInfo: null, InstructionSet: null);
    }

    var (maxLeaf, vb, vc, vd) = X86Base.CpuId(0, 0);
    string vendor = DecodeVendor(vb, vd, vc);

    var (fms, _, feat1Ecx, feat1Edx) = X86Base.CpuId(1, 0);
    var (family, model, stepping) = DecodeFms(fms);

    var (maxExtLeaf, _, _, _) = X86Base.CpuId(unchecked((int)0x80000000u), 0);
    string? brand = ((uint)maxExtLeaf >= 0x80000004u) ? DecodeBrand() : null;

    uint baseMHz = 0, busMHz = 0;
    if ((uint)maxLeaf >= 0x16u) {
      var (eax16, ebx16, _, _) = X86Base.CpuId(0x16, 0);
      baseMHz = (uint)eax16;   // Processor Base Frequency (MHz)
      busMHz = (uint)ebx16;    // Bus (Reference) Frequency (MHz)
    }

    uint feat7Ebx = 0, feat7Ecx = 0;
    if ((uint)maxLeaf >= 0x07u) {
      var (_, ebx7, ecx7, _) = X86Base.CpuId(0x07, 0);
      feat7Ebx = (uint)ebx7;
      feat7Ecx = (uint)ecx7;
    }

    uint extEcx = 0, extEdx = 0;
    if ((uint)maxExtLeaf >= 0x80000001u) {
      var (_, _, ecxE, edxE) = X86Base.CpuId(unchecked((int)0x80000001u), 0);
      extEcx = (uint)ecxE;
      extEdx = (uint)edxE;
    }

    bool isAmd = vendor.Contains("AMD", StringComparison.OrdinalIgnoreCase);
    // Intel exposes VMX (leaf 1 ECX bit 5); AMD exposes SVM (ext leaf ECX bit 2).
    bool virtualizationSupported = ((uint)feat1Ecx & (1u << 5)) != 0 || (extEcx & (1u << 2)) != 0;

    var instructionSet = DecodeInstructionSet((uint)feat1Ecx, (uint)feat1Edx, feat7Ebx, feat7Ecx, extEcx, extEdx);

    return new CpuIdRawData(
        Brand: brand,
        Vendor: vendor,
        FamilyId: family,
        ModelId: model,
        SteppingId: stepping,
        BaseSpeedMHz: baseMHz,
        BusSpeedMHz: busMHz,
        PhysicalCoreCount: 0,   // CPUID topology is per-package/thread-local; SMBIOS/WMI own the totals.
        LogicalCoreCount: (uint)Environment.ProcessorCount,
        VirtualizationSupported: virtualizationSupported,
        // CPUID can't observe the firmware toggle; only WMI can. Report false here and let WMI override.
        VirtualizationFirmwareEnabled: false,
        CacheInfo: null,
        InstructionSet: instructionSet);
  }

  private static (uint family, uint model, uint stepping) DecodeFms(int eax) {
    uint v = (uint)eax;
    uint stepping = v & 0xF;
    uint baseModel = (v >> 4) & 0xF;
    uint baseFamily = (v >> 8) & 0xF;
    uint extModel = (v >> 16) & 0xF;
    uint extFamily = (v >> 20) & 0xFF;

    uint family = baseFamily == 0xF ? baseFamily + extFamily : baseFamily;
    uint model = (baseFamily == 0x6 || baseFamily == 0xF) ? (extModel << 4) + baseModel : baseModel;
    return (family, model, stepping);
  }

  private static string DecodeVendor(int ebx, int edx, int ecx) {
    Span<byte> buf = stackalloc byte[12];
    BitConverter.TryWriteBytes(buf[..4], ebx);
    BitConverter.TryWriteBytes(buf.Slice(4, 4), edx);
    BitConverter.TryWriteBytes(buf.Slice(8, 4), ecx);
    return Encoding.ASCII.GetString(buf).Trim();
  }

  private static string DecodeBrand() {
    Span<byte> buf = stackalloc byte[48];
    for (int i = 0; i < 3; i++) {
      var (a, b, c, d) = X86Base.CpuId(unchecked((int)(0x80000002u + (uint)i)), 0);
      int off = i * 16;
      BitConverter.TryWriteBytes(buf.Slice(off, 4), a);
      BitConverter.TryWriteBytes(buf.Slice(off + 4, 4), b);
      BitConverter.TryWriteBytes(buf.Slice(off + 8, 4), c);
      BitConverter.TryWriteBytes(buf.Slice(off + 12, 4), d);
    }
    int len = buf.IndexOf((byte)0);
    if (len < 0) len = buf.Length;
    return Encoding.ASCII.GetString(buf[..len]).Trim();
  }

  private static CpuInstructionInfo DecodeInstructionSet(
      uint f1Ecx, uint f1Edx, uint f7Ebx, uint f7Ecx, uint eEcx, uint eEdx) {
    static bool Bit(uint value, int bit) => (value & (1u << bit)) != 0;
    return new CpuInstructionInfo {
      // Leaf 1 EDX
      CX8 = Bit(f1Edx, 8),
      MSR = Bit(f1Edx, 5),
      SEP = Bit(f1Edx, 11),
      CLFSH = Bit(f1Edx, 19),
      MMX = Bit(f1Edx, 23),
      FXSR = Bit(f1Edx, 24),
      SSE = Bit(f1Edx, 25),
      SSE2 = Bit(f1Edx, 26),
      // Leaf 1 ECX
      SSE3 = Bit(f1Ecx, 0),
      PCLMULQDQ = Bit(f1Ecx, 1),
      MONITOR = Bit(f1Ecx, 3),
      SSSE3 = Bit(f1Ecx, 9),
      FMA = Bit(f1Ecx, 12),
      CMPXCHG16B = Bit(f1Ecx, 13),
      SSE41 = Bit(f1Ecx, 19),
      SSE42 = Bit(f1Ecx, 20),
      MOVBE = Bit(f1Ecx, 22),
      POPCNT = Bit(f1Ecx, 23),
      AES = Bit(f1Ecx, 25),
      XSAVE = Bit(f1Ecx, 26),
      OSXSAVE = Bit(f1Ecx, 27),
      AVX = Bit(f1Ecx, 28),
      F16C = Bit(f1Ecx, 29),
      RDRAND = Bit(f1Ecx, 30),
      // Leaf 7 EBX
      FSGSBASE = Bit(f7Ebx, 0),
      BMI1 = Bit(f7Ebx, 3),
      HLE = Bit(f7Ebx, 4),
      AVX2 = Bit(f7Ebx, 5),
      BMI2 = Bit(f7Ebx, 8),
      ERMS = Bit(f7Ebx, 9),
      INVPCID = Bit(f7Ebx, 10),
      RTM = Bit(f7Ebx, 11),
      AVX512F = Bit(f7Ebx, 16),
      RDSEED = Bit(f7Ebx, 18),
      ADX = Bit(f7Ebx, 19),
      AVX512PF = Bit(f7Ebx, 26),
      AVX512ER = Bit(f7Ebx, 27),
      AVX512CD = Bit(f7Ebx, 28),
      SHA = Bit(f7Ebx, 29),
      // Leaf 7 ECX
      PREFETCHWT1 = Bit(f7Ecx, 0),
      // Extended leaf 0x80000001 ECX
      LAHF = Bit(eEcx, 0),
      ABM = Bit(eEcx, 5),
      LZCNT = Bit(eEcx, 5),
      SSE4a = Bit(eEcx, 6),
      XOP = Bit(eEcx, 11),
      TBM = Bit(eEcx, 21),
      // Extended leaf 0x80000001 EDX
      SYSCALL = Bit(eEdx, 11),
      MMXEXT = Bit(eEdx, 22),
      RDTSCP = Bit(eEdx, 27),
      _3DNOWEXT = Bit(eEdx, 30),
      _3DNOW = Bit(eEdx, 31),
    };
  }
}
