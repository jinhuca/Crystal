using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.SCSIControllerDevice;
public static class WmiSCSIControllerDeviceExtensions {
  private const string WmiClassName = WmiSCSIControllerDevice.ClassName;

  public static async Task<IReadOnlyList<SCSIControllerDeviceMetrics>> ToSafeSCSIControllerDeviceMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance SCSI controller/device relationship data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<SCSIControllerDeviceMetrics>();
      }

      var results = new List<SCSIControllerDeviceMetrics>(instancesData.Count);

      // 2. Loop through every detected relationship instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong
          ? v.AsReadOnlyULong() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new SCSIControllerDeviceMetrics(
          AccessState: (ushort?)GetInt(WmiSCSIControllerDevice.AccessState),
          Antecedent: GetStr(WmiSCSIControllerDevice.Antecedent),
          Dependent: GetStr(WmiSCSIControllerDevice.Dependent),
          NegotiatedDataWidth: (uint?)GetInt(WmiSCSIControllerDevice.NegotiatedDataWidth),
          NegotiatedSpeed: GetULong(WmiSCSIControllerDevice.NegotiatedSpeed),
          NumberOfHardResets: (uint?)GetInt(WmiSCSIControllerDevice.NumberOfHardResets),
          NumberOfSoftResets: (uint?)GetInt(WmiSCSIControllerDevice.NumberOfSoftResets)));
      }
      return results;
    }
    catch {
      return Array.Empty<SCSIControllerDeviceMetrics>();
    }
  }
}
