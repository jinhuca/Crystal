using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.DisplayControllerConfiguration;

/// <summary>
/// Contains the WMI class name and property names for the <c>Win32_DisplayControllerConfiguration</c> WMI class.
/// </summary>
internal static class WmiDisplayControllerConfiguration {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.DisplayControllerConfiguration;

  // ---------------------------------------------------------------------
  // Shared Properties (CIM_Setting)
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Name = CommonWmiProperties.Name;

  // ---------------------------------------------------------------------
  // Display Controller Configuration Specific Properties
  // ---------------------------------------------------------------------
  public const string BitsPerPixel = nameof(BitsPerPixel);
  public const string ColorPlanes = nameof(ColorPlanes);
  public const string DeviceEntriesInAColorTable = nameof(DeviceEntriesInAColorTable);
  public const string DeviceSpecificPens = nameof(DeviceSpecificPens);
  public const string HorizontalResolution = nameof(HorizontalResolution);
  public const string RefreshRate = nameof(RefreshRate);
  public const string ReservedSystemPaletteEntries = nameof(ReservedSystemPaletteEntries);
  public const string SettingID = nameof(SettingID);
  public const string SystemPaletteEntries = nameof(SystemPaletteEntries);
  public const string VerticalResolution = nameof(VerticalResolution);
  public const string VideoMode = nameof(VideoMode);
}
