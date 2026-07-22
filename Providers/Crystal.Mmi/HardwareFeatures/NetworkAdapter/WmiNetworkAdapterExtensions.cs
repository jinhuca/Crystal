using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.NetworkAdapter;
public static class WmiNetworkAdapterExtensions {
  private const string WmiClassName = "Win32_NetworkAdapter";

  public static async Task<IReadOnlyList<NetworkAdapterMetrics>> ToSafeNetworkAdapterMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Query the asynchronous driver observer stream
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<NetworkAdapterMetrics>();
      }

      var results = new List<NetworkAdapterMetrics>(instancesData.Count);

      // 2. Filter and process device data structures sequentially
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

        // --- EDGE FILTER GUARDS: Extract physical hardware devices strictly ---
        bool isPhysical = GetBool("PhysicalAdapter") ?? false;
        string? mac = GetStr("MACAddress");

        if (!isPhysical || string.IsNullOrEmpty(mac)) {
          continue;
        }

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new NetworkAdapterMetrics(
          Availability: (ushort?)GetInt(WmiNetworkAdapter.Availability),
          Caption: GetStr(WmiNetworkAdapter.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiNetworkAdapter.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiNetworkAdapter.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiNetworkAdapter.CreationClassName),
          Description: GetStr(WmiNetworkAdapter.Description),
          DeviceID: GetStr(WmiNetworkAdapter.DeviceID),
          ErrorCleared: GetBool(WmiNetworkAdapter.ErrorCleared),
          ErrorDescription: GetStr(WmiNetworkAdapter.ErrorDescription),
          GUID: GetStr(WmiNetworkAdapter.GUID),
          Index: (uint?)GetInt(WmiNetworkAdapter.Index),
          InstallDate: GetDate(WmiNetworkAdapter.InstallDate),
          Installed: GetBool(WmiNetworkAdapter.Installed),
          InterfaceIndex: (uint?)GetInt(WmiNetworkAdapter.InterfaceIndex),
          LastErrorCode: GetStr(WmiNetworkAdapter.LastErrorCode),
          MACAddress: mac,
          Manufacturer: GetStr(WmiNetworkAdapter.Manufacturer),
          MaxNumberControlled: (uint?)GetInt(WmiNetworkAdapter.MaxNumberControlled),
          MaxSpeed: (uint?)GetInt(WmiNetworkAdapter.MaxSpeed),
          Name: GetStr(WmiNetworkAdapter.Name),
          NetConnectionID: GetStr(WmiNetworkAdapter.NetConnectionID),
          NetConnectionStatus: (ushort?)GetInt(WmiNetworkAdapter.NetConnectionStatus),
          NetEnabled: GetBool(WmiNetworkAdapter.NetEnabled),
          PNPDeviceID: GetStr(WmiNetworkAdapter.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiNetworkAdapter.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiNetworkAdapter.PowerManagementSupported),
          ProductName: GetStr(WmiNetworkAdapter.ProductName),
          ProviderName: GetStr(WmiNetworkAdapter.ProviderName),
          PhysicalAdapter: isPhysical,
          ServiceName: GetStr(WmiNetworkAdapter.ServiceName),
          Speed: GetULong(WmiNetworkAdapter.Speed),
          Status: GetStr(WmiNetworkAdapter.Status),
          StatusInfo: (ushort?)GetInt(WmiNetworkAdapter.StatusInfo),
          SystemCreationClassName: GetStr(WmiNetworkAdapter.SystemCreationClassName),
          SystemName: GetStr(WmiNetworkAdapter.SystemName),
          TimeOfLastReset: GetDate(WmiNetworkAdapter.TimeOfLastReset)));
      }

      return results;
    }
    catch {
      return Array.Empty<NetworkAdapterMetrics>();
    }
  }
}
