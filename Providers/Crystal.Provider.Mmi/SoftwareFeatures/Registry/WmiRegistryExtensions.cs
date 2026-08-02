using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.Registry;
public static class WmiRegistryExtensions {
  private const string WmiClassName = WmiRegistry.ClassName;

  public static async Task<IReadOnlyList<RegistryMetrics>> ToSafeRegistryMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance registry resource data blocks asynchronously
      //    (in practice Win32_Registry exposes a single instance for the local system)
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<RegistryMetrics>();
      }

      var results = new List<RegistryMetrics>(instancesData.Count);

      // 2. Loop through every detected instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime
          ? v.AsDateTime() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new RegistryMetrics(
          Caption: GetStr(WmiRegistry.Caption),
          CurrentSize: (uint?)GetInt(WmiRegistry.CurrentSize),
          Description: GetStr(WmiRegistry.Description),
          InstallDate: GetDate(WmiRegistry.InstallDate),
          MaximumSize: (uint?)GetInt(WmiRegistry.MaximumSize),
          Name: GetStr(WmiRegistry.Name),
          ProposedSize: (uint?)GetInt(WmiRegistry.ProposedSize),
          Status: GetStr(WmiRegistry.Status)));
      }
      return results;
    }
    catch {
      return Array.Empty<RegistryMetrics>();
    }
  }
}
