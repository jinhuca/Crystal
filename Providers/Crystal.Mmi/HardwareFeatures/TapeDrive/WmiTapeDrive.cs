using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.TapeDrive;

internal static class WmiTapeDrive {
  public const string ClassName = WmiClasses.TapeDrive;

  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;
  public const string DeviceID = CommonWmiProperties.DeviceId;
  public const string PNPDeviceID = CommonWmiProperties.PnpDeviceId;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string CreationClassName = CommonWmiProperties.CreationClassName;
  public const string InstallDate = CommonWmiProperties.InstallDate;

  public const string Availability = nameof(Availability);
  public const string Capabilities = nameof(Capabilities);
  public const string CapabilityDescriptions = nameof(CapabilityDescriptions);
  public const string Compression = nameof(Compression);
  public const string CompressionMethod = nameof(CompressionMethod);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string DefaultBlockSize = nameof(DefaultBlockSize);
  public const string ECC = nameof(ECC);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string ErrorMethodology = nameof(ErrorMethodology);
  public const string FeaturesHigh = nameof(FeaturesHigh);
  public const string FeaturesLow = nameof(FeaturesLow);
  public const string Id = nameof(Id);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string MaxBlockSize = nameof(MaxBlockSize);
  public const string MaxMediaSize = nameof(MaxMediaSize);
  public const string MaxPartitionCount = nameof(MaxPartitionCount);
  public const string MediaType = nameof(MediaType);
  public const string MinBlockSize = nameof(MinBlockSize);
  public const string NeedsCleaning = nameof(NeedsCleaning);
  public const string NumberOfMediaSupported = nameof(NumberOfMediaSupported);
  public const string Padding = nameof(Padding);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string ReportSetMarks = nameof(ReportSetMarks);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
}
