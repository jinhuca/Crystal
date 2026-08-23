namespace Crystal.Provider.Mmi.HardwareFeatures.DiskPartition;

/// <summary>
/// Represents the metrics of a disk partition, including availability, boot status, configuration, 
/// size, and other relevant properties. This record is used to encapsulate the state and characteristics 
/// of a disk partition in a structured manner.
/// </summary>
/// <param name="Availability">The availability of the disk partition.</param>
/// <param name="Bootable">A value indicating whether the disk partition is bootable.</param>
/// <param name="BootPartition">A value indicating whether the disk partition is a boot partition.</param>
/// <param name="Caption">The caption of the disk partition.</param>
/// <param name="ConfigManagerErrorCode">The error code for the configuration manager.</param>
/// <param name="ConfigManagerUserConfig">A value indicating whether the configuration manager is user-configured.</param>
/// <param name="CreationClassName">The creation class name of the disk partition.</param>
/// <param name="BlockSize">The block size of the disk partition.</param>
/// <param name="Description">The description of the disk partition.</param>
/// <param name="DeviceID">The device ID of the disk partition.</param>
/// <param name="DiskIndex">The index of the disk containing the partition.</param>
/// <param name="ErrorCleared">A value indicating whether the error has been cleared.</param>
/// <param name="ErrorDescription">The description of the error.</param>
/// <param name="ErrorMethodology">The methodology for handling errors.</param>
/// <param name="Index">The index of the disk partition.</param>
/// <param name="InstallDate">The installation date of the disk partition.</param>
/// <param name="LastErrorCode">The last error code.</param>
/// <param name="Name">The name of the disk partition.</param>
/// <param name="NumberOfBlocks">The number of blocks in the disk partition.</param>
/// <param name="PrimaryPartition">A value indicating whether the disk partition is primary.</param>
/// <param name="PNPDeviceID">The PNP device ID of the disk partition.</param>
/// <param name="PowerManagementCapabilities">The power management capabilities of the disk partition.</param>
/// <param name="PowerManagementSupported">A value indicating whether power management is supported.</param>
/// <param name="Purpose">The purpose of the disk partition.</param>
/// <param name="Size">The size of the disk partition.</param>
/// <param name="StartingOffset">The starting offset of the disk partition.</param>
/// <param name="Status">The status of the disk partition.</param>
/// <param name="StatusInfo">The status information of the disk partition.</param>
/// <param name="SystemCreationClassName">The creation class name of the system.</param>
/// <param name="SystemName">The name of the system.</param>
/// <param name="TargetOperatingSystem">The target operating system.</param>
/// <param name="Type">The type of the disk partition.</param>
public record DiskPartitionMetrics(
  ushort? Availability,
  bool? Bootable,
  bool? BootPartition,
  string? Caption,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  ulong? BlockSize,            // Maps from CIM_StorageExtent.BlockSize (uint64)
  string? Description,
  string? DeviceID,            // Key identifier (e.g., "Disk #0, Partition #1")
  uint? DiskIndex,
  bool? ErrorCleared,
  string? ErrorDescription,
  string? ErrorMethodology,
  uint? Index,
  DateTime? InstallDate,
  uint? LastErrorCode,
  string? Name,
  ulong? NumberOfBlocks,       // Maps from CIM_StorageExtent.NumberOfBlocks (uint64)
  bool? PrimaryPartition,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? Purpose,
  ulong? Size,                 // Partition capacity in bytes (uint64)
  ulong? StartingOffset,       // Partition start address (uint64)
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  ushort? TargetOperatingSystem,
  ushort? Type                 // e.g., 12 = GPT, 1 = Extended, etc.
);
