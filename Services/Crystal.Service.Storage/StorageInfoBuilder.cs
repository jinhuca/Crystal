using Crystal.Provider.Mmi.HardwareFeatures.DiskDrive;
using Crystal.Provider.Mmi.MmiEngine;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Crystal.Service.Storage;

/// <summary>Builds the static storage inventory from WMI (<c>Win32_DiskDrive</c>),
/// mapping each physical drive and rolling up the total capacity.</summary>
public sealed class StorageInfoBuilder {
  private const double BytesPerGB = 1024.0 * 1024.0 * 1024.0;

  private readonly IWmiHardwareProvider _wmi;
  private int? _osDriveIndex;
  private bool _osDriveIndexResolved;

  public StorageInfoBuilder(IWmiHardwareProvider wmi) => _wmi = wmi;

  public async Task<StorageSnapshot> BuildAsync(CancellationToken ct) {
    // Bypass the WMI cache: the monitor re-enumerates on a slow cadence for hotplug, and the cached
    // (first-query) drive set would otherwise mask a drive being attached or removed.
    var disks = await _wmi.ToSafeDiskDriveMetricsAsync(ct, bypassCache: true);
    var osIndex = OsDriveIndex();
    var drives = disks
        .Where(d => !string.IsNullOrWhiteSpace(d.Model) || !string.IsNullOrWhiteSpace(d.Caption))
        .Select(d => ToDrive(d, osIndex))
        .ToList();

    return new StorageSnapshot(
        Drives: drives,
        TotalCapacityGB: drives.Sum(d => d.CapacityGB ?? 0),
        DriveCount: drives.Count);
  }

  private static StorageDriveInfo ToDrive(DiskDriveMetrics d, int? osIndex) => new(
      Model: d.Model ?? d.Caption ?? "Unknown drive",
      CapacityGB: d.Size is { } bytes ? Math.Round(bytes / BytesPerGB, 1) : null,
      InterfaceType: d.InterfaceType,
      MediaType: d.MediaType,
      Manufacturer: d.Manufacturer?.Trim(),
      SerialNumber: d.SerialNumber?.Trim(),
      FirmwareRevision: d.FirmwareRevision?.Trim(),
      Partitions: d.Partitions,
      DriveIndex: (int?)d.Index,
      IsSystemDisk: d.Index is { } idx && osIndex == (int)idx);

  // The OS/system disk is the physical disk hosting the Windows volume (e.g. C:), which the UI selects
  // by default. Resolved once (the boot disk doesn't change at runtime): ask the storage stack which
  // physical disk backs the system volume (locale-independent, no admin), and only if that fails fall
  // back to parsing the PhysicalDisk perf counters. Left null when neither resolves, in which case no
  // drive is flagged and the UI selects the first disk.
  private int? OsDriveIndex() {
    if (_osDriveIndexResolved) return _osDriveIndex;
    _osDriveIndexResolved = true;
    _osDriveIndex = QueryOsDriveIndexByVolume() ?? QueryOsDriveIndexByPerfCounter();
    return _osDriveIndex;
  }

  // Map the system volume to its physical-disk number via IOCTL_STORAGE_GET_DEVICE_NUMBER — exactly
  // how Disk Management / Task Manager relate a drive letter to "Disk N". Opening \\.\C: for query
  // (zero access rights) needs no elevation.
  private static int? QueryOsDriveIndexByVolume() {
    try {
      var systemDrive = Path.GetPathRoot(Environment.SystemDirectory)?.TrimEnd('\\');
      if (string.IsNullOrEmpty(systemDrive)) return null;

      using var handle = CreateFile($@"\\.\{systemDrive}", 0, FileShare.ReadWrite,
          IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
      if (handle.IsInvalid) return null;

      var number = new StorageDeviceNumber();
      if (DeviceIoControl(handle, IoctlStorageGetDeviceNumber, IntPtr.Zero, 0,
              ref number, Marshal.SizeOf<StorageDeviceNumber>(), out _, IntPtr.Zero))
        return number.DeviceNumber;
    }
    catch {
      // API/marshalling failure — fall back to the perf-counter method.
    }
    return null;
  }

  // Fallback: Windows names its PhysicalDisk perf-counter instances "<index> <letters>" ("0 C:") —
  // the same convention StorageLoadSource matches on — so find the instance whose letters include the
  // system drive and take its leading physical-disk index. Can throw / come back empty when the
  // counters are disabled or their names are localized, in which case no drive is flagged.
  private static int? QueryOsDriveIndexByPerfCounter() {
    try {
      var systemDrive = Path.GetPathRoot(Environment.SystemDirectory)?.TrimEnd('\\');
      if (string.IsNullOrEmpty(systemDrive)) return null;

      var category = new PerformanceCounterCategory("PhysicalDisk");
      foreach (var name in category.GetInstanceNames()) {
        var tokens = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length >= 2 && int.TryParse(tokens[0], out var index)
            && tokens.Skip(1).Any(t => string.Equals(t, systemDrive, StringComparison.OrdinalIgnoreCase)))
          return index;
      }
    }
    catch {
      // Perf counters disabled/unavailable — leave unresolved; selection falls back to the first disk.
    }
    return null;
  }

  private const uint IoctlStorageGetDeviceNumber = 0x2D1080;

  [StructLayout(LayoutKind.Sequential)]
  private struct StorageDeviceNumber {
    public int DeviceType;
    public int DeviceNumber;
    public int PartitionNumber;
  }

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  private static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess,
      FileShare shareMode, IntPtr securityAttributes, FileMode creationDisposition,
      uint flagsAndAttributes, IntPtr templateFile);

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool DeviceIoControl(SafeFileHandle device, uint ioControlCode,
      IntPtr inBuffer, uint inBufferSize, ref StorageDeviceNumber outBuffer, int outBufferSize,
      out uint bytesReturned, IntPtr overlapped);
}
