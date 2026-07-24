using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TryIt01_PInvoke;

public sealed class CpuMemoryMonitor {
  #region Win32

  [StructLayout(LayoutKind.Sequential)]
  private struct FILETIME {
    public uint Low;
    public uint High;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ToUInt64() => ((ulong)High << 32) | Low;
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
  }

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool GetSystemTimes(
      out FILETIME idle,
      out FILETIME kernel,
      out FILETIME user);

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool GlobalMemoryStatusEx(
      ref MEMORYSTATUSEX lpBuffer);

  #endregion

  private ulong _lastIdle;
  private ulong _lastKernel;
  private ulong _lastUser;
  private bool _initialized;

  public readonly struct Sample {
    public readonly double CpuUsagePercent;
    public readonly double MemoryUsagePercent;

    public readonly ulong TotalMemory;
    public readonly ulong AvailableMemory;

    public Sample(
        double cpu,
        double memory,
        ulong total,
        ulong available) {
      CpuUsagePercent = cpu;
      MemoryUsagePercent = memory;
      TotalMemory = total;
      AvailableMemory = available;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Sample GetSample() {
    GetSystemTimes(out var idleFt, out var kernelFt, out var userFt);

    ulong idle = idleFt.ToUInt64();
    ulong kernel = kernelFt.ToUInt64();
    ulong user = userFt.ToUInt64();

    double cpu = 0;

    if (_initialized) {
      ulong idleDelta = idle - _lastIdle;
      ulong kernelDelta = kernel - _lastKernel;
      ulong userDelta = user - _lastUser;

      ulong total = kernelDelta + userDelta;

      if (total != 0) {
        cpu = 100.0 *
              (total - idleDelta) /
              total;
      }
    }

    _lastIdle = idle;
    _lastKernel = kernel;
    _lastUser = user;
    _initialized = true;

    MEMORYSTATUSEX mem = default;
    mem.dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();

    GlobalMemoryStatusEx(ref mem);

    return new Sample(
        cpu,
        mem.dwMemoryLoad,
        mem.ullTotalPhys,
        mem.ullAvailPhys);
  }
}