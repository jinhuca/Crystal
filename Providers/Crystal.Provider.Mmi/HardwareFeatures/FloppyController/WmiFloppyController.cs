using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.FloppyController;

/// <summary>
/// Contains the WMI class name and property names for the <c>Win32_FloppyController</c> WMI class.
/// </summary>
internal static class WmiFloppyController {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.FloppyController;

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
  public const string Manufacturer = CommonWmiProperties.Manufacturer;

  // ---------------------------------------------------------------------
  // Floppy Controller (CIM_Controller) Specific Properties
  // ---------------------------------------------------------------------
  public const string Availability = nameof(Availability);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string MaxNumberControlled = nameof(MaxNumberControlled);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string ProtocolSupported = nameof(ProtocolSupported);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string TimeOfLastReset = nameof(TimeOfLastReset);
}
