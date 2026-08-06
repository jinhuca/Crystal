using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.VideoController;
public static class WmiVideoControllerExtensions {
  private const string WmiClassName = WmiVideoController.ClassName;

  public static async Task<IReadOnlyList<VideoControllerMetrics>> ToSafeVideoControllerMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance graphics data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<VideoControllerMetrics>();
      }

      var results = new List<VideoControllerMetrics>(instancesData.Count);

      // 2. Loop through every detected physical GPU instance sequentially
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
        results.Add(new VideoControllerMetrics(
          Availability: (ushort?)GetInt(WmiVideoController.Availability),
          AdapterCompatibility: GetStr(WmiVideoController.AdapterCompatibility),
          AdapterDACType: GetStr(WmiVideoController.AdapterDACType),
          AdapterRAM: (uint?)GetInt(WmiVideoController.AdapterRAM),
          Architecture: (ushort?)GetInt(WmiVideoController.Architecture),
          Caption: GetStr(WmiVideoController.Caption),
          ColorTableEntries: (uint?)GetInt(WmiVideoController.ColorTableEntries),
          ConfigManagerErrorCode: (uint?)GetInt(WmiVideoController.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiVideoController.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiVideoController.CreationClassName),
          CurrentBitsPerPixel: (uint?)GetInt(WmiVideoController.CurrentBitsPerPixel),
          CurrentHorizontalResolution: (uint?)GetInt(WmiVideoController.CurrentHorizontalResolution),
          CurrentNumberOfColors: GetULong(WmiVideoController.CurrentNumberOfColors),
          CurrentNumberOfColumns: (uint?)GetInt(WmiVideoController.CurrentNumberOfColumns),
          CurrentNumberOfRows: (uint?)GetInt(WmiVideoController.CurrentNumberOfRows),
          CurrentRefreshRate: (uint?)GetInt(WmiVideoController.CurrentRefreshRate),
          CurrentVerticalResolution: (uint?)GetInt(WmiVideoController.CurrentVerticalResolution),
          Description: GetStr(WmiVideoController.Description),
          DeviceID: GetStr(WmiVideoController.DeviceID),
          DitherType: (uint?)GetInt(WmiVideoController.DitherType),
          ErrorCleared: GetBool(WmiVideoController.ErrorCleared),
          ErrorDescription: GetStr(WmiVideoController.ErrorDescription),
          ICMIntent: (uint?)GetInt(WmiVideoController.ICMIntent),
          ICMMethod: (uint?)GetInt(WmiVideoController.ICMMethod),
          InfDate: GetDate(WmiVideoController.InfDate),
          InfSection: GetStr(WmiVideoController.InfSection),
          InstalledDisplayDrivers: GetStr(WmiVideoController.InstalledDisplayDrivers),
          DriverVersion: GetStr(WmiVideoController.DriverVersion),
          InstallDate: GetDate(WmiVideoController.InstallationDate),
          LastErrorCode: (uint?)GetInt(WmiVideoController.LastErrorCode),
          MaxMemorySupported: (uint?)GetInt(WmiVideoController.MaxMemorySupported),
          MaxRefreshRate: (uint?)GetInt(WmiVideoController.MaxRefreshRate),
          MinRefreshRate: (uint?)GetInt(WmiVideoController.MinRefreshRate),
          Name: GetStr(WmiVideoController.Name),
          VideoArchitecture: (ushort?)GetInt(WmiVideoController.VideoArchitecture),
          VideoMemoryType: (ushort?)GetInt(WmiVideoController.VideoMemoryType),
          VideoProcessor: GetStr(WmiVideoController.VideoProcessor),
          PNPDeviceID: GetStr(WmiVideoController.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiVideoController.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiVideoController.PowerManagementSupported),
          Status: GetStr(WmiVideoController.Status),
          StatusInfo: (ushort?)GetInt(WmiVideoController.StatusInfo),
          SystemCreationClassName: GetStr(WmiVideoController.SystemCreationClassName),
          SystemName: GetStr(WmiVideoController.SystemName)));
      }

      return results;
    }
    catch {
      return Array.Empty<VideoControllerMetrics>();
    }
  }
}
