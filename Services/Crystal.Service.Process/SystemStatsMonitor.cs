using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using SysProcess = System.Diagnostics.Process;

namespace Crystal.Service.Process;

/// <summary>
/// Polls the OS on a cadence and emits system-wide totals — process count, summed thread and handle
/// counts, and true machine-wide CPU and memory utilization. Process/thread/handle counts come from
/// <see cref="SysProcess.GetProcesses"/> (a cheap in-process enumeration; Crystal runs elevated so it
/// can read counts for processes in other sessions). CPU% comes from <c>GetSystemTimes</c> (the
/// whole-machine busy fraction between polls, matching Task Manager) rather than summing per-process
/// CPU, which double-counts the idle process and reads as ~99%. Memory comes from
/// <c>GlobalMemoryStatusEx</c>.
/// <para>
/// Cold and ref-counted like the other <c>*Monitor</c> types: the timer only ticks while
/// something is subscribed, and the default cadence is 1 second. CPU% needs the delta between two
/// samples, so the very first emission after subscribing reports 0% CPU.
/// </para>
/// </summary>
public sealed class SystemStatsMonitor {
  private readonly IObservable<SystemStats> _stats;

  // Previous GetSystemTimes readings, so CPU% is the busy fraction over the interval between polls.
  // Guarded by _cpuGate because RefCount can resubscribe on a different scheduler thread.
  private readonly object _cpuGate = new();
  private ulong _prevIdle, _prevKernel, _prevUser;
  private bool _hasPrevCpu;

  public SystemStatsMonitor(TimeSpan? pollInterval = null, IScheduler? scheduler = null) {
    var interval = pollInterval ?? TimeSpan.FromSeconds(1);
    scheduler ??= DefaultScheduler.Instance;

    _stats = Observable
        .Interval(interval, scheduler)
        .Select(_ => Sample())
        .Publish()
        .RefCount();
  }

  /// <summary>Live system totals; emits a fresh snapshot on each poll.</summary>
  public IObservable<SystemStats> Stats => _stats;

  private SystemStats Sample() {
    int processes = 0, threads = 0, handles = 0;
    foreach (var p in SysProcess.GetProcesses()) {
      try {
        processes++;
        threads += p.Threads.Count;
        // HandleCount can throw for protected processes we can't open; skip those rather
        // than let one inaccessible process abort the whole sample.
        handles += p.HandleCount;
      } catch {
        // Process exited between enumeration and read, or access was denied — ignore it.
      } finally {
        p.Dispose();
      }
    }

    return new SystemStats(processes, threads, handles, SampleCpuPercent(), MemoryPercent, MemoryTotalMb: TotalPhysicalMb);
  }

  // Whole-machine CPU busy fraction since the previous sample. Kernel time as reported already
  // includes idle time, so busy = (kernel + user) - idle over the same window. Returns 0 on the
  // first sample (no previous reading to diff against) or if the interval had no ticks.
  private double SampleCpuPercent() {
    if (!GetSystemTimes(out var idleFt, out var kernelFt, out var userFt)) return 0;

    ulong idle = idleFt.ToUInt64(), kernel = kernelFt.ToUInt64(), user = userFt.ToUInt64();
    lock (_cpuGate) {
      if (!_hasPrevCpu) {
        _prevIdle = idle; _prevKernel = kernel; _prevUser = user;
        _hasPrevCpu = true;
        return 0;
      }

      ulong idleDelta = idle - _prevIdle;
      ulong totalDelta = (kernel - _prevKernel) + (user - _prevUser);
      _prevIdle = idle; _prevKernel = kernel; _prevUser = user;

      if (totalDelta == 0) return 0;
      double busy = (double)(totalDelta - idleDelta) / totalDelta * 100.0;
      return busy < 0 ? 0 : busy > 100 ? 100 : busy;
    }
  }

  private static double MemoryPercent {
    get {
      var status = MEMORYSTATUSEX.Create();
      return GlobalMemoryStatusEx(ref status) ? status.dwMemoryLoad : 0;
    }
  }

  private static double TotalPhysicalMb {
    get {
      var status = MEMORYSTATUSEX.Create();
      return GlobalMemoryStatusEx(ref status) ? status.ullTotalPhys / (1024.0 * 1024.0) : 0;
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  private struct FILETIME {
    public uint LowDateTime;
    public uint HighDateTime;
    public readonly ulong ToUInt64() => ((ulong)HighDateTime << 32) | LowDateTime;
  }

  [StructLayout(LayoutKind.Sequential)]
  private struct MEMORYSTATUSEX {
    public uint dwLength;
    public uint dwMemoryLoad;
    public ulong ullTotalPhys;
    public ulong ullAvailPhys;
    public ulong ullTotalPageFile;
    public ulong ullAvailPageFile;
    public ulong ullTotalVirtual;
    public ulong ullAvailVirtual;
    public ulong ullAvailExtendedVirtual;

    public static MEMORYSTATUSEX Create() =>
        new() { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
  }

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool GetSystemTimes(out FILETIME idleTime, out FILETIME kernelTime, out FILETIME userTime);

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
}
