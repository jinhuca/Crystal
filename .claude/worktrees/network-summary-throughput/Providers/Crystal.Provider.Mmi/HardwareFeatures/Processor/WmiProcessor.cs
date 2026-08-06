using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.Processor;

internal static class WmiProcessor {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.Processor;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Name = CommonWmiProperties.Name;
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string Status = CommonWmiProperties.Status;
  public const string DeviceId = CommonWmiProperties.DeviceId;
  public const string PnpDeviceId = CommonWmiProperties.PnpDeviceId;

  // ---------------------------------------------------------------------
  // Processor Specific Properties
  // ---------------------------------------------------------------------
  public const string AddressWidth = nameof(AddressWidth);
  public const string Architecture = nameof(Architecture);
  public const string AssetTag = nameof(AssetTag);
  public const string Availability = nameof(Availability);
  public const string Characteristics = nameof(Characteristics);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string CpuStatus = nameof(CpuStatus);
  public const string CreationClassName = nameof(CreationClassName);
  public const string CurrentClockSpeed = nameof(CurrentClockSpeed);
  public const string CurrentVoltage = nameof(CurrentVoltage);
  public const string DataWidth = nameof(DataWidth);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string ExtClock = nameof(ExtClock);
  public const string Family = nameof(Family);
  public const string InstallationDate = nameof(InstallationDate);
  public const string L2CacheSize = nameof(L2CacheSize);
  public const string L2CacheSpeed = nameof(L2CacheSpeed);
  public const string L3CacheSize = nameof(L3CacheSize);
  public const string L3CacheSpeed = nameof(L3CacheSpeed);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string Level = nameof(Level);
  public const string LoadPercentage = nameof(LoadPercentage);
  public const string MaxClockSpeed = nameof(MaxClockSpeed);
  public const string NumberOfCores = nameof(NumberOfCores);
  public const string NumberOfEnabledCore = nameof(NumberOfEnabledCore);
  public const string NumberOfLogicalProcessors = nameof(NumberOfLogicalProcessors);
  public const string OtherFamilyDescription = nameof(OtherFamilyDescription);
  public const string PartNumber = nameof(PartNumber);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string ProcessorId = nameof(ProcessorId);
  public const string ProcessorType = nameof(ProcessorType);
  public const string Revision = nameof(Revision);
  public const string Role = nameof(Role);
  public const string SecondLevelAddressTranslationExtensions = nameof(SecondLevelAddressTranslationExtensions);
  public const string SerialNumber = nameof(SerialNumber);
  public const string SocketDesignation = nameof(SocketDesignation);
  public const string StatusInfo = nameof(StatusInfo);
  public const string Stepping = nameof(Stepping);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string ThreadCount = nameof(ThreadCount);
  public const string UniqueId = nameof(UniqueId);
  public const string UpgradeMethod = nameof(UpgradeMethod);
  public const string Version = nameof(Version);
  public const string VirtualizationFirmwareEnabled = nameof(VirtualizationFirmwareEnabled);
  public const string VMMonitorModeExtensions = nameof(VMMonitorModeExtensions);
  public const string VoltageCaps = nameof(VoltageCaps);
}