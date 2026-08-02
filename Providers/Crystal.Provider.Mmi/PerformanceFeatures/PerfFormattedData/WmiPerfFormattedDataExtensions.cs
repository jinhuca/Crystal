using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.PerformanceFeatures.PerfFormattedData;

public static class WmiPerfFormattedDataExtensions {
  private const string WmiClassName = "Win32_PerfFormattedData";

  public static async Task<IReadOnlyList<PerfFormattedDataMetrics>> ToSafePerfFormattedDataMetricsAsync(
      this IWmiHardwareProvider provider,
      CancellationToken cancellationToken) {
    try {
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) return Array.Empty<PerfFormattedDataMetrics>();

      var results = new List<PerfFormattedDataMetrics>(instancesData.Count);

      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String ? v.AsString() : null;
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong ? v.AsULong() : null;

        results.Add(new PerfFormattedDataMetrics(
            Caption: GetStr("Caption"),
            Description: GetStr("Description"),
            Name: GetStr("Name"),
            Frequency_Object: GetULong(WmiPerfFormattedData.Frequency_Object),
            Frequency_PerfTime: GetULong(WmiPerfFormattedData.Frequency_PerfTime),
            Frequency_Sys100NS: GetULong(WmiPerfFormattedData.Frequency_Sys100NS),
            Timestamp_Object: GetULong(WmiPerfFormattedData.Timestamp_Object),
            Timestamp_PerfTime: GetULong(WmiPerfFormattedData.Timestamp_PerfTime),
            Timestamp_Sys100NS: GetULong(WmiPerfFormattedData.Timestamp_Sys100NS)
        ));
      }

      return results;
    }
    catch {
      return Array.Empty<PerfFormattedDataMetrics>();
    }
  }
}
