using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.PhysicalMedia;
public static class WmiPhysicalMediaExtensions {
  private const string WmiClassName = WmiPhysicalMedia.ClassName;

  public static async Task<IReadOnlyList<PhysicalMediaMetrics>> ToSafePhysicalMediaMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance physical media data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<PhysicalMediaMetrics>();
      }

      var results = new List<PhysicalMediaMetrics>(instancesData.Count);

      // 2. Loop through every detected physical media instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;
        bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool
          ? v.AsBool() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime
          ? v.AsDateTime() : null;
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong
          ? v.AsReadOnlyULong() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new PhysicalMediaMetrics(
          Capacity: GetULong(WmiPhysicalMedia.Capacity),
          Caption: GetStr(WmiPhysicalMedia.Caption),
          CleanerMedia: GetBool(WmiPhysicalMedia.CleanerMedia),
          CreationClassName: GetStr(WmiPhysicalMedia.CreationClassName),
          Description: GetStr(WmiPhysicalMedia.Description),
          HotSwappable: GetBool(WmiPhysicalMedia.HotSwappable),
          InstallDate: GetDate(WmiPhysicalMedia.InstallDate),
          Manufacturer: GetStr(WmiPhysicalMedia.Manufacturer),
          MediaDescription: GetStr(WmiPhysicalMedia.MediaDescription),
          MediaType: (ushort?)GetInt(WmiPhysicalMedia.MediaType),
          Model: GetStr(WmiPhysicalMedia.Model),
          Name: GetStr(WmiPhysicalMedia.Name),
          OtherIdentifyingInfo: GetStr(WmiPhysicalMedia.OtherIdentifyingInfo),
          PartNumber: GetStr(WmiPhysicalMedia.PartNumber),
          PoweredOn: GetBool(WmiPhysicalMedia.PoweredOn),
          Removable: GetBool(WmiPhysicalMedia.Removable),
          Replaceable: GetBool(WmiPhysicalMedia.Replaceable),
          SerialNumber: GetStr(WmiPhysicalMedia.SerialNumber),
          SKU: GetStr(WmiPhysicalMedia.SKU),
          Status: GetStr(WmiPhysicalMedia.Status),
          Tag: GetStr(WmiPhysicalMedia.Tag),
          Version: GetStr(WmiPhysicalMedia.Version),
          WriteProtectOn: GetBool(WmiPhysicalMedia.WriteProtectOn)));
      }
      return results;
    }
    catch {
      return Array.Empty<PhysicalMediaMetrics>();
    }
  }
}
