namespace Crystal.Provider.Mmi.HardwareFeatures.DisplayControllerConfiguration;

// Win32_DisplayControllerConfiguration is derived from CIM_Setting (not CIM_LogicalDevice)
// and is deprecated in favor of Win32_VideoController, Win32_DesktopMonitor, and
// CIM_VideoControllerResolution — kept here for completeness/legacy compatibility.
public record DisplayControllerConfigurationMetrics(
  uint? BitsPerPixel,
  string? Caption,
  uint? ColorPlanes,
  string? Description,
  uint? DeviceEntriesInAColorTable,
  uint? DeviceSpecificPens,
  uint? HorizontalResolution,
  string? Name,
  int? RefreshRate,          // 0 or 1 = default rate, -1 = optimal rate
  uint? ReservedSystemPaletteEntries,
  string? SettingID,
  uint? SystemPaletteEntries,
  uint? VerticalResolution,
  string? VideoMode
);
