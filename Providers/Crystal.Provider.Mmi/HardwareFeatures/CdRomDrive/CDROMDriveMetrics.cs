namespace Crystal.Provider.Mmi.HardwareFeatures.CdRomDrive;

/// <summary>
/// Represents the metrics of a CD-ROM drive, including availability, capabilities, and various properties related to the drive's configuration and status.
/// </summary>
/// <param name="Availability">The availability of the CD-ROM drive.</param>
/// <param name="CapabilityDescriptions">The descriptions of the capabilities of the CD-ROM drive.</param>
/// <param name="Caption">The caption of the CD-ROM drive.</param>
/// <param name="CompressionMethod">The compression method used by the CD-ROM drive.</param>
/// <param name="ConfigManagerErrorCode">The error code of the configuration manager for the CD-ROM drive.</param>
/// <param name="ConfigManagerUserConfig">A value indicating whether the configuration manager is user-configured for the CD-ROM drive.</param>
/// <param name="CreationClassName">The creation class name of the CD-ROM drive.</param>
/// <param name="DefaultBlockSize">The default block size of the CD-ROM drive.</param>
/// <param name="Description">The description of the CD-ROM drive.</param>
/// <param name="DeviceID">The device ID of the CD-ROM drive.</param>
/// <param name="Drive">The drive letter of the CD-ROM drive.</param>
/// <param name="DriveIntegrity">A value indicating whether the drive integrity is maintained.</param>
/// <param name="ErrorCleared">A value indicating whether the error is cleared.</param>
/// <param name="ErrorDescription">The description of the error.</param>
/// <param name="ErrorMethodology">The methodology for handling errors.</param>
/// <param name="FileSystemFlags">The flags for the file system of the CD-ROM drive.</param>
/// <param name="FileSystemFlagsEx">The extended flags for the file system of the CD-ROM drive.</param>
/// <param name="Id">The ID of the CD-ROM drive.</param>
/// <param name="InstallDate">The installation date of the CD-ROM drive.</param>
/// <param name="LastErrorCode">The error code of the last error.</param>
/// <param name="Manufacturer">The manufacturer of the CD-ROM drive.</param>
/// <param name="MaxBlockSize">The maximum block size of the CD-ROM drive.</param>
/// <param name="MaximumComponentLength">The maximum length of a component in the CD-ROM drive.</param>
/// <param name="MaxMediaSize">The maximum media size of the CD-ROM drive.</param>
/// <param name="MediaLoaded">A value indicating whether media is loaded in the CD-ROM drive.</param>
/// <param name="MediaType">The type of media supported by the CD-ROM drive.</param>
/// <param name="MfrAssignedRevisionLevel">The revision level assigned by the manufacturer.</param>
/// <param name="MinBlockSize">The minimum block size of the CD-ROM drive.</param>
/// <param name="Name">The name of the CD-ROM drive.</param>
/// <param name="NeedsCleaning">A value indicating whether the CD-ROM drive needs cleaning.</param>
/// <param name="NumberOfMediaSupported">The number of media types supported by the CD-ROM drive.</param>
/// <param name="PNPDeviceID">The Plug and Play device ID of the CD-ROM drive.</param>
/// <param name="PowerManagementCapabilities">The capabilities for power management of the CD-ROM drive.</param>
/// <param name="PowerManagementSupported">A value indicating whether power management is supported by the CD-ROM drive.</param>
/// <param name="RevisionLevel">The revision level of the CD-ROM drive.</param>
/// <param name="SCSIBus">The SCSI bus to which the CD-ROM drive is connected.</param>
/// <param name="SCSILogicalUnit">The logical unit of the SCSI bus.</param>
/// <param name="SCSIPort">The port of the SCSI bus.</param>
/// <param name="SCSITargetId">The target ID of the SCSI bus.</param>
/// <param name="SerialNumber">The serial number of the CD-ROM drive.</param>
/// <param name="Size">The size of the CD-ROM drive.</param>
/// <param name="Status">The status of the CD-ROM drive.</param>
/// <param name="StatusInfo">The status information of the CD-ROM drive.</param>
/// <param name="SystemCreationClassName">The creation class name of the system to which the CD-ROM drive belongs.</param>
/// <param name="SystemName">The name of the system to which the CD-ROM drive belongs.</param>
/// <param name="VolumeName">The name of the volume on the CD-ROM drive.</param>
/// <param name="VolumeSerialNumber">The serial number of the volume on the CD-ROM drive.</param>
public record CDROMDriveMetrics(
  ushort? Availability,
  string[]? CapabilityDescriptions,
  string? Caption,
  string? CompressionMethod,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  ulong? DefaultBlockSize,
  string? Description,
  string? DeviceID,
  string? Drive,
  bool? DriveIntegrity,
  bool? ErrorCleared,
  string? ErrorDescription,
  string? ErrorMethodology,
  ushort? FileSystemFlags,
  uint? FileSystemFlagsEx,
  string? Id,
  DateTime? InstallDate,
  uint? LastErrorCode,
  string? Manufacturer,
  ulong? MaxBlockSize,
  uint? MaximumComponentLength,
  ulong? MaxMediaSize,
  bool? MediaLoaded,
  string? MediaType,
  string? MfrAssignedRevisionLevel,
  ulong? MinBlockSize,
  string? Name,
  bool? NeedsCleaning,
  uint? NumberOfMediaSupported,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? RevisionLevel,
  uint? SCSIBus,
  ushort? SCSILogicalUnit,
  ushort? SCSIPort,
  ushort? SCSITargetId,
  string? SerialNumber,
  ulong? Size,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  string? VolumeName,
  string? VolumeSerialNumber
);
