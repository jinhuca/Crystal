using Crystal.Provider.Mmi.HardwareFeatures.DiskDrive;
using Crystal.Provider.Mmi.MmiEngine;
using System.Diagnostics;

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

  // The OS/system disk is the physical disk hosting the Windows volume (e.g. C:). Windows names its
  // PhysicalDisk perf-counter instances "<index> <letters>" ("0 C:") — the same convention
  // StorageLoadSource matches on — so find the instance whose letters include the system drive and
  // take its leading physical-disk index. Resolved once (the boot disk doesn't change at runtime);
  // left null when counters are unavailable, in which case no drive is flagged as the system disk.
  private int? OsDriveIndex() {
    if (_osDriveIndexResolved) return _osDriveIndex;
    _osDriveIndexResolved = true;
    try {
      var systemDrive = Path.GetPathRoot(Environment.SystemDirectory)?.TrimEnd('\\');
      if (!string.IsNullOrEmpty(systemDrive)) {
        var category = new PerformanceCounterCategory("PhysicalDisk");
        foreach (var name in category.GetInstanceNames()) {
          var tokens = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
          if (tokens.Length >= 2 && int.TryParse(tokens[0], out var index)
              && tokens.Skip(1).Any(t => string.Equals(t, systemDrive, StringComparison.OrdinalIgnoreCase))) {
            _osDriveIndex = index;
            break;
          }
        }
      }
    }
    catch {
      // Perf counters disabled/unavailable — leave unresolved; selection falls back to the first disk.
    }
    return _osDriveIndex;
  }
}
