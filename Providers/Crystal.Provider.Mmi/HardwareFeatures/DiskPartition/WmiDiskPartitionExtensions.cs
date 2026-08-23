using Crystal.Provider.Mmi.HardwareFeatures.DiskDrive;
using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.DiskPartition;

/// <summary>
/// Extension methods for <see cref="IWmiHardwareProvider"/> to read and resolve disk partition metrics from WMI.
/// </summary>
public static class WmiDiskPartitionExtensions {
  /// <summary>
  /// Fetches disk partition metrics from WMI and safely converts them into a list of <see cref="DiskPartitionMetrics"/> records.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>A task that represents the asynchronous operation and returns a list of disk partition metrics.</returns>
  public static async Task<IReadOnlyList<DiskPartitionMetrics>> ToSafeDiskPartitionMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiDiskPartition.ClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<DiskPartitionMetrics>();
      }

      var results = new List<DiskPartitionMetrics>(instancesData.Count);

      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String 
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int 
          ? v.AsInt() : null;
        bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool 
          ? v.AsBool() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime 
          ? v.AsDateTime() : null;
        ushort[]? GetUShortArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray 
          ? v.AsUShortArray() : null;
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong 
          ? v.AsReadOnlyULong() : null;

        results.Add(new DiskPartitionMetrics(
          Availability: (ushort?)GetInt(WmiDiskPartition.Availability),
          Bootable: GetBool(WmiDiskPartition.Bootable),
          BootPartition: GetBool(WmiDiskPartition.BootPartition),
          Caption: GetStr(WmiDiskPartition.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiDiskPartition.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiDiskPartition.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiDiskPartition.CreationClassName),
          BlockSize: GetULong(WmiDiskPartition.BlockSize),
          Description: GetStr(WmiDiskPartition.Description),
          DeviceID: GetStr(WmiDiskPartition.DeviceID),
          DiskIndex: (uint?)GetInt(WmiDiskPartition.DiskIndex),
          ErrorCleared: GetBool(WmiDiskPartition.ErrorCleared),
          ErrorDescription: GetStr(WmiDiskPartition.ErrorDescription),
          ErrorMethodology: GetStr(WmiDiskPartition.ErrorMethodology),
          Index: (uint?)GetInt(WmiDiskPartition.Index),
          InstallDate: GetDate(WmiDiskPartition.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiDiskPartition.LastErrorCode),
          Name: GetStr(WmiDiskPartition.Name),
          NumberOfBlocks: GetULong(WmiDiskPartition.NumberOfBlocks),
          PrimaryPartition: GetBool(WmiDiskPartition.PrimaryPartition),
          PNPDeviceID: GetStr(WmiDiskPartition.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiDiskPartition.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiDiskPartition.PowerManagementSupported),
          Purpose: GetStr(WmiDiskPartition.Purpose),
          Size: GetULong(WmiDiskPartition.Size),
          StartingOffset: GetULong(WmiDiskPartition.StartingOffset),
          Status: GetStr(WmiDiskPartition.Status),
          StatusInfo: (ushort?)GetInt(WmiDiskPartition.StatusInfo),
          SystemCreationClassName: GetStr(WmiDiskPartition.SystemCreationClassName),
          SystemName: GetStr(WmiDiskPartition.SystemName),
          TargetOperatingSystem: (ushort?)GetInt(WmiDiskPartition.TargetOperatingSystem),
          Type: (ushort?)GetInt(WmiDiskPartition.Type)));
      }

      return results;
    }
    catch {
      return Array.Empty<DiskPartitionMetrics>();
    }
  }

  /// <summary>
  /// Fetches disk drive and partition metrics from WMI and resolves them into a structured topology of physical drives, 
  /// their partitions, and associated volume letters.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>A task that represents the asynchronous operation and returns a list of resolved physical drives.</returns>
  public static async Task<IReadOnlyList<ResolvedPhysicalDrive>> ToResolvedDriveTopologyAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    // 1. Fetch the data collections concurrently using our engine
    var drivesTask = provider.ToSafeDiskDriveMetricsAsync(cancellationToken);
    var partitionsTask = provider.ToSafeDiskPartitionMetricsAsync(cancellationToken);

    // We fetch raw dictionaries for the bridge tables to build quick lookups
    var driveToPartitionTask = provider.GetMultiMetricsForClassAsync("Win32_DiskDriveToDiskPartition", cancellationToken);
    var partitionToLogicalTask = provider.GetMultiMetricsForClassAsync("Win32_LogicalDiskToPartition", cancellationToken);

    await Task.WhenAll(drivesTask, partitionsTask, driveToPartitionTask, partitionToLogicalTask);

    var drives = drivesTask.Result;
    var partitions = partitionsTask.Result;

    // 2. Build map: Partition DeviceID -> List of Drive Letters
    // WMI Format for Dependent: "Win32_LogicalDisk.DeviceID=\"C:\""
    // WMI Format for Antecedent: "Win32_DiskPartition.DeviceID=\"Disk #0, Partition #1\""
    var partitionToLetterMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
    foreach (var rel in partitionToLogicalTask.Result) {
      string? dependent = rel.TryGetValue("Dependent", out var dep) ? dep.AsString() : null;
      string? antecedent = rel.TryGetValue("Antecedent", out var ant) ? ant.AsString() : null;

      if (dependent != null && antecedent != null) {
        string driveLetter = dependent.Split("DeviceID=\"").LastOrDefault()?.TrimEnd('"') ?? "";
        string partitionId = antecedent.Split("DeviceID=\"").LastOrDefault()?.TrimEnd('"') ?? "";

        if (!string.IsNullOrEmpty(driveLetter) && !string.IsNullOrEmpty(partitionId)) {
          if (!partitionToLetterMap.TryGetValue(partitionId, out var letters)) {
            letters = new List<string>();
            partitionToLetterMap[partitionId] = letters;
          }
          letters.Add(driveLetter);
        }
      }
    }

    // 3. Build map: Drive DeviceID -> List of Partitions
    // WMI Format for Antecedent: "Win32_DiskDrive.DeviceID=\"\\\\.\\PHYSICALDRIVE0\""
    // WMI Format for Dependent: "Win32_DiskPartition.DeviceID=\"Disk #0, Partition #1\""
    var driveToPartitionMap = new Dictionary<string, List<ResolvedPartition>>(StringComparer.OrdinalIgnoreCase);
    foreach (var rel in driveToPartitionTask.Result) {
      string? antecedent = rel.TryGetValue("Antecedent", out var ant) ? ant.AsString() : null;
      string? dependent = rel.TryGetValue("Dependent", out var dep) ? dep.AsString() : null;

      if (antecedent != null && dependent != null) {
        string driveId = antecedent.Split("DeviceID=\"").LastOrDefault()?.TrimEnd('"')?.Replace(@"\\", @"\") ?? "";
        string partitionId = dependent.Split("DeviceID=\"").LastOrDefault()?.TrimEnd('"') ?? "";

        var targetPartition = partitions.FirstOrDefault(p => p.DeviceID != null && p.DeviceID.Equals(partitionId, StringComparison.OrdinalIgnoreCase));
        if (targetPartition != null) {
          partitionToLetterMap.TryGetValue(partitionId, out var letters);
          var resolvedPartition = new ResolvedPartition(targetPartition, letters ?? (IReadOnlyList<string>)Array.Empty<string>());

          if (!driveToPartitionMap.TryGetValue(driveId, out var partitionList)) {
            partitionList = new List<ResolvedPartition>();
            driveToPartitionMap[driveId] = partitionList;
          }
          partitionList.Add(resolvedPartition);
        }
      }
    }

    // 4. Assemble final structured topology
    var resolvedDrives = new List<ResolvedPhysicalDrive>(drives.Count);
    foreach (var drive in drives) {
      driveToPartitionMap.TryGetValue(drive.DeviceID ?? "", out var matchedPartitions);
      resolvedDrives.Add(new ResolvedPhysicalDrive(drive, matchedPartitions ?? (IReadOnlyList<ResolvedPartition>)Array.Empty<ResolvedPartition>()));
    }

    return resolvedDrives;
  }
}

/// <summary>
/// Represents a resolved disk partition with its metrics and associated volume letters.
/// </summary>
/// <param name="PartitionInfo">The metrics for the resolved disk partition.</param>
/// <param name="VolumeLetters">The list of volume letters associated with the partition.</param>
public record ResolvedPartition(
    DiskPartitionMetrics PartitionInfo,
    IReadOnlyList<string> VolumeLetters // Can be empty, or contain multiple letters (e.g., "C:", "D:")
);

/// <summary>
/// Represents a resolved physical drive with its metrics and associated partitions.
/// </summary>
/// <param name="DriveInfo">The metrics for the resolved physical drive.</param>
/// <param name="Partitions">The list of resolved partitions associated with the drive.</param>
public record ResolvedPhysicalDrive(
    DiskDriveMetrics DriveInfo,
    IReadOnlyList<ResolvedPartition> Partitions
);
