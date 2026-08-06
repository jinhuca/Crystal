namespace Crystal.Provider.Mmi.HardwareFeatures.VideoSettings;

// Win32_VideoSettings is a WMI association class (CIM_VideoSetting) — it has no scalar
// telemetry of its own. It relates a video controller (Element) to a supported
// resolution/configuration object that can be applied to it (Setting). Both reference
// properties come back from WMI as embedded object-path strings, e.g.:
//   Element: Win32_VideoController.DeviceID="VideoController1"
//   Setting: CIM_VideoControllerResolution.SettingID="1920 x 1080 x 32 colors"
public record VideoSettingsMetrics(
  string? Element,  // Win32_VideoController REF — the video controller
  string? Setting    // CIM_VideoControllerResolution REF — a supported resolution/setting
) {
  // --- RUNTIME PRESENTATION HELPERS ---

  // Extracts the bare key value out of the embedded WMI object-path reference.
  public string? VideoControllerDeviceId => ExtractKey(Element, "DeviceID=\"");
  public string? SettingId => ExtractKey(Setting, "SettingID=\"");

  private static string? ExtractKey(string? path, string marker) =>
    string.IsNullOrEmpty(path) ? null : path.Split(marker).LastOrDefault()?.TrimEnd('"');
}
