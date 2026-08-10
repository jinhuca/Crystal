using Crystal.Provider.Mmi.HardwareFeatures.DiskDrive;
using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Service.Storage;

/// <summary>Builds the static storage inventory from WMI (<c>Win32_DiskDrive</c>),
/// mapping each physical drive and rolling up the total capacity.</summary>
public sealed class StorageInfoBuilder {
  private const double BytesPerGB = 1024.0 * 1024.0 * 1024.0;

  private readonly IWmiHardwareProvider _wmi;

  public StorageInfoBuilder(IWmiHardwareProvider wmi) => _wmi = wmi;

  public async Task<StorageSnapshot> BuildAsync(CancellationToken ct) {
    var disks = await _wmi.ToSafeDiskDriveMetricsAsync(ct);
    var drives = disks
        .Where(d => !string.IsNullOrWhiteSpace(d.Model) || !string.IsNullOrWhiteSpace(d.Caption))
        .Select(ToDrive)
        .ToList();

    return new StorageSnapshot(
        Drives: drives,
        TotalCapacityGB: drives.Sum(d => d.CapacityGB ?? 0),
        DriveCount: drives.Count);
  }

  private static StorageDriveInfo ToDrive(DiskDriveMetrics d) => new(
      Model: d.Model ?? d.Caption ?? "Unknown drive",
      CapacityGB: d.Size is { } bytes ? Math.Round(bytes / BytesPerGB, 1) : null,
      InterfaceType: d.InterfaceType,
      MediaType: d.MediaType,
      Manufacturer: d.Manufacturer?.Trim(),
      SerialNumber: d.SerialNumber?.Trim(),
      FirmwareRevision: d.FirmwareRevision?.Trim(),
      Partitions: d.Partitions,
      DriveIndex: (int?)d.Index);
}
