namespace Crystal.Mmi.SoftwareFeatures.UserDesktop;

// Win32_UserDesktop is a WMI association class (CIM_ElementSetting) relating a user account
// (Element) to the desktop settings that customize it (Setting). Both reference properties
// come back from WMI as embedded object-path strings, e.g.:
//   Element: Win32_UserAccount.Domain="SOMEDOMAIN",Name="johndoe"
//   Setting: Win32_Desktop.Name="SOMEDOMAIN\\johndoe"
public record UserDesktopMetrics(
  string? Element,  // Win32_UserAccount REF — the user account
  string? Setting    // Win32_Desktop REF — the desktop settings for that account
) {
  // --- RUNTIME PRESENTATION HELPERS ---

  // Extracts the bare key value out of the embedded WMI object-path reference.
  public string? UserAccountName => ExtractKey(Element, "Name=\"");
  public string? DesktopSettingName => ExtractKey(Setting, "Name=\"");

  private static string? ExtractKey(string? path, string marker) =>
    string.IsNullOrEmpty(path) ? null : path.Split(marker).LastOrDefault()?.TrimEnd('"');
}
