using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.SerialPort;
public static class WmiSerialPortExtensions {
  private const string WmiClassName = WmiSerialPort.ClassName;

  public static async Task<IReadOnlyList<SerialPortMetrics>> ToSafeSerialPortMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance serial port data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<SerialPortMetrics>();
      }

      var results = new List<SerialPortMetrics>(instancesData.Count);

      // 2. Loop through every detected serial port instance sequentially
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
        string? GetFlattenedStrArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.StringArray
          ? string.Join(", ", v.AsStringArray() ?? Array.Empty<string>()) : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new SerialPortMetrics(
          Availability: (ushort?)GetInt(WmiSerialPort.Availability),
          Binary: GetBool(WmiSerialPort.Binary),
          Capabilities: GetUShortArr(WmiSerialPort.Capabilities),
          CapabilityDescriptions: GetFlattenedStrArr(WmiSerialPort.CapabilityDescriptions),
          Caption: GetStr(WmiSerialPort.Caption),
          ConfigManagerErrorCode: (uint?)GetInt(WmiSerialPort.ConfigManagerErrorCode),
          ConfigManagerUserConfig: GetBool(WmiSerialPort.ConfigManagerUserConfig),
          CreationClassName: GetStr(WmiSerialPort.CreationClassName),
          Description: GetStr(WmiSerialPort.Description),
          DeviceID: GetStr(WmiSerialPort.DeviceID),
          ErrorCleared: GetBool(WmiSerialPort.ErrorCleared),
          ErrorDescription: GetStr(WmiSerialPort.ErrorDescription),
          InstallDate: GetDate(WmiSerialPort.InstallDate),
          LastErrorCode: (uint?)GetInt(WmiSerialPort.LastErrorCode),
          MaxBaudRate: (uint?)GetInt(WmiSerialPort.MaxBaudRate),
          MaximumInputBufferSize: (uint?)GetInt(WmiSerialPort.MaximumInputBufferSize),
          MaximumOutputBufferSize: (uint?)GetInt(WmiSerialPort.MaximumOutputBufferSize),
          MaxNumberControlled: (uint?)GetInt(WmiSerialPort.MaxNumberControlled),
          Name: GetStr(WmiSerialPort.Name),
          OSAutoDiscovered: GetBool(WmiSerialPort.OSAutoDiscovered),
          PNPDeviceID: GetStr(WmiSerialPort.PNPDeviceID),
          PowerManagementCapabilities: GetUShortArr(WmiSerialPort.PowerManagementCapabilities),
          PowerManagementSupported: GetBool(WmiSerialPort.PowerManagementSupported),
          ProtocolSupported: (ushort?)GetInt(WmiSerialPort.ProtocolSupported),
          ProviderType: GetStr(WmiSerialPort.ProviderType),
          SettableBaudRate: GetBool(WmiSerialPort.SettableBaudRate),
          SettableDataBits: GetBool(WmiSerialPort.SettableDataBits),
          SettableFlowControl: GetBool(WmiSerialPort.SettableFlowControl),
          SettableParity: GetBool(WmiSerialPort.SettableParity),
          SettableParityCheck: GetBool(WmiSerialPort.SettableParityCheck),
          SettableRLSD: GetBool(WmiSerialPort.SettableRLSD),
          SettableStopBits: GetBool(WmiSerialPort.SettableStopBits),
          Status: GetStr(WmiSerialPort.Status),
          StatusInfo: (ushort?)GetInt(WmiSerialPort.StatusInfo),
          Supports16BitMode: GetBool(WmiSerialPort.Supports16BitMode),
          SupportsDTRDSR: GetBool(WmiSerialPort.SupportsDTRDSR),
          SupportsElapsedTimeouts: GetBool(WmiSerialPort.SupportsElapsedTimeouts),
          SupportsIntTimeouts: GetBool(WmiSerialPort.SupportsIntTimeouts),
          SupportsParityCheck: GetBool(WmiSerialPort.SupportsParityCheck),
          SupportsRLSD: GetBool(WmiSerialPort.SupportsRLSD),
          SupportsRTSCTS: GetBool(WmiSerialPort.SupportsRTSCTS),
          SupportsSpecialCharacters: GetBool(WmiSerialPort.SupportsSpecialCharacters),
          SupportsXOnXOff: GetBool(WmiSerialPort.SupportsXOnXOff),
          SupportsXOnXOffSet: GetBool(WmiSerialPort.SupportsXOnXOffSet),
          SystemCreationClassName: GetStr(WmiSerialPort.SystemCreationClassName),
          SystemName: GetStr(WmiSerialPort.SystemName),
          TimeOfLastReset: GetDate(WmiSerialPort.TimeOfLastReset)));
      }
      return results;
    }
    catch {
      return Array.Empty<SerialPortMetrics>();
    }
  }
}
