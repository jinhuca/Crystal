using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.Tpm;

public static class WmiTpmExtensions {
  public static async Task<TpmMetrics> ToSafeTpmMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance data collection asynchronously (namespace-aware overload)
      var instances = await provider.GetMultiMetricsForClassAsync(WmiTpm.Namespace, WmiTpm.ClassName, cancellationToken);
      var data = instances.FirstOrDefault();

      // --- FULL NULL/CRASH FALLBACK RETRIEVAL ---
      if (data == null || data.Count == 0) {
        return new TpmMetrics(
          null, null, null, null, null, null, null,
          null, null, null, null, null, null, null
        );
      }

      cancellationToken.ThrowIfCancellationRequested();

      // --- CLEAN LOOKUP CONDITIONAL WRAPPERS ---
      string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
        ? v.AsString() : null;
      int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
        ? v.AsInt() : null;
      bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool
        ? v.AsBool() : null;

      // --- INSTANTIATE SORTED EXTRACTED VALUES ---
      return new TpmMetrics(
        Caption: GetStr(WmiTpm.Caption),
        Description: GetStr(WmiTpm.Description),
        InstanceName: GetStr(WmiTpm.InstanceName),
        IsActivated_InitialValue: GetBool(WmiTpm.IsActivated_InitialValue),
        IsEnabled_InitialValue: GetBool(WmiTpm.IsEnabled_InitialValue),
        IsOwned_InitialValue: GetBool(WmiTpm.IsOwned_InitialValue),
        ManufacturerId: (uint?)GetInt(WmiTpm.ManufacturerId),
        ManufacturerIdTxt: GetStr(WmiTpm.ManufacturerIdTxt),
        ManufacturerVersion: GetStr(WmiTpm.ManufacturerVersion),
        ManufacturerVersionFull20: GetStr(WmiTpm.ManufacturerVersionFull20),
        ManufacturerVersionInfo: GetStr(WmiTpm.ManufacturerVersionInfo),
        PhysicalPresenceVersionInfo: GetStr(WmiTpm.PhysicalPresenceVersionInfo),
        SpecVersion: GetStr(WmiTpm.SpecVersion),
        Status: GetStr(WmiTpm.Status));
    }
    catch {
      return new TpmMetrics(
        null, null, null, null, null, null, null,
        null, null, null, null, null, null, null
      );
    }
  }
}
