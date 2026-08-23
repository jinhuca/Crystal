using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.Battery;

/// <summary>
/// Provides extension methods for <see cref="IWmiHardwareProvider"/> to retrieve battery metrics from WMI (<c>Win32_Battery</c>).
/// </summary>
public static class WmiBatteryExtensions {
  /// <summary>
  /// The WMI class name for battery metrics (<c>Win32_Battery</c>).
  /// </summary>
  private const string WmiClassName = WmiBattery.ClassName;

  /// <summary>
  /// Asynchronously retrieves battery metrics from WMI using the provided <see cref="IWmiHardwareProvider"/>. 
  /// If any error occurs during retrieval, it returns a <see cref="BatteryMetrics"/> instance with all properties set to null.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>A task that represents the asynchronous operation and returns the battery metrics.</returns>
  public static async Task<BatteryMetrics> ToSafeBatteryMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance data collection asynchronously
      var instances = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      var data = instances.FirstOrDefault();

      // --- FULL NULL/CRASH FALLBACK RETRIEVAL ---
      if (data == null || data.Count == 0) {
        return new BatteryMetrics(
          null, null, null, null, null, null, null, null, null, null,
          null, null, null, null, null, null, null, null, null, null,
          null, null, null, null, null, null, null, null, null, null,
          null, null, null
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
      DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime
        ? v.AsDateTime() : null;
      ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong
        ? v.AsReadOnlyULong() : null;
      ushort[]? GetUShortArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray
        ? v.AsUShortArray() : null;

      // --- INSTANTIATE SORTED EXTRACTED VALUES ---
      return new BatteryMetrics(
        Availability: (ushort?)GetInt(WmiBattery.Availability),
        BatteryRechargeTime: (uint?)GetInt(WmiBattery.BatteryRechargeTime),
        BatteryStatus: (ushort?)GetInt(WmiBattery.BatteryStatus),
        Caption: GetStr(WmiBattery.Caption),
        Chemistry: (ushort?)GetInt(WmiBattery.Chemistry),
        ConfigManagerErrorCode: (uint?)GetInt(WmiBattery.ConfigManagerErrorCode),
        ConfigManagerUserConfig: GetBool(WmiBattery.ConfigManagerUserConfig),
        CreationClassName: GetStr(WmiBattery.CreationClassName),
        Description: GetStr(WmiBattery.Description),
        DesignCapacity: (uint?)GetInt(WmiBattery.DesignCapacity),
        DesignVoltage: GetULong(WmiBattery.DesignVoltage),
        DeviceID: GetStr(WmiBattery.DeviceID),
        ErrorCleared: GetBool(WmiBattery.ErrorCleared),
        ErrorDescription: GetStr(WmiBattery.ErrorDescription),
        EstimatedChargeRemaining: (ushort?)GetInt(WmiBattery.EstimatedChargeRemaining),
        EstimatedRunTime: (uint?)GetInt(WmiBattery.EstimatedRunTime),
        ExpectedBatteryLife: (uint?)GetInt(WmiBattery.ExpectedBatteryLife),
        ExpectedLife: (uint?)GetInt(WmiBattery.ExpectedLife),
        FullChargeCapacity: (uint?)GetInt(WmiBattery.FullChargeCapacity),
        InstallDate: GetDate(WmiBattery.InstallationDate),
        LastErrorCode: (uint?)GetInt(WmiBattery.LastErrorCode),
        MaxRechargeTime: (uint?)GetInt(WmiBattery.MaxRechargeTime),
        Name: GetStr(WmiBattery.Name),
        PNPDeviceID: GetStr(WmiBattery.PNPDeviceID),
        PowerManagementCapabilities: GetUShortArr(WmiBattery.PowerManagementCapabilities),
        PowerManagementSupported: GetBool(WmiBattery.PowerManagementSupported),
        SmartBatteryVersion: GetStr(WmiBattery.SmartBatteryVersion),
        Status: GetStr(WmiBattery.Status),
        StatusInfo: (ushort?)GetInt(WmiBattery.StatusInfo),
        SystemCreationClassName: GetStr(WmiBattery.SystemCreationClassName),
        SystemName: GetStr(WmiBattery.SystemName),
        TimeOnBattery: (uint?)GetInt(WmiBattery.TimeOnBattery),
        TimeToFullCharge: (uint?)GetInt(WmiBattery.TimeToFullCharge));
    }
    catch {
      return new BatteryMetrics(
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null,
        null, null, null
      );
    }
  }
}
