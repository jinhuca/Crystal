using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.MappedLogicalDisk;

internal static class WmiMappedLogicalDisk {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.MappedLogicalDisk;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string Name = CommonWmiProperties.Name;
  public const string DeviceID = CommonWmiProperties.DeviceId;
  public const string PNPDeviceID = CommonWmiProperties.PnpDeviceId;
  public const string CreationClassName = CommonWmiProperties.CreationClassName;
  public const string InstallDate = CommonWmiProperties.InstallDate;

  // ---------------------------------------------------------------------
  // Mapped Logical Disk (CIM_LogicalDisk) Specific Properties
  // ---------------------------------------------------------------------
  public const string Access = nameof(Access);
  public const string Availability = nameof(Availability);
  public const string BlockSize = nameof(BlockSize);
  public const string Compressed = nameof(Compressed);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string ErrorMethodology = nameof(ErrorMethodology);
  public const string FileSystem = nameof(FileSystem);
  public const string FreeSpace = nameof(FreeSpace);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string MaximumComponentLength = nameof(MaximumComponentLength);
  public const string NumberOfBlocks = nameof(NumberOfBlocks);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string ProviderName = nameof(ProviderName);
  public const string Purpose = nameof(Purpose);
  public const string QuotasDisabled = nameof(QuotasDisabled);
  public const string QuotasIncomplete = nameof(QuotasIncomplete);
  public const string QuotasRebuilding = nameof(QuotasRebuilding);
  public const string SessionID = nameof(SessionID);
  public const string Size = nameof(Size);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SupportsDiskQuotas = nameof(SupportsDiskQuotas);
  public const string SupportsFileBasedCompression = nameof(SupportsFileBasedCompression);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string VolumeName = nameof(VolumeName);
  public const string VolumeSerialNumber = nameof(VolumeSerialNumber);
}
