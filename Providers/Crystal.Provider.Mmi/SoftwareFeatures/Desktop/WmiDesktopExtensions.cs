using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.Desktop;
public static class WmiDesktopExtensions {
  private const string WmiClassName = WmiDesktop.ClassName;

  public static async Task<IReadOnlyList<DesktopMetrics>> ToSafeDesktopMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance desktop profile data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<DesktopMetrics>();
      }

      var results = new List<DesktopMetrics>(instancesData.Count);

      // 2. Loop through every detected desktop profile instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;
        bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool
          ? v.AsBool() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new DesktopMetrics(
          BorderWidth: (uint?)GetInt(WmiDesktop.BorderWidth),
          Caption: GetStr(WmiDesktop.Caption),
          CoolSwitch: GetBool(WmiDesktop.CoolSwitch),
          CursorBlinkRate: (uint?)GetInt(WmiDesktop.CursorBlinkRate),
          Description: GetStr(WmiDesktop.Description),
          DragFullWindows: GetBool(WmiDesktop.DragFullWindows),
          GridGranularity: (uint?)GetInt(WmiDesktop.GridGranularity),
          IconSpacing: (uint?)GetInt(WmiDesktop.IconSpacing),
          IconTitleFaceName: GetStr(WmiDesktop.IconTitleFaceName),
          IconTitleSize: (uint?)GetInt(WmiDesktop.IconTitleSize),
          IconTitleWrap: GetBool(WmiDesktop.IconTitleWrap),
          Name: GetStr(WmiDesktop.Name),
          Pattern: GetStr(WmiDesktop.Pattern),
          ScreenSaverActive: GetBool(WmiDesktop.ScreenSaverActive),
          ScreenSaverExecutable: GetStr(WmiDesktop.ScreenSaverExecutable),
          ScreenSaverSecure: GetBool(WmiDesktop.ScreenSaverSecure),
          ScreenSaverTimeout: (uint?)GetInt(WmiDesktop.ScreenSaverTimeout),
          SettingID: GetStr(WmiDesktop.SettingID),
          Wallpaper: GetStr(WmiDesktop.Wallpaper),
          WallpaperStretched: GetBool(WmiDesktop.WallpaperStretched),
          WallpaperTiled: GetBool(WmiDesktop.WallpaperTiled)));
      }
      return results;
    }
    catch {
      return Array.Empty<DesktopMetrics>();
    }
  }
}
