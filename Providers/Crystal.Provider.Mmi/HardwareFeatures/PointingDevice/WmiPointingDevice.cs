using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.PointingDevice;

internal static class WmiPointingDevice {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.PointingDevice;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string Name = CommonWmiProperties.Name;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string DeviceID = CommonWmiProperties.DeviceId;
  public const string PNPDeviceID = CommonWmiProperties.PnpDeviceId;
  public const string CreationClassName = CommonWmiProperties.CreationClassName;
  public const string InstallDate = CommonWmiProperties.InstallDate;

  // ---------------------------------------------------------------------
  // Pointing Device Specific Properties
  // ---------------------------------------------------------------------
  public const string Availability = nameof(Availability);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string DeviceInterface = nameof(DeviceInterface);
  public const string DoubleSpeedThreshold = nameof(DoubleSpeedThreshold);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string Handedness = nameof(Handedness);
  public const string HardwareType = nameof(HardwareType);
  public const string InfFileName = nameof(InfFileName);
  public const string InfSection = nameof(InfSection);
  public const string IsLocked = nameof(IsLocked);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string NumberOfButtons = nameof(NumberOfButtons);
  public const string PointingType = nameof(PointingType);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string QuadSpeedThreshold = nameof(QuadSpeedThreshold);
  public const string Resolution = nameof(Resolution);
  public const string SampleRate = nameof(SampleRate);
  public const string StatusInfo = nameof(StatusInfo);
  public const string Synch = nameof(Synch);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
}
