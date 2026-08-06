using Crystal.Infrastructure.DataStructures.Cpu.Definitions;
using Crystal.Provider.CpuId;
using Crystal.Provider.Mmi.HardwareFeatures.Processor;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;
using Crystal.Service.Cpu;
using Xunit;

namespace Crystal.Service.Cpu.Tests;

public class CpuSpecsResolverTests {
  private static readonly CpuSpecsResolver Resolver = new();

  // CpuIdRawData has 13 positional members; this builds one with neutral defaults so each test
  // only sets the fields it exercises.
  private static CpuIdRawData Cpuid(
    string? brand = "Test CPU",
    string? vendor = "TestVendor",
    uint family = 6,
    uint model = 1,
    uint stepping = 2,
    uint baseSpeed = 0,
    uint busSpeed = 0,
    uint physical = 0,
    uint logical = 0,
    bool virtSupported = false,
    bool virtEnabled = false,
    CpuCacheInfo? cache = null,
    CpuInstructionInfo? instructions = null) =>
    new(brand, vendor, family, model, stepping, baseSpeed, busSpeed, physical, logical,
        virtSupported, virtEnabled, cache, instructions);

  [Fact]
  public void Resolve_CopiesIdentityFieldsFromCpuid() {
    var specs = Resolver.Resolve(Cpuid(brand: "Ryzen 9", vendor: "AuthenticAMD", family: 25, model: 33, stepping: 2), null, null);

    Assert.Equal("Ryzen 9", specs.BrandName);
    Assert.Equal("AuthenticAMD", specs.VendorName);
    Assert.Equal(25, specs.FamilyId);
    Assert.Equal(33, specs.ModelId);
    Assert.Equal(2, specs.SteppingId);
  }

  // ── Base / bus speed: CPUID leaf 0x16 wins when present, SMBIOS is the fallback ──────────

  [Fact]
  public void Resolve_BaseSpeed_PrefersCpuidWhenNonZero() {
    var smbios = new SmbiosProcessorInfo("CPU0", MaxSpeedMHz: 4200, ExternalClockMHz: 100, LogicalCoreCount: 8, CacheInfo: null);
    var specs = Resolver.Resolve(Cpuid(baseSpeed: 3600), smbios, null);
    Assert.Equal(3600, specs.BaseSpeed);
  }

  [Fact]
  public void Resolve_BaseSpeed_FallsBackToSmbiosWhenCpuidZero() {
    var smbios = new SmbiosProcessorInfo("CPU0", MaxSpeedMHz: 4200, ExternalClockMHz: 100, LogicalCoreCount: 8, CacheInfo: null);
    var specs = Resolver.Resolve(Cpuid(baseSpeed: 0), smbios, null);
    Assert.Equal(4200, specs.BaseSpeed);
  }

  [Fact]
  public void Resolve_BusSpeed_FallsBackToSmbiosExternalClock() {
    var smbios = new SmbiosProcessorInfo("CPU0", MaxSpeedMHz: 4200, ExternalClockMHz: 100, LogicalCoreCount: 8, CacheInfo: null);
    var specs = Resolver.Resolve(Cpuid(busSpeed: 0), smbios, null);
    Assert.Equal(100, specs.BusSpeed);
  }

  [Fact]
  public void Resolve_Speeds_NullWhenNeitherSourceHasValue() {
    var specs = Resolver.Resolve(Cpuid(baseSpeed: 0, busSpeed: 0), null, null);
    Assert.Null(specs.BaseSpeed);
    Assert.Null(specs.BusSpeed);
  }

  // ── Logical core count: WMI (OS) is authoritative over CPUID ─────────────────────────────

  [Fact]
  public void Resolve_LogicalCores_PrefersWmiOverCpuid() {
    var wmi = new WmiProcessorMetrics("CPU0", NumberOfLogicalProcessors: 32, NumberOfCores: 16, VirtualizationFirmwareEnabled: null);
    var specs = Resolver.Resolve(Cpuid(logical: 24), null, wmi);
    Assert.Equal(32, specs.LogicalCoreNum);
  }

  [Fact]
  public void Resolve_LogicalCores_FallsBackToCpuidWhenWmiMissing() {
    var specs = Resolver.Resolve(Cpuid(logical: 24), null, null);
    Assert.Equal(24, specs.LogicalCoreNum);
  }

  [Fact]
  public void Resolve_LogicalCores_NullWhenNoSourceReports() {
    var wmi = new WmiProcessorMetrics("CPU0", NumberOfLogicalProcessors: 0, NumberOfCores: null, VirtualizationFirmwareEnabled: null);
    var specs = Resolver.Resolve(Cpuid(logical: 0), null, wmi);
    Assert.Null(specs.LogicalCoreNum);
  }

  // ── Physical core count: WMI > CPUID > SMBIOS ────────────────────────────────────────────

  [Fact]
  public void Resolve_PhysicalCores_PrefersWmi() {
    var smbios = new SmbiosProcessorInfo("CPU0", null, null, LogicalCoreCount: 4, CacheInfo: null);
    var wmi = new WmiProcessorMetrics("CPU0", NumberOfLogicalProcessors: 32, NumberOfCores: 16, VirtualizationFirmwareEnabled: null);
    var specs = Resolver.Resolve(Cpuid(physical: 8), smbios, wmi);
    Assert.Equal(16, specs.PhysicalCoreNum);
  }

  [Fact]
  public void Resolve_PhysicalCores_UsesCpuidWhenWmiMissing() {
    var smbios = new SmbiosProcessorInfo("CPU0", null, null, LogicalCoreCount: 4, CacheInfo: null);
    var specs = Resolver.Resolve(Cpuid(physical: 8), smbios, null);
    Assert.Equal(8, specs.PhysicalCoreNum);
  }

  [Fact]
  public void Resolve_PhysicalCores_FallsBackToSmbiosLast() {
    var smbios = new SmbiosProcessorInfo("CPU0", null, null, LogicalCoreCount: 4, CacheInfo: null);
    var specs = Resolver.Resolve(Cpuid(physical: 0), smbios, null);
    Assert.Equal(4, specs.PhysicalCoreNum);
  }

  [Fact]
  public void Resolve_PhysicalCores_NullWhenAllSourcesEmpty() {
    var specs = Resolver.Resolve(Cpuid(physical: 0), null, null);
    Assert.Null(specs.PhysicalCoreNum);
  }

  // ── Virtualization: supported and enabled are distinct facts ─────────────────────────────

  [Fact]
  public void Resolve_VirtualizationSupported_ComesFromCpuid() {
    var specs = Resolver.Resolve(Cpuid(virtSupported: true), null, null);
    Assert.True(specs.VirtualizationSupported);
  }

  [Fact]
  public void Resolve_VirtualizationEnabled_PrefersWmiFlag() {
    var wmi = new WmiProcessorMetrics("CPU0", null, null, VirtualizationFirmwareEnabled: true);
    var specs = Resolver.Resolve(Cpuid(virtEnabled: false), null, wmi);
    Assert.True(specs.VirtualizationEnabled);
  }

  [Fact]
  public void Resolve_VirtualizationEnabled_FallsBackToCpuidWhenWmiNull() {
    var wmi = new WmiProcessorMetrics("CPU0", null, null, VirtualizationFirmwareEnabled: null);
    var specs = Resolver.Resolve(Cpuid(virtEnabled: true), null, wmi);
    Assert.True(specs.VirtualizationEnabled);
  }

  // ── Cache: CPUID wins, SMBIOS is the fallback ────────────────────────────────────────────

  [Fact]
  public void Resolve_CacheInfo_PrefersCpuid() {
    var cpuidCache = new CpuCacheInfo { L1_cache_size = 32, L2_cache_size = 512, L3_cache_size = 8192 };
    var smbiosCache = new CpuCacheInfo { L1_cache_size = 1, L2_cache_size = 2, L3_cache_size = 3 };
    var smbios = new SmbiosProcessorInfo("CPU0", null, null, null, CacheInfo: smbiosCache);

    var specs = Resolver.Resolve(Cpuid(cache: cpuidCache), smbios, null);

    Assert.NotNull(specs.CacheInfo);
    Assert.Equal(32, specs.CacheInfo!.Value.L1_cache_size);
    Assert.Equal(8192, specs.CacheInfo!.Value.L3_cache_size);
  }

  [Fact]
  public void Resolve_CacheInfo_FallsBackToSmbiosWhenCpuidNull() {
    var smbiosCache = new CpuCacheInfo { L1_cache_size = 64 };
    var smbios = new SmbiosProcessorInfo("CPU0", null, null, null, CacheInfo: smbiosCache);

    var specs = Resolver.Resolve(Cpuid(cache: null), smbios, null);

    Assert.NotNull(specs.CacheInfo);
    Assert.Equal(64, specs.CacheInfo!.Value.L1_cache_size);
  }

  [Fact]
  public void Resolve_InstructionSet_TakenFromCpuid() {
    var instructions = new CpuInstructionInfo { AVX = true, AVX2 = true, SSE42 = true };
    var specs = Resolver.Resolve(Cpuid(instructions: instructions), null, null);

    Assert.NotNull(specs.InstructionSet);
    Assert.True(specs.InstructionSet!.Value.AVX2);
  }
}
