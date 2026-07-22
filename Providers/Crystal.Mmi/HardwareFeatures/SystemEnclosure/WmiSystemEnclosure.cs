using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.SystemEnclosure;

internal static class WmiSystemEnclosure {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = "Win32_SystemEnclosure";

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // System Enclosure Specific Properties
  // ---------------------------------------------------------------------
  public const string AssetTag = nameof(AssetTag);
  public const string AudibleAlarm = nameof(AudibleAlarm);
  public const string BreachDescription = nameof(BreachDescription);
  public const string CableManagementStrategy = nameof(CableManagementStrategy);
  public const string ChassisTypes = nameof(ChassisTypes);
  public const string CreationClassName = nameof(CreationClassName);
  public const string HeatSinkPresent = nameof(HeatSinkPresent);
  public const string HotSwappable = nameof(HotSwappable);
  public const string InstallationDate = nameof(InstallationDate);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string LockPresent = nameof(LockPresent);
  public const string SecurityBreach = nameof(SecurityBreach);
  public const string SecurityStatus = nameof(SecurityStatus);
  public const string SerialNumber = nameof(SerialNumber);
  public const string SMBIOSAssetTag = nameof(SMBIOSAssetTag);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string Tag = nameof(Tag);
  public const string Version = nameof(Version);
  public const string VisibleAlarm = nameof(VisibleAlarm);
}