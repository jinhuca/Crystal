using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.Fan;

/// <summary>
/// Contains the WMI class name and property names for the Win32_Fan WMI class.
/// </summary>
internal static class WmiFan {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = "Win32_Fan";

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
  // Fan Specific Properties
  // ---------------------------------------------------------------------
  public const string ActiveCooling = nameof(ActiveCooling);
  public const string Availability = nameof(Availability);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string DesiredSpeed = nameof(DesiredSpeed);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string VariableSpeed = nameof(VariableSpeed);
}
