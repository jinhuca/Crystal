using System.Runtime.InteropServices;

namespace Crystal.Service.Memory;

/// <summary>
/// Reads the kernel-memory figures Task Manager's Memory page shows but the telemetry provider does
/// not expose: committed / commit limit, system cache, paged and non-paged pool, and the amount of
/// installed RAM the OS cannot use ("hardware reserved"). Values come from <c>GetPerformanceInfo</c>
/// (psapi) reported in pages, plus <c>GetPhysicallyInstalledSystemMemory</c> (kernel32) for the
/// installed total. All results are returned in GB; a failed call yields null so the UI shows "—".
/// </summary>
internal static class KernelMemoryInfo {
  public readonly record struct Reading(
      double? CommittedGB,
      double? CommitLimitGB,
      double? CachedGB,
      double? PagedPoolGB,
      double? NonPagedPoolGB,
      double? HardwareReservedGB);

  public static Reading Read() {
    var info = new PERFORMANCE_INFORMATION { cb = (uint)Marshal.SizeOf<PERFORMANCE_INFORMATION>() };
    if (!GetPerformanceInfo(ref info, info.cb))
      return new Reading(null, null, null, null, null, null);

    double pageSize = info.PageSize;
    double ToGB(nuint pages) => pages * pageSize / (1024.0 * 1024.0 * 1024.0);

    // Hardware reserved = physically installed RAM minus what the OS reports as usable. The former
    // comes from SMBIOS (installed sticks), the latter from GetPerformanceInfo (PhysicalTotal, the
    // memory the OS actually manages). GetPhysicallyInstalledSystemMemory returns kilobytes.
    double? hardwareReservedGB = null;
    if (GetPhysicallyInstalledSystemMemory(out ulong installedKb)) {
      double installedGB = installedKb / (1024.0 * 1024.0);
      double osUsableGB = ToGB(info.PhysicalTotal);
      double reserved = installedGB - osUsableGB;
      hardwareReservedGB = reserved > 0 ? reserved : 0;
    }

    return new Reading(
        CommittedGB: ToGB(info.CommitTotal),
        CommitLimitGB: ToGB(info.CommitLimit),
        // SystemCache approximates Task Manager's "Cached" (standby + modified) closely enough for a
        // readout; the exact figure needs standby-list counters not available from this API.
        CachedGB: ToGB(info.SystemCache),
        PagedPoolGB: ToGB(info.KernelPaged),
        NonPagedPoolGB: ToGB(info.KernelNonpaged),
        HardwareReservedGB: hardwareReservedGB);
  }

  [StructLayout(LayoutKind.Sequential)]
  private struct PERFORMANCE_INFORMATION {
    public uint cb;
    public nuint CommitTotal;
    public nuint CommitLimit;
    public nuint CommitPeak;
    public nuint PhysicalTotal;
    public nuint PhysicalAvailable;
    public nuint SystemCache;
    public nuint KernelTotal;
    public nuint KernelPaged;
    public nuint KernelNonpaged;
    public nuint PageSize;
    public uint HandleCount;
    public uint ProcessCount;
    public uint ThreadCount;
  }

  [DllImport("psapi.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool GetPerformanceInfo(ref PERFORMANCE_INFORMATION pPerformanceInformation, uint cb);

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool GetPhysicallyInstalledSystemMemory(out ulong totalMemoryInKilobytes);
}
