namespace Crystal.Mmi.SoftwareFeatures.Desktop;

public record DesktopMetrics(
  uint? BorderWidth,
  string? Caption,
  bool? CoolSwitch,
  uint? CursorBlinkRate,
  string? Description,
  bool? DragFullWindows,
  uint? GridGranularity,
  uint? IconSpacing,
  string? IconTitleFaceName,
  uint? IconTitleSize,
  bool? IconTitleWrap,
  string? Name,
  string? Pattern,
  bool? ScreenSaverActive,
  string? ScreenSaverExecutable,
  bool? ScreenSaverSecure,
  uint? ScreenSaverTimeout,
  string? SettingID,
  string? Wallpaper,
  bool? WallpaperStretched,
  bool? WallpaperTiled
);
