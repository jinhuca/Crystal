namespace Crystal.Provider.Mmi.HardwareFeatures.DesktopMonitor;

/// <summary>
/// Represents the metrics of a desktop monitor, including availability, bandwidth, 
/// display type, screen dimensions, and other relevant properties.
/// </summary>
/// <param name="Availability">The availability of the desktop monitor.</param>
/// <param name="Bandwidth">The bandwidth of the desktop monitor.</param>
/// <param name="Caption">The caption of the desktop monitor.</param>
/// <param name="ConfigManagerErrorCode">The configuration manager error code.</param>
/// <param name="ConfigManagerUserConfig">Indicates if the configuration manager user config is set.</param>
/// <param name="CreationClassName">The creation class name.</param>
/// <param name="Description">The description of the desktop monitor.</param>
/// <param name="DeviceID">The device ID of the desktop monitor.</param>
/// <param name="DisplayType">The display type of the desktop monitor.</param>
/// <param name="ErrorCleared">Indicates if the error is cleared.</param>
/// <param name="ErrorDescription">The description of the error.</param>
/// <param name="InstallationDate">The installation date of the desktop monitor.</param>
/// <param name="IsLocked">Indicates if the desktop monitor is locked.</param>
/// <param name="LastErrorCode">The last error code.</param>
/// <param name="MonitorManufacturer">The manufacturer of the desktop monitor.</param>
/// <param name="MonitorType">The type of the desktop monitor.</param>
/// <param name="Name">The name of the desktop monitor.</param>
/// <param name="PixelsPerXLogicalInch">The number of pixels per logical inch on the X-axis.</param>
/// <param name="PixelsPerYLogicalInch">The number of pixels per logical inch on the Y-axis.</param>
/// <param name="PNPDeviceID">The PNP device ID.</param>
/// <param name="PowerManagementCapabilities">The power management capabilities.</param>
/// <param name="PowerManagementSupported">Indicates if power management is supported.</param>
/// <param name="ScreenHeight">The height of the screen in pixels.</param>
/// <param name="ScreenWidth">The width of the screen in pixels.</param>
/// <param name="Status">The status of the desktop monitor.</param>
/// <param name="StatusInfo">The status info of the desktop monitor.</param>
/// <param name="SystemCreationClassName">The system creation class name.</param>
/// <param name="SystemName">The system name.</param>
public record DesktopMonitorMetrics(
  ushort?   Availability,
  uint?     Bandwidth,               // MHz
  string?   Caption,
  uint?     ConfigManagerErrorCode,
  bool?     ConfigManagerUserConfig,
  string?   CreationClassName,
  string?   Description,
  string?   DeviceID,
  ushort?   DisplayType,
  bool?     ErrorCleared,
  string?   ErrorDescription,
  DateTime? InstallationDate,
  bool?     IsLocked,
  uint?     LastErrorCode,
  string?   MonitorManufacturer,
  string?   MonitorType,
  string?   Name,
  uint?     PixelsPerXLogicalInch,
  uint?     PixelsPerYLogicalInch,
  string?   PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool?     PowerManagementSupported,
  uint?     ScreenHeight,            // pixels
  uint?     ScreenWidth,             // pixels
  string?   Status,
  ushort?   StatusInfo,
  string?   SystemCreationClassName,
  string?   SystemName
)
{
  /// <summary>
  /// Human-readable description of <see cref="DisplayType"/>.
  /// </summary>
  public string? DisplayTypeName => DisplayType switch {
    0 => "Unknown",
    1 => "Other",
    2 => "Multiscan Color",
    3 => "Multiscan Monochrome",
    4 => "Fixed Frequency Color",
    5 => "Fixed Frequency Monochrome",
    _ => null
  };
}
