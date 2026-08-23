namespace Crystal.Provider.Mmi.HardwareFeatures.DisplayControllerConfiguration;

/// <summary>
/// Represents the configuration of a display controller, including its resolution, color depth, and refresh rate. 
/// This record is used to encapsulate the metrics obtained from the Win32_DisplayControllerConfiguration WMI class.
///  Win32_DisplayControllerConfiguration is derived from CIM_Setting (not CIM_LogicalDevice)
/// and is deprecated in favor of Win32_VideoController, Win32_DesktopMonitor, and
/// CIM_VideoControllerResolution — kept here for completeness/legacy compatibility.
/// </summary>
/// <param name="BitsPerPixel">The number of bits per pixel for the display controller.</param>
/// <param name="Caption">A short description of the display controller.</param>
/// <param name="ColorPlanes">The number of color planes for the display controller.</param>
/// <param name="Description">A detailed description of the display controller.</param>
/// <param name="DeviceEntriesInAColorTable">The number of entries in the color table.</param>
/// <param name="DeviceSpecificPens">The number of device-specific pens.</param>
/// <param name="HorizontalResolution">The horizontal resolution of the display controller.</param>
/// <param name="Name">The name of the display controller.</param>
/// <param name="RefreshRate">The refresh rate of the display controller.</param>
/// <param name="ReservedSystemPaletteEntries">The number of reserved system palette entries.</param>
/// <param name="SettingID">The setting ID of the display controller.</param>
/// <param name="SystemPaletteEntries">The number of system palette entries.</param>
/// <param name="VerticalResolution">The vertical resolution of the display controller.</param>
/// <param name="VideoMode">The video mode of the display controller.</param>
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
