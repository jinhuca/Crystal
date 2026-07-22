using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.SoundDevice;

internal static class WmiSoundDevice {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = "Win32_SoundDevice";

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Name = CommonWmiProperties.Name;
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string Status = CommonWmiProperties.Status;
  public const string DeviceID = CommonWmiProperties.DeviceId;
  public const string PNPDeviceID = CommonWmiProperties.PnpDeviceId;

  // ---------------------------------------------------------------------
  // Sound Device Specific Properties
  // ---------------------------------------------------------------------
  public const string Availability = nameof(Availability);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string CreationClassName = nameof(CreationClassName);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string InstallationDate = nameof(InstallationDate);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string ProductName = nameof(ProductName);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string DMABufferSize = nameof(DMABufferSize);
  public const string MPU401Address = nameof(MPU401Address);
  public const string ProductID = nameof(ProductID);
}
