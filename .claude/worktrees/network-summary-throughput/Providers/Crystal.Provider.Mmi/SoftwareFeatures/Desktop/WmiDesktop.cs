using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.SoftwareFeatures.Desktop;

internal static class WmiDesktop {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.Desktop;

  // ---------------------------------------------------------------------
  // Shared Properties (CIM_Setting)
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Name = CommonWmiProperties.Name;

  // ---------------------------------------------------------------------
  // Desktop Specific Properties
  // ---------------------------------------------------------------------
  public const string BorderWidth = nameof(BorderWidth);
  public const string CoolSwitch = nameof(CoolSwitch);
  public const string CursorBlinkRate = nameof(CursorBlinkRate);
  public const string DragFullWindows = nameof(DragFullWindows);
  public const string GridGranularity = nameof(GridGranularity);
  public const string IconSpacing = nameof(IconSpacing);
  public const string IconTitleFaceName = nameof(IconTitleFaceName);
  public const string IconTitleSize = nameof(IconTitleSize);
  public const string IconTitleWrap = nameof(IconTitleWrap);
  public const string Pattern = nameof(Pattern);
  public const string ScreenSaverActive = nameof(ScreenSaverActive);
  public const string ScreenSaverExecutable = nameof(ScreenSaverExecutable);
  public const string ScreenSaverSecure = nameof(ScreenSaverSecure);
  public const string ScreenSaverTimeout = nameof(ScreenSaverTimeout);
  public const string SettingID = nameof(SettingID);
  public const string Wallpaper = nameof(Wallpaper);
  public const string WallpaperStretched = nameof(WallpaperStretched);
  public const string WallpaperTiled = nameof(WallpaperTiled);
}
