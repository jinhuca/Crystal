using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.DesktopMonitor;

internal static class WmiDesktopMonitor {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = "Win32_DesktopMonitor";

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Name = CommonWmiProperties.Name;
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string DeviceID = CommonWmiProperties.DeviceId;

  // ---------------------------------------------------------------------
  // Desktop Monitor Specific Properties
  // ---------------------------------------------------------------------
  public const string Availability = nameof(Availability);
  public const string Bandwidth = nameof(Bandwidth);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string CreationClassName = nameof(CreationClassName);
  public const string DisplayType = nameof(DisplayType);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string InstallationDate = nameof(InstallationDate);
  public const string IsLocked = nameof(IsLocked);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string MonitorManufacturer = nameof(MonitorManufacturer);
  public const string MonitorType = nameof(MonitorType);
  public const string PixelsPerXLogicalInch = nameof(PixelsPerXLogicalInch);
  public const string PixelsPerYLogicalInch = nameof(PixelsPerYLogicalInch);
  public const string PNPDeviceID = nameof(PNPDeviceID);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string ScreenHeight = nameof(ScreenHeight);
  public const string ScreenWidth = nameof(ScreenWidth);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
}