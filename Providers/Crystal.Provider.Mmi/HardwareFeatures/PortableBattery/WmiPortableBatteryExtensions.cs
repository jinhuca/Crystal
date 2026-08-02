using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.PortableBattery;
public static class WmiPortableBatteryExtensions {
  private const string WmiClassName = WmiPortableBattery.ClassName;

  public static async Task<IReadOnlyList<PortableBatteryMetrics>> ToSafePortableBatteryMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance portable battery data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<PortableBatteryMetrics>();
      }

      var results = new List<PortableBatteryMetrics>(instancesData.Count);

      // 2. Loop through every detected portable battery instance sequentially
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
        ushort[]? GetUShortArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray
          ? v.AsUShortArray() : null;
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong
          ? v.AsReadOnlyULong() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new PortableBatteryMetrics(
          Availability: (ushort?)GetInt(WmiPortableBattery.Availability),
          BatteryRechargeTime: (uint?)GetInt(WmiPortableBattery.BatteryRechargeTime),
          BatteryStatus: (ushort?)GetInt(WmiPortableBattery.BatteryStatus),
          CapacityMultiplier: (ushort?)GetInt(WmiPortableBattery.CapacityMultiplier),
          Caption: GetStr(WmiPortableBattery.Caption),
          Chemistry: (ushort?)GetInt(WmiPortableBattery.Chemistry),
          ConfigManagerErrorCode: (uint?)GetInt(WmiPortableBattery.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiPortableBattery.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiPortableBattery.CreationClassName),
          Description: GetStr(WmiPortableBattery.Description),
          DesignCapacity: (uint?)GetInt(WmiPortableBattery.DesignCapacity),
          DesignVoltage: GetULong(WmiPortableBattery.DesignVoltage),
          DeviceID: GetStr(WmiPortableBattery.DeviceID),
          ErrorCleared: GetBool(WmiPortableBattery.ErrorCleared),
          ErrorDescription: GetStr(WmiPortableBattery.ErrorDescription),
          EstimatedChargeRemaining: (ushort?)GetInt(WmiPortableBattery.EstimatedChargeRemaining),
          EstimatedRunTime: (uint?)GetInt(WmiPortableBattery.EstimatedRunTime),
          ExpectedBatteryLife: (uint?)GetInt(WmiPortableBattery.ExpectedBatteryLife),
          ExpectedLife: (uint?)GetInt(WmiPortableBattery.ExpectedLife),
          FullChargeCapacity: (uint?)GetInt(WmiPortableBattery.FullChargeCapacity),
          InstallDate: GetDate(WmiPortableBattery.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiPortableBattery.LastErrorCode),
          Location: GetStr(WmiPortableBattery.Location),
          ManufactureDate: GetStr(WmiPortableBattery.ManufactureDate),
          Manufacturer: GetStr(WmiPortableBattery.Manufacturer),
          MaxBatteryError: (ushort?)GetInt(WmiPortableBattery.MaxBatteryError),
          MaxRechargeTime: (uint?)GetInt(WmiPortableBattery.MaxRechargeTime),
          Name: GetStr(WmiPortableBattery.Name),
          PNPDeviceID: GetStr(WmiPortableBattery.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiPortableBattery.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiPortableBattery.PowerManagementSupported),
          SmartBatteryVersion: GetStr(WmiPortableBattery.SmartBatteryVersion),
          Status: GetStr(WmiPortableBattery.Status),
          StatusInfo: (ushort?)GetInt(WmiPortableBattery.StatusInfo),
          SystemCreationClassName: GetStr(WmiPortableBattery.SystemCreationClassName),
          SystemName: GetStr(WmiPortableBattery.SystemName),
          TimeOnBattery: (uint?)GetInt(WmiPortableBattery.TimeOnBattery),
          TimeToFullCharge: (uint?)GetInt(WmiPortableBattery.TimeToFullCharge)));
      }
      return results;
    }
    catch {
      return Array.Empty<PortableBatteryMetrics>();
    }
  }
}
