using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.PerformanceFeatures.PerfCounter;

public static class WmiPerfCounterExtensions {
  private const string WmiClassName = "Win32_Perf";

  public static async Task<IReadOnlyList<PerfCounterMetrics>> ToSafePerfCounterMetricsAsync(
      this IWmiHardwareProvider provider,
      CancellationToken cancellationToken) {
    try {
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) return Array.Empty<PerfCounterMetrics>();

      var results = new List<PerfCounterMetrics>(instancesData.Count);

      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        string? GetStr(string key)   => data.TryGetValue(key, out var v) && v.Type == WmiType.String ? v.AsString()  : null;
        ulong?  GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong  ? v.AsULong()   : null;

        results.Add(new PerfCounterMetrics(
            Caption:            GetStr("Caption"),
            Description:        GetStr("Description"),
            Name:               GetStr("Name"),
            Frequency_Object:   GetULong("Frequency_Object"),
            Frequency_PerfTime: GetULong("Frequency_PerfTime"),
            Frequency_Sys100NS: GetULong("Frequency_Sys100NS"),
            Timestamp_Object:   GetULong("Timestamp_Object"),
            Timestamp_PerfTime: GetULong("Timestamp_PerfTime"),
            Timestamp_Sys100NS: GetULong("Timestamp_Sys100NS")
        ));
      }

      return results;
    }
    catch {
      return Array.Empty<PerfCounterMetrics>();
    }
  }
}
