using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.PnPEntity;

internal static class WmiPnPEntity {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = "Win32_PnPEntity";

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string Status = CommonWmiProperties.Status;
  public const string Name = CommonWmiProperties.Name;
  public const string DeviceID = CommonWmiProperties.DeviceId;
  public const string PNPDeviceID = CommonWmiProperties.PnpDeviceId;

  // ---------------------------------------------------------------------
  // USB Controller Specific Properties
  // ---------------------------------------------------------------------
  public const string Availability = nameof(Availability);
  public const string ClassGuid = nameof(ClassGuid);
  public const string CompatibleID = nameof(CompatibleID);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string CreationClassName = nameof(CreationClassName);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string HardwareID = nameof(HardwareID);
  public const string InstallationDate = nameof(InstallationDate);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string PNPClass = nameof(PNPClass);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string Present = nameof(Present);
  public const string Service = nameof(Service);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
}