using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crystal.Infrastructure.DataStructures.Cpu.Definitions;
using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;

namespace Crystal.Provider.Smbios.HardwareFeatures.Processor;

/// <summary>
/// Reads the system SMBIOS table and projects each populated Type 4 processor
/// structure into a <see cref="SmbiosProcessorInfo"/>, resolving cache sizes
/// from the referenced Type 7 structures.
/// </summary>
public sealed class SmbiosProcessorProvider : ISmbiosProcessorProvider {
  public Task<IReadOnlyList<SmbiosProcessorInfo>> GetAllProcessorsAsync(CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();

    var table = SmbiosTable.Load();
    var cacheByHandle = table.CacheInformation.ToDictionary(c => c.Handle);

    IReadOnlyList<SmbiosProcessorInfo> result = table.ProcessorInformation
        .Where(p => p.IsPopulated)
        .Select(p => Project(p, cacheByHandle))
        .ToList();

    return Task.FromResult(result);
  }

  private static SmbiosProcessorInfo Project(
      T004_ProcessorInformation p,
      IReadOnlyDictionary<ushort, T007_CacheInformation> caches) {
    var cache = BuildCacheInfo(p, caches);
    return new SmbiosProcessorInfo(
        SocketDesignation: p.SocketDesignation,
        MaxSpeedMHz: p.MaxSpeedMhz > 0 ? p.MaxSpeedMhz : null,
        ExternalClockMHz: p.ExternalClockMhz > 0 ? p.ExternalClockMhz : null,
        LogicalCoreCount: p.LogicalCoreCount > 0 ? p.LogicalCoreCount : null,
        CacheInfo: cache);
  }

  private static CpuCacheInfo? BuildCacheInfo(
      T004_ProcessorInformation p,
      IReadOnlyDictionary<ushort, T007_CacheInformation> caches) {
    // SMBIOS reports cache size in KiB; CpuCacheInfo stores bytes.
    int l1 = SizeBytes(p.L1CacheHandle, caches);
    int l2 = SizeBytes(p.L2CacheHandle, caches);
    int l3 = SizeBytes(p.L3CacheHandle, caches);
    if (l1 == 0 && l2 == 0 && l3 == 0) return null;

    return new CpuCacheInfo {
      L1_cache_size = l1,
      L2_cache_size = l2,
      L3_cache_size = l3,
    };
  }

  private static int SizeBytes(ushort handle, IReadOnlyDictionary<ushort, T007_CacheInformation> caches) {
    if (handle == 0xFFFF) return 0;
    if (!caches.TryGetValue(handle, out var c)) return 0;
    long kib = c.InstalledSizeKiB > 0 ? c.InstalledSizeKiB : c.MaxSizeKiB;
    return (int)(kib * 1024);
  }
}
