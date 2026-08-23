namespace Crystal.Provider.Mmi.HardwareFeatures.FloppyDrive;

/// <summary>
/// Represents the metrics of a floppy drive, including its availability, capabilities, configuration, 
/// and status information. This record is used to encapsulate the properties of a floppy drive 
/// as retrieved from WMI (Windows Management Instrumentation).
///  Win32_FloppyDrive is derived from CIM_DisketteDrive -> CIM_MediaAccessDevice, so it
/// carries the generic removable-media-device field set (Capabilities/MaxMediaSize/
/// NeedsCleaning/etc.) rather than any floppy-specific properties of its own.
/// </summary>
/// <param name="Availability">Availability of the floppy drive.</param>
/// <param name="Capabilities">Capabilities of the floppy drive.</param>
/// <param name="CapabilityDescriptions">Descriptions of the floppy drive's capabilities.</param>
/// <param name="Caption">Caption of the floppy drive.</param>
/// <param name="CompressionMethod">Compression method used by the floppy drive.</param>
/// <param name="ConfigManagerErrorCode">Error code from the configuration manager.</param>
/// <param name="ConfigManagerUserConfig">Indicates if the configuration is user-configured.</param>
/// <param name="CreationClassName">The creation class name.</param>
/// <param name="DefaultBlockSize">The default block size.</param>
/// <param name="Description">The description of the floppy drive.</param>
/// <param name="DeviceID">The device ID.</param>
/// <param name="ErrorCleared">Indicates if the error is cleared.</param>
/// <param name="ErrorDescription">The description of the error.</param>
/// <param name="ErrorMethodology">The methodology for error handling.</param>
/// <param name="InstallDate">The installation date.</param>
/// <param name="LastErrorCode">The last error code.</param>
/// <param name="Manufacturer">The manufacturer of the floppy drive.</param>
/// <param name="MaxBlockSize">The maximum block size.</param>
/// <param name="MaxMediaSize">The maximum media size.</param>
/// <param name="MinBlockSize">The minimum block size.</param>
/// <param name="Name">The name of the floppy drive.</param>
/// <param name="NeedsCleaning">Indicates if the floppy drive needs cleaning.</param>
/// <param name="NumberOfMediaSupported">The number of media supported.</param>
/// <param name="PNPDeviceID">The PNP device ID.</param>
/// <param name="PowerManagementCapabilities">The power management capabilities.</param>
/// <param name="PowerManagementSupported">Indicates if power management is supported.</param>
/// <param name="Status">The status of the floppy drive.</param>
/// <param name="StatusInfo">The status information.</param>
/// <param name="SystemCreationClassName">The system creation class name.</param>
/// <param name="SystemName">The system name.</param>
public record FloppyDriveMetrics(
  ushort? Availability,
  ushort[]? Capabilities,
  string[]? CapabilityDescriptions,
  string? Caption,
  ushort? CompressionMethod,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  ulong? DefaultBlockSize,
  string? Description,
  string? DeviceID,
  bool? ErrorCleared,
  string? ErrorDescription,
  string? ErrorMethodology,
  DateTime? InstallDate,
  uint? LastErrorCode,
  string? Manufacturer,
  ulong? MaxBlockSize,
  ulong? MaxMediaSize,
  ulong? MinBlockSize,
  string? Name,
  bool? NeedsCleaning,
  uint? NumberOfMediaSupported,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName
);
