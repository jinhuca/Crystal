namespace Crystal.Provider.Mmi.HardwareFeatures.DeviceSettings;

// Win32_DeviceSettings is a WMI association class (CIM_ElementSetting) — it has no
// scalar telemetry of its own. It relates a logical device (Element) to a configuration
// object that can be applied to it (Setting), e.g. Win32_SerialPortSetting relates a
// Win32_SerialPort to a Win32_SerialPortConfiguration. Both reference properties come
// back from WMI as embedded object-path strings, e.g.:
//   Element: Win32_SerialPort.DeviceID="COM1"
//   Setting: Win32_SerialPortConfiguration.SettingID="COM1"
public record DeviceSettingsMetrics(
  string? Element,  // CIM_LogicalDevice REF — the device the setting applies to
  string? Setting    // CIM_Setting REF — the configuration/setting object
) {
  // --- RUNTIME PRESENTATION HELPERS ---

  // Extracts the bare key value out of the embedded WMI object-path reference.
  public string? ElementDeviceId => ExtractKey(Element, "DeviceID=\"");
  public string? SettingId => ExtractKey(Setting, "SettingID=\"");

  private static string? ExtractKey(string? path, string marker) =>
    string.IsNullOrEmpty(path) ? null : path.Split(marker).LastOrDefault()?.TrimEnd('"');
}
