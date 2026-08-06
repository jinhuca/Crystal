using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.DiskPartition;

internal static class WmiDiskPartition {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.DiskPartition;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string DeviceID = CommonWmiProperties.DeviceId;

  // ---------------------------------------------------------------------
  // Disk Partition Specific Properties
  // ---------------------------------------------------------------------
  public const string Availability = nameof(Availability);
  public const string Bootable = nameof(Bootable);
  public const string BlockSize = nameof(BlockSize);
  public const string BootPartition = nameof(BootPartition);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string CreationClassName = nameof(CreationClassName);
  public const string DiskIndex = nameof(DiskIndex);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string ErrorMethodology = nameof(ErrorMethodology);
  public const string Index = nameof(Index);
  public const string InstallDate = nameof(InstallDate);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string Name = nameof(Name);
  public const string NumberOfBlocks = nameof(NumberOfBlocks);
  public const string PNPDeviceID = nameof(PNPDeviceID);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string PrimaryPartition = nameof(PrimaryPartition);
  public const string Purpose = nameof(Purpose);
  public const string RewritePartition = nameof(RewritePartition);
  public const string Size = nameof(Size);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string StartingOffset = nameof(StartingOffset);
  public const string TargetOperatingSystem = nameof(TargetOperatingSystem);
  public const string Type = nameof(Type);
}