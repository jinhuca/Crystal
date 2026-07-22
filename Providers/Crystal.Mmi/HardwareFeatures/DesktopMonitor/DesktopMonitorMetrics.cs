namespace Crystal.Mmi.HardwareFeatures.DesktopMonitor;

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
