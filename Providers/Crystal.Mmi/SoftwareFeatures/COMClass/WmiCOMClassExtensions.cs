using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.SoftwareFeatures.COMClass;

public static class WmiCOMClassExtensions {
  private const string WmiClassName = WmiCOMClass.ClassName;

  public static async Task<IReadOnlyList<COMClassMetrics>> ToSafeCOMClassMetricsAsync(
      this IWmiHardwareProvider provider,
      CancellationToken cancellationToken) {
    try {
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) return Array.Empty<COMClassMetrics>();

      var results = new List<COMClassMetrics>(instancesData.Count);

      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String ? v.AsString() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime ? v.AsDateTime() : null;

        results.Add(new COMClassMetrics(
            Caption: GetStr(WmiCOMClass.Caption),
            Description: GetStr(WmiCOMClass.Description),
            InstallDate: GetDate(WmiCOMClass.InstallDate),
            Name: GetStr(WmiCOMClass.Name),
            Status: GetStr(WmiCOMClass.Status)
        ));
      }

      return results;
    }
    catch {
      return Array.Empty<COMClassMetrics>();
    }
  }
}
