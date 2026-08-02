using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.TimeZone;
public static class WmiTimeZoneExtensions {
  private const string WmiClassName = WmiTimeZone.ClassName;

  public static async Task<IReadOnlyList<TimeZoneMetrics>> ToSafeTimeZoneMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance time zone data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<TimeZoneMetrics>();
      }

      var results = new List<TimeZoneMetrics>(instancesData.Count);

      // 2. Loop through every detected time zone instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new TimeZoneMetrics(
          Bias: GetInt(WmiTimeZone.Bias),
          Caption: GetStr(WmiTimeZone.Caption),
          DaylightBias: GetInt(WmiTimeZone.DaylightBias),
          DaylightDay: (uint?)GetInt(WmiTimeZone.DaylightDay),
          DaylightDayOfWeek: (byte?)GetInt(WmiTimeZone.DaylightDayOfWeek),
          DaylightHour: (uint?)GetInt(WmiTimeZone.DaylightHour),
          DaylightMillisecond: (uint?)GetInt(WmiTimeZone.DaylightMillisecond),
          DaylightMinute: (uint?)GetInt(WmiTimeZone.DaylightMinute),
          DaylightMonth: (uint?)GetInt(WmiTimeZone.DaylightMonth),
          DaylightName: GetStr(WmiTimeZone.DaylightName),
          DaylightSecond: (uint?)GetInt(WmiTimeZone.DaylightSecond),
          DaylightYear: (uint?)GetInt(WmiTimeZone.DaylightYear),
          Description: GetStr(WmiTimeZone.Description),
          SettingID: GetStr(WmiTimeZone.SettingID),
          StandardBias: (uint?)GetInt(WmiTimeZone.StandardBias),
          StandardDay: (uint?)GetInt(WmiTimeZone.StandardDay),
          StandardDayOfWeek: (byte?)GetInt(WmiTimeZone.StandardDayOfWeek),
          StandardHour: (uint?)GetInt(WmiTimeZone.StandardHour),
          StandardMillisecond: (uint?)GetInt(WmiTimeZone.StandardMillisecond),
          StandardMinute: (uint?)GetInt(WmiTimeZone.StandardMinute),
          StandardMonth: (uint?)GetInt(WmiTimeZone.StandardMonth),
          StandardName: GetStr(WmiTimeZone.StandardName),
          StandardSecond: (uint?)GetInt(WmiTimeZone.StandardSecond),
          StandardYear: (uint?)GetInt(WmiTimeZone.StandardYear)));
      }
      return results;
    }
    catch {
      return Array.Empty<TimeZoneMetrics>();
    }
  }
}
