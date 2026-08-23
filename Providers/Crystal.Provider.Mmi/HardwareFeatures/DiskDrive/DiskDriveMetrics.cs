namespace Crystal.Provider.Mmi.HardwareFeatures.DiskDrive;

/// <summary>
/// Represents the metrics of a disk drive, including various properties related to its availability, 
/// capabilities, configuration, and physical characteristics.
/// </summary>
/// <param name="Availability">The availability status of the disk drive.</param>
/// <param name="BytesPerSector">The number of bytes per sector.</param>
/// <param name="Capabilities">The capabilities of the disk drive.</param>
/// <param name="CapabilityDescriptions">Descriptions of the disk drive's capabilities.</param>
/// <param name="Caption">A short description of the disk drive.</param>
/// <param name="CompressionMethod">The compression method used by the disk drive.</param>
/// <param name="ConfigManagerErrorCode">The error code for the configuration manager.</param>
/// <param name="ConfigManagerUserConfig">Indicates if the configuration is user-configured.</param>
/// <param name="CreationClassName">The class name of the creation object.</param>
/// <param name="DefaultBlockSize">The default block size of the disk drive.</param>
/// <param name="Description">A detailed description of the disk drive.</param>
/// <param name="DeviceID">The unique identifier for the disk drive.</param>
/// <param name="ErrorCleared">Indicates if the error has been cleared.</param>
/// <param name="ErrorDescription">A description of the error.</param>
/// <param name="ErrorMethodology">The methodology for handling errors.</param>
/// <param name="FirmwareRevision">The firmware revision of the disk drive.</param>
/// <param name="Index">The index of the disk drive.</param>
/// <param name="InstallDate">The date the disk drive was installed.</param>
/// <param name="InterfaceType">The type of interface used by the disk drive.</param>
/// <param name="LastErrorCode">The error code for the last error.</param>
/// <param name="Manufacturer">The manufacturer of the disk drive.</param>
/// <param name="MaxBlockSize">The maximum block size of the disk drive.</param>
/// <param name="MaxMediaSize">The maximum media size of the disk drive.</param>
/// <param name="MediaLoaded">Indicates if media is loaded in the disk drive.</param>
/// <param name="MediaType">The type of media used by the disk drive.</param>
/// <param name="MinBlockSize">The minimum block size of the disk drive.</param>
/// <param name="Model">The model of the disk drive.</param>
/// <param name="Name">The name of the disk drive.</param>
/// <param name="NeedsCleaning">Indicates if the disk drive needs cleaning.</param>
/// <param name="NumberOfMediaSupported">The number of media types supported by the disk drive.</param>
/// <param name="Partitions">The number of partitions on the disk drive.</param>
/// <param name="PNPDeviceID">The Plug and Play device identifier.</param>
/// <param name="PowerManagementCapabilities">The power management capabilities of the disk drive.</param>
/// <param name="PowerManagementSupported">Indicates if power management is supported.</param>
/// <param name="SCSIBus">The SCSI bus number.</param>
/// <param name="SCSILogicalUnit">The SCSI logical unit number.</param>
/// <param name="SCSIPort">The SCSI port number.</param>
/// <param name="SCSITargetId">The SCSI target identifier.</param>
/// <param name="SectorsPerTrack">The number of sectors per track.</param>
/// <param name="SerialNumber">The serial number of the disk drive.</param>
/// <param name="Signature">The signature of the disk drive.</param>
/// <param name="Size">The size of the disk drive.</param>
/// <param name="Status">The status of the disk drive.</param>
/// <param name="StatusInfo">Information about the status of the disk drive.</param>
/// <param name="SystemCreationClassName">The class name of the system creation object.</param>
/// <param name="SystemName">The name of the system.</param>
/// <param name="TotalCylinders">The total number of cylinders.</param>
/// <param name="TotalHeads">The total number of heads.</param>
/// <param name="TotalSectors">The total number of sectors.</param>
/// <param name="TotalTracks">The total number of tracks.</param>
/// <param name="TracksPerCylinder">The number of tracks per cylinder.</param>
public record DiskDriveMetrics(
  ushort? Availability,
  uint? BytesPerSector,
  ushort[]? Capabilities,
  string[]? CapabilityDescriptions,
  string? Caption,
  string? CompressionMethod,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  ulong? DefaultBlockSize,
  string? Description,
  string? DeviceID,
  bool? ErrorCleared,
  string? ErrorDescription,
  string? ErrorMethodology,
  string? FirmwareRevision,
  uint? Index,
  DateTime? InstallDate,
  string? InterfaceType,
  uint? LastErrorCode,
  string? Manufacturer,
  ulong? MaxBlockSize,
  ulong? MaxMediaSize,
  bool? MediaLoaded,
  string? MediaType,
  ulong? MinBlockSize,
  string? Model,
  string? Name,
  bool? NeedsCleaning,
  uint? NumberOfMediaSupported,
  uint? Partitions,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  uint? SCSIBus,
  ushort? SCSILogicalUnit,
  ushort? SCSIPort,
  ushort? SCSITargetId,
  uint? SectorsPerTrack,
  string? SerialNumber,
  uint? Signature,
  ulong? Size,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  ulong? TotalCylinders,
  uint? TotalHeads,
  ulong? TotalSectors,
  ulong? TotalTracks,
  uint? TracksPerCylinder);
