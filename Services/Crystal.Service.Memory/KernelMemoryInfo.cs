using System.Runtime.InteropServices;

namespace Crystal.Service.Memory;

/// <summary>
/// Reads the kernel-memory figures Task Manager's Memory page shows but the telemetry provider does
/// not expose: committed / commit limit, system cache, paged and non-paged pool, and the amount of
/// installed RAM the OS cannot use ("hardware reserved"). Values come from <c>GetPerformanceInfo</c>
/// (psapi) reported in pages, plus <c>GetPhysicallyInstalledSystemMemory</c> (kernel32) for the
/// installed total, and <c>NtQuerySystemInformation</c> (ntdll) for the pagefile size/usage.
/// All results are returned in GB; a failed call yields null so the UI shows "—".
/// </summary>
internal static class KernelMemoryInfo {
  /// <summary>
  /// The kernel-memory figures, all in GB and each nullable so an unavailable source maps to
  /// null (rendered as "—"). Mirrors the corresponding fields on <see cref="MemoryLoadReading"/>.
  /// </summary>
  /// <param name="CommittedGB">Current commit charge (RAM + pagefile backed).</param>
  /// <param name="CommitLimitGB">Maximum commit charge the system can accept.</param>
  /// <param name="CommitPeakGB">Highest commit charge since boot.</param>
  /// <param name="CachedGB">System cache size (approximates Task Manager's "Cached").</param>
  /// <param name="PagedPoolGB">Kernel paged pool size.</param>
  /// <param name="NonPagedPoolGB">Kernel non-paged pool size.</param>
  /// <param name="HardwareReservedGB">Installed RAM the OS cannot use (installed minus OS-usable).</param>
  /// <param name="PhysicalTotalGB">OS-usable physical memory.</param>
  /// <param name="PageFileUsedGB">Pagefile bytes in use, summed across all pagefiles.</param>
  /// <param name="PageFileTotalGB">Total configured pagefile size, summed across all pagefiles.</param>
  /// <param name="PageFilePeakGB">Highest pagefile occupancy since boot.</param>
  public readonly record struct Reading(
      double? CommittedGB,
      double? CommitLimitGB,
      double? CommitPeakGB,
      double? CachedGB,
      double? PagedPoolGB,
      double? NonPagedPoolGB,
      double? HardwareReservedGB,
      double? PhysicalTotalGB,
      double? PageFileUsedGB,
      double? PageFileTotalGB,
      double? PageFilePeakGB);

  /// <summary>
  /// Reads all kernel-memory figures in one shot. Calls <c>GetPerformanceInfo</c> for the page-based
  /// counters (returning an all-null <see cref="Reading"/> if that fails, since the page size it
  /// yields is needed to convert everything else), then layers on the pagefile totals and the
  /// hardware-reserved delta. Each supplementary source degrades to null independently.
  /// </summary>
  /// <returns>The assembled kernel-memory reading.</returns>
  public static Reading Read() {
    var info = new PERFORMANCE_INFORMATION { cb = (uint)Marshal.SizeOf<PERFORMANCE_INFORMATION>() };
    if (!GetPerformanceInfo(ref info, info.cb))
      return new Reading(null, null, null, null, null, null, null, null, null, null, null);

    double pageSize = info.PageSize;
    double ToGB(nuint pages) => pages * pageSize / (1024.0 * 1024.0 * 1024.0);

    var (pageFileUsedGB, pageFileTotalGB, pageFilePeakGB) = ReadPageFile(pageSize);

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
        // Highest commit charge since boot — the session's peak memory pressure.
        CommitPeakGB: ToGB(info.CommitPeak),
        // SystemCache approximates Task Manager's "Cached" (standby + modified) closely enough for a
        // readout; the exact figure needs standby-list counters not available from this API.
        CachedGB: ToGB(info.SystemCache),
        PagedPoolGB: ToGB(info.KernelPaged),
        NonPagedPoolGB: ToGB(info.KernelNonpaged),
        HardwareReservedGB: hardwareReservedGB,
        // OS-usable physical memory — the span the composition bar's four segments fill.
        PhysicalTotalGB: ToGB(info.PhysicalTotal),
        PageFileUsedGB: pageFileUsedGB,
        PageFileTotalGB: pageFileTotalGB,
        // Highest pagefile occupancy since boot — the peak backing-store pressure.
        PageFilePeakGB: pageFilePeakGB);
  }

  /// <summary>
  /// Pagefile size and current usage, summed across every configured pagefile (systems can have one
  /// per volume). GetPerformanceInfo only exposes the combined commit charge, not the pagefile
  /// backing it, so this comes from NtQuerySystemInformation's SystemPageFileInformation — a linked
  /// list of entries, each carrying total/in-use/peak in pages. Grows the buffer and retries on
  /// STATUS_INFO_LENGTH_MISMATCH. Returns (null, null, null) when the query fails or no pagefile is
  /// configured, so the UI falls back to "—".
  /// </summary>
  /// <param name="pageSize">The system page size in bytes, used to convert page counts to GB.</param>
  /// <returns>Used, total and peak pagefile sizes in GB, or nulls when unavailable.</returns>
  private static (double? UsedGB, double? TotalGB, double? PeakGB) ReadPageFile(double pageSize) {
    uint length = 4096;
    IntPtr buffer = Marshal.AllocHGlobal((int)length);
    try {
      int status;
      uint returnLength;
      while ((status = NtQuerySystemInformation(SystemPageFileInformation, buffer, length, out returnLength))
             == STATUS_INFO_LENGTH_MISMATCH) {
        Marshal.FreeHGlobal(buffer);
        length = returnLength > length ? returnLength : length * 2;
        buffer = Marshal.AllocHGlobal((int)length);
      }
      // A disabled pagefile succeeds but writes no entries; reading the uninitialized buffer then
      // would be garbage, so treat an empty result as "no pagefile" (unavailable).
      if (status != 0 || returnLength == 0) return (null, null, null);

      double totalPages = 0, usedPages = 0, peakPages = 0;
      bool any = false;
      IntPtr entryPtr = buffer;
      while (true) {
        var entry = Marshal.PtrToStructure<SYSTEM_PAGEFILE_INFORMATION>(entryPtr);
        any = true;
        totalPages += entry.TotalSize;
        usedPages += entry.TotalInUse;
        peakPages += entry.PeakUsage;
        if (entry.NextEntryOffset == 0) break;
        entryPtr = IntPtr.Add(entryPtr, (int)entry.NextEntryOffset);
      }
      if (!any) return (null, null, null);

      double ToGB(double pages) => pages * pageSize / (1024.0 * 1024.0 * 1024.0);
      return (ToGB(usedPages), ToGB(totalPages), ToGB(peakPages));
    } finally {
      Marshal.FreeHGlobal(buffer);
    }
  }

  /// <summary>
  /// Native <c>PERFORMANCE_INFORMATION</c> struct filled by <c>GetPerformanceInfo</c>. The
  /// pointer-sized (nuint) counters are page counts, not bytes; <c>cb</c> must be set to the struct
  /// size before the call. Only a subset of fields is consumed here.
  /// </summary>
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

  /// <summary>
  /// psapi <c>GetPerformanceInfo</c>: fills a <see cref="PERFORMANCE_INFORMATION"/> with
  /// system-wide memory/handle counts. Returns false on failure.
  /// </summary>
  [DllImport("psapi.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool GetPerformanceInfo(ref PERFORMANCE_INFORMATION pPerformanceInformation, uint cb);

  /// <summary>
  /// kernel32 <c>GetPhysicallyInstalledSystemMemory</c>: reports SMBIOS-installed RAM in
  /// kilobytes (the physical sticks, including any the OS cannot use). Returns false on failure.
  /// </summary>
  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool GetPhysicallyInstalledSystemMemory(out ulong totalMemoryInKilobytes);

  /// <summary>
  /// The <c>SystemPageFileInformation</c> class value for <see cref="NtQuerySystemInformation"/>.
  /// </summary>
  private const int SystemPageFileInformation = 18;

  /// <summary>
  /// NTSTATUS returned when the supplied buffer is too small; signals a grow-and-retry.
  /// </summary>
  private const int STATUS_INFO_LENGTH_MISMATCH = unchecked((int)0xC0000004);

  /// <summary>
  /// One node of the SystemPageFileInformation linked list. Sizes are in pages; PageFileName
  /// is present in the native struct but unused here.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  private struct SYSTEM_PAGEFILE_INFORMATION {
    public uint NextEntryOffset;
    public uint TotalSize;
    public uint TotalInUse;
    public uint PeakUsage;
    public UNICODE_STRING PageFileName;
  }

  /// <summary>
  /// Native <c>UNICODE_STRING</c> (length-prefixed string) embedded in
  /// <see cref="SYSTEM_PAGEFILE_INFORMATION"/>; declared for correct struct layout, not read.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  private struct UNICODE_STRING {
    public ushort Length;
    public ushort MaximumLength;
    public IntPtr Buffer;
  }

  /// <summary>
  /// ntdll <c>NtQuerySystemInformation</c>: general system-information query, used here for
  /// the pagefile list. Returns an NTSTATUS (0 = success); see
  /// <see cref="STATUS_INFO_LENGTH_MISMATCH"/> for the buffer-too-small case.
  /// </summary>
  [DllImport("ntdll.dll")]
  private static extern int NtQuerySystemInformation(
      int systemInformationClass, IntPtr systemInformation, uint systemInformationLength, out uint returnLength);
}
