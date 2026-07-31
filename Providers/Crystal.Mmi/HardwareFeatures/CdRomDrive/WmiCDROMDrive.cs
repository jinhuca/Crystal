using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.CdRomDrive;

internal static class WmiCDROMDrive {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.CDROMDrive;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;
  public const string DeviceID = CommonWmiProperties.DeviceId;
  public const string PNPDeviceID = CommonWmiProperties.PnpDeviceId;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string CreationClassName = CommonWmiProperties.CreationClassName;
  public const string InstallDate = CommonWmiProperties.InstallDate;

  // ---------------------------------------------------------------------
  // CD-ROM Drive Specific Properties
  // ---------------------------------------------------------------------
  public const string Availability = nameof(Availability);
  public const string CapabilityDescriptions = nameof(CapabilityDescriptions);
  public const string CompressionMethod = nameof(CompressionMethod);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string DefaultBlockSize = nameof(DefaultBlockSize);
  public const string Drive = nameof(Drive);
  public const string DriveIntegrity = nameof(DriveIntegrity);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string ErrorMethodology = nameof(ErrorMethodology);
  public const string FileSystemFlags = nameof(FileSystemFlags);
  public const string FileSystemFlagsEx = nameof(FileSystemFlagsEx);
  public const string Id = nameof(Id);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string MaxBlockSize = nameof(MaxBlockSize);
  public const string MaximumComponentLength = nameof(MaximumComponentLength);
  public const string MaxMediaSize = nameof(MaxMediaSize);
  public const string MediaLoaded = nameof(MediaLoaded);
  public const string MediaType = nameof(MediaType);
  public const string MfrAssignedRevisionLevel = nameof(MfrAssignedRevisionLevel);
  public const string MinBlockSize = nameof(MinBlockSize);
  public const string NeedsCleaning = nameof(NeedsCleaning);
  public const string NumberOfMediaSupported = nameof(NumberOfMediaSupported);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string RevisionLevel = nameof(RevisionLevel);
  public const string SCSIBus = nameof(SCSIBus);
  public const string SCSILogicalUnit = nameof(SCSILogicalUnit);
  public const string SCSIPort = nameof(SCSIPort);
  public const string SCSITargetId = nameof(SCSITargetId);
  public const string SerialNumber = nameof(SerialNumber);
  public const string Size = nameof(Size);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string VolumeName = nameof(VolumeName);
  public const string VolumeSerialNumber = nameof(VolumeSerialNumber);
}
