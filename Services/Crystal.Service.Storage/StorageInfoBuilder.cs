using Crystal.Provider.Mmi.HardwareFeatures.DiskDrive;
using Crystal.Provider.Mmi.MmiEngine;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Crystal.Service.Storage;

/// <summary>
/// Builds the static storage inventory from WMI (<c>Win32_DiskDrive</c>),
/// mapping each physical drive and rolling up the total capacity.
/// </summary>
public sealed class StorageInfoBuilder {
  /// <summary>
  /// Bytes in one gigabyte, used to convert raw drive capacities to GB.
  /// </summary>
  private const double BytesPerGB = 1024.0 * 1024.0 * 1024.0;

  /// <summary>
  /// The WMI provider queried for the physical-drive inventory.
  /// </summary>
  private readonly IWmiHardwareProvider _wmi;

  /// <summary>
  /// Cached OS/system physical-disk number; the boot disk doesn't change at runtime, so it is
  /// resolved lazily and reused. Null means "not the system disk" or "couldn't be determined".
  /// </summary>
  private int? _osDriveIndex;

  /// <summary>
  /// Whether <see cref="_osDriveIndex"/> has been resolved yet, so a null result (unresolved)
  /// isn't re-queried on every build.
  /// </summary>
  private bool _osDriveIndexResolved;

  /// <summary>
  /// Creates a builder over the given WMI hardware provider.
  /// </summary>
  /// <param name="wmi">The provider used to enumerate <c>Win32_DiskDrive</c>.</param>
  public StorageInfoBuilder(IWmiHardwareProvider wmi) => _wmi = wmi;

  /// <summary>
  /// Enumerates the physical drives from WMI (bypassing the cache so hotplug is visible),
  /// maps each to a <see cref="StorageDriveInfo"/> with the system disk flagged, and rolls up the
  /// total capacity and drive count into a <see cref="StorageSnapshot"/>. Drives with neither a model
  /// nor a caption (placeholder/phantom entries) are filtered out.
  /// </summary>
  /// <param name="ct">Cancellation token for the underlying WMI query.</param>
  /// <returns>The current storage inventory snapshot.</returns>
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

  /// <summary>
  /// Maps a raw <see cref="DiskDriveMetrics"/> WMI row to a <see cref="StorageDriveInfo"/>,
  /// converting the byte capacity to rounded GB, trimming string fields, and flagging the disk as the
  /// system disk when its index matches the resolved OS-drive index.
  /// </summary>
  /// <param name="d">The raw WMI disk-drive metrics.</param>
  /// <param name="osIndex">The resolved OS/system physical-disk number, or null when unknown.</param>
  /// <returns>The mapped drive info.</returns>
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

  /// <summary>
  /// Resolves and caches the OS/system physical-disk number, preferring the volume→device IOCTL and
  /// falling back to the PhysicalDisk perf counters. Returns null when neither resolves.
  /// </summary>
  /// <remarks>
  /// The OS/system disk is the physical disk hosting the Windows volume (e.g. C:), which the UI selects
  /// by default. Resolved once (the boot disk doesn't change at runtime): ask the storage stack which
  /// physical disk backs the system volume (locale-independent, no admin), and only if that fails fall
  /// back to parsing the PhysicalDisk perf counters. Left null when neither resolves, in which case no
  /// drive is flagged and the UI selects the first disk.
  /// </remarks>
  private int? OsDriveIndex() {
    if (_osDriveIndexResolved) return _osDriveIndex;
    _osDriveIndexResolved = true;
    _osDriveIndex = QueryOsDriveIndexByVolume() ?? QueryOsDriveIndexByPerfCounter();
    return _osDriveIndex;
  }

  /// <summary>
  /// Resolves the system volume's physical-disk number via <c>IOCTL_STORAGE_GET_DEVICE_NUMBER</c>.
  /// Returns null (so the caller falls back) when the volume can't be determined, the handle is
  /// invalid, or the interop call fails.
  /// </summary>
  /// <remarks>
  /// Maps the system volume to its physical-disk number via IOCTL_STORAGE_GET_DEVICE_NUMBER — exactly
  /// how Disk Management / Task Manager relate a drive letter to "Disk N". Opening \\.\C: for query
  /// (zero access rights) needs no elevation.
  /// </remarks>
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

  /// <summary>
  /// Fallback resolver: scans the PhysicalDisk perf-counter instance names for the one whose drive
  /// letters include the system drive and returns its leading physical-disk index. Returns null when
  /// the counters are disabled, localized, or don't include the system drive.
  /// </summary>
  /// <remarks>
  /// Windows names its PhysicalDisk perf-counter instances "&lt;index&gt; &lt;letters&gt;" ("0 C:") —
  /// the same convention StorageLoadSource matches on — so find the instance whose letters include the
  /// system drive and take its leading physical-disk index.
  /// </remarks>
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

  /// <summary>
  /// The <c>IOCTL_STORAGE_GET_DEVICE_NUMBER</c> control code passed to
  /// <see cref="DeviceIoControl"/> to map an opened volume to its physical-disk number.
  /// </summary>
  private const uint IoctlStorageGetDeviceNumber = 0x2D1080;

  /// <summary>
  /// Managed layout of the Win32 <c>STORAGE_DEVICE_NUMBER</c> struct filled by the IOCTL;
  /// <see cref="DeviceNumber"/> is the physical-disk index this builder wants.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  private struct StorageDeviceNumber {
    public int DeviceType;
    public int DeviceNumber;
    public int PartitionNumber;
  }

  /// <summary>
  /// P/Invoke for Win32 <c>CreateFileW</c>, used to open the system volume (<c>\\.\C:</c>) with
  /// zero access rights so no elevation is required for the device-number query.
  /// </summary>
  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  private static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess,
      FileShare shareMode, IntPtr securityAttributes, FileMode creationDisposition,
      uint flagsAndAttributes, IntPtr templateFile);

  /// <summary>
  /// P/Invoke for Win32 <c>DeviceIoControl</c>, used with
  /// <see cref="IoctlStorageGetDeviceNumber"/> to fill a <see cref="StorageDeviceNumber"/> for the
  /// opened volume.
  /// </summary>
  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool DeviceIoControl(SafeFileHandle device, uint ioControlCode,
      IntPtr inBuffer, uint inBufferSize, ref StorageDeviceNumber outBuffer, int outBufferSize,
      out uint bytesReturned, IntPtr overlapped);
}
