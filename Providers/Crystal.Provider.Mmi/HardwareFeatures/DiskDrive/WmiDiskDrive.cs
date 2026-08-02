using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.DiskDrive;

internal static class WmiDiskDrive {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName =      WmiClasses.DiskDrive;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption =      CommonWmiProperties.Caption;
  public const string Description =      CommonWmiProperties.Description;
  public const string Manufacturer =      CommonWmiProperties.Manufacturer;
  public const string Status =      CommonWmiProperties.Status;
  public const string DeviceID =      CommonWmiProperties.DeviceId;
  public const string PNPDeviceID =      CommonWmiProperties.PnpDeviceId;

  // ---------------------------------------------------------------------
  // Disk Specific Specific Properties
  // ---------------------------------------------------------------------
  public const string Availability = nameof(Availability);
  public const string BytesPerSector = nameof(BytesPerSector);
  public const string Capabilities = nameof(Capabilities);
  public const string CapabilityDescriptions = nameof(CapabilityDescriptions);
  public const string CompressionMethod = nameof(CompressionMethod);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string CreationClassName = nameof(CreationClassName);
  public const string DefaultBlockSize = nameof(DefaultBlockSize);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string ErrorMethodology = nameof(ErrorMethodology);
  public const string FirmwareRevision = nameof(FirmwareRevision);
  public const string Index = nameof(Index);
  public const string InstallDate = nameof(InstallDate);
  public const string InterfaceType = nameof(InterfaceType);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string MaxBlockSize = nameof(MaxBlockSize);
  public const string MediaLoaded = nameof(MediaLoaded);
  public const string MaxMediaSize = nameof(MaxMediaSize);
  public const string MediaType = nameof(MediaType);
  public const string MinBlockSize = nameof(MinBlockSize);
  public const string Model = nameof(Model);
  public const string Name = nameof(Name);
  public const string Partitions = nameof(Partitions);
  public const string SCSIBus = nameof(SCSIBus);
  public const string SCSILogicalUnit = nameof(SCSILogicalUnit);
  public const string SCSIPort = nameof(SCSIPort);
  public const string SCSITargetId = nameof(SCSITargetId);
  public const string SerialNumber = nameof(SerialNumber);
  public const string Signature = nameof(Signature);
  public const string Size = nameof(Size);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string NeedsCleaning = nameof(NeedsCleaning);
  public const string NumberOfMediaSupported = nameof(NumberOfMediaSupported);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string SectorsPerTrack = nameof(SectorsPerTrack);
  public const string TotalCylinders = nameof(TotalCylinders);
  public const string TotalHeads = nameof(TotalHeads);
  public const string TotalSectors = nameof(TotalSectors);
  public const string TotalTracks = nameof(TotalTracks);
  public const string TracksPerCylinder = nameof(TracksPerCylinder);
}