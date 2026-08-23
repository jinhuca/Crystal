using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.Bios;

/// <summary>
/// Provides extension methods for <see cref="IWmiHardwareProvider"/> to safely retrieve BIOS metrics from WMI (<c>Win32_BIOS</c>), 
/// handling nulls and exceptions gracefully.
/// </summary>
public static class WmiBiosExtensions {
  public static async Task<BiosMetrics> ToSafeBiosMetricsAsync(this IWmiHardwareProvider provider, CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance data collection asynchronously
      var instances = await provider.GetMultiMetricsForClassAsync(WmiBios.ClassName, cancellationToken);
      var data = instances.FirstOrDefault();

      // --- FULL NULL/CRASH FALLBACK RETRIEVAL ---
      if (data == null || data.Count == 0) {
        return new BiosMetrics(
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null
        );
      }

      cancellationToken.ThrowIfCancellationRequested();

      // --- CLEAN LOOKUP CONDITIONAL WRAPPERS ---
      string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String ? v.AsString() : null;
      int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int ? v.AsInt() : null;
      bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool ? v.AsBool() : null;
      DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime ? v.AsDateTime() : null;

      // Handles array types cleanly
      ushort? GetFirstCharacteristic(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray ? v.AsUShortArray()?.FirstOrDefault() : null;
      string? FlattenStrArray(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.StringArray ? string.Join(", ", v.AsStringArray() ?? Array.Empty<string>()) : null;

      // --- INSTANTIATE SORTED EXTRACTED VALUES ---
      return new BiosMetrics(
          BiosCharacteristics: GetFirstCharacteristic("BiosCharacteristics"),
          BIOSVersion: FlattenStrArray("BIOSVersion"),
          BuildNumber: GetStr("BuildNumber"),
          Caption: GetStr("Caption"),
          CodeSet: GetStr("CodeSet"),
          CurrentLanguage: GetStr("CurrentLanguage"),
          Description: GetStr("Description"),
          EmbeddedControllerMajorVersion: GetStr("EmbeddedControllerMajorVersion"),
          EmbeddedControllerMinorVersion: GetStr("EmbeddedControllerMinorVersion"),
          IdentificationCode: GetStr("IdentificationCode"),
          InstallableLanguages: (ushort?)GetInt("InstallableLanguages"),
          InstallDate: GetDate("InstallationDate"),
          LanguageEdition: GetStr("LanguageEdition"),
          ListOfLanguages: FlattenStrArray("ListOfLanguages"),
          Manufacturer: GetStr("Manufacturer"),
          Name: GetStr("Name"),
          OtherTargetOS: GetStr("OtherTargetOS"),
          PartNumber: GetStr("PartNumber"),
          PrimaryBIOS: GetBool("PrimaryBIOS"),
          ReleaseDate: GetStr("ReleaseDate"),
          SerialNumber: GetStr("SerialNumber"),
          SMBIOSBIOSVersion: GetStr("SMBIOSBIOSVersion"),
          SMBIOSPresent: GetBool("SMBIOSPresent"),
          SMBIOSMajorVersion: (ushort?)GetInt("SMBIOSMajorVersion"),
          SMBIOSMinorVersion: (ushort?)GetInt("SMBIOSMinorVersion"),
          Status: GetStr("Status"),
          SystemBiosMajorVersion: GetStr("SystemBiosMajorVersion"),
          SystemBiosMinorVersion: GetStr("SystemBiosMinorVersion"),
          TargetOperatingSystem: (ushort?)GetInt("TargetOperatingSystem"),
          Version: GetStr("Version")
      );
    }
    catch {
      return new BiosMetrics(
          null, null, null, null, null, null, null, null, null, null,
          null, null, null, null, null, null, null, null, null, null,
          null, null, null, null, null, null, null, null, null, null
      );
    }
  }

}
