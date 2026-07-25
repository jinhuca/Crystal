using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.OnBoardDevice;
public static class WmiOnBoardDeviceExtensions {
  private const string WmiClassName = WmiOnBoardDevice.ClassName;

  public static async Task<IReadOnlyList<OnBoardDeviceMetrics>> ToSafeOnBoardDeviceMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance onboard device data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<OnBoardDeviceMetrics>();
      }

      var results = new List<OnBoardDeviceMetrics>(instancesData.Count);

      // 2. Loop through every detected onboard device instance sequentially
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

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new OnBoardDeviceMetrics(
          Caption: GetStr(WmiOnBoardDevice.Caption),
          CreationClassName: GetStr(WmiOnBoardDevice.CreationClassName),
          Description: GetStr(WmiOnBoardDevice.Description),
          DeviceType: (ushort?)GetInt(WmiOnBoardDevice.DeviceType),
          Enabled: GetBool(WmiOnBoardDevice.Enabled),
          HotSwappable: GetBool(WmiOnBoardDevice.HotSwappable),
          InstallDate: GetDate(WmiOnBoardDevice.InstallDate),
          Manufacturer: GetStr(WmiOnBoardDevice.Manufacturer),
          Model: GetStr(WmiOnBoardDevice.Model),
          Name: GetStr(WmiOnBoardDevice.Name),
          OtherIdentifyingInfo: GetStr(WmiOnBoardDevice.OtherIdentifyingInfo),
          PartNumber: GetStr(WmiOnBoardDevice.PartNumber),
          PoweredOn: GetBool(WmiOnBoardDevice.PoweredOn),
          Removable: GetBool(WmiOnBoardDevice.Removable),
          Replaceable: GetBool(WmiOnBoardDevice.Replaceable),
          SerialNumber: GetStr(WmiOnBoardDevice.SerialNumber),
          SKU: GetStr(WmiOnBoardDevice.SKU),
          Status: GetStr(WmiOnBoardDevice.Status),
          Tag: GetStr(WmiOnBoardDevice.Tag),
          Version: GetStr(WmiOnBoardDevice.Version)));
      }
      return results;
    }
    catch {
      return Array.Empty<OnBoardDeviceMetrics>();
    }
  }
}
