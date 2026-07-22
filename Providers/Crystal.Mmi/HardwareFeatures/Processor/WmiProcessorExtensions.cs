using System.Collections.Frozen;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.Processor;

public static class WmiProcessorExtensions {
  private const string WmiClassName = WmiClasses.Processor;

  public static async Task<ProcessorMetrics> ToSafeProcessorMetricsAsync(
      this IWmiHardwareProvider provider,
      CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance data collection asynchronously
      IReadOnlyList<FrozenDictionary<string, WmiValue>> instances =
          await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);

      // 2. Extract first instance data block safely
      FrozenDictionary<string, WmiValue>? data = instances.FirstOrDefault();

      // 3. Complete null/crash fallback object mapping if data is absent
      if(data == null || data.Count == 0) {
        return new ProcessorMetrics(
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null
        );
      }

      // Check cancellation mid-flight before entering mapping parsing
      cancellationToken.ThrowIfCancellationRequested();

      // 4. Concise Lookup Extraction Helpers
      string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String ? v.AsString() : null;
      int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int ? v.AsInt() : null;
      bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool ? v.AsBool() : null;
      DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime ? v.AsDateTime() : null;
      ushort[]? GetUShortArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray ? v.AsUShortArray() : null;

      // 5. Instantiate Sorted Extracted Values
      return new ProcessorMetrics(
        AddressWidth: (ushort?)GetInt(WmiProcessor.AddressWidth),
        Architecture: (ushort?)GetInt(WmiProcessor.Architecture),
        AssetTag: GetStr(WmiProcessor.AssetTag),
        Availability: (ushort?)GetInt(WmiProcessor.Availability),
        Caption: GetStr(WmiProcessor.Caption),
        Characteristics: (uint?)GetInt(WmiProcessor.Characteristics),
        ConfigManagerErrorCode: (uint?)GetInt(WmiProcessor.ConfigManagerErrorCode),
        ConfigManagerUserConfig: GetBool(WmiProcessor.ConfigManagerUserConfig),
        CpuStatus: (ushort?)GetInt(WmiProcessor.CpuStatus),
        CreationClassName: GetStr(WmiProcessor.CreationClassName),
        CurrentClockSpeed: (uint?)GetInt(WmiProcessor.CurrentClockSpeed),
        CurrentVoltage: (ushort?)GetInt(WmiProcessor.CurrentVoltage),
        DataWidth: (ushort?)GetInt(WmiProcessor.DataWidth),
        Description: GetStr(WmiProcessor.Description),
        DeviceID: GetStr(WmiProcessor.DeviceId),
        ErrorCleared: GetBool(WmiProcessor.ErrorCleared),
        ErrorDescription: GetStr(WmiProcessor.ErrorDescription),
        ExtClock: (uint?)GetInt(WmiProcessor.ExtClock),
        Family: (ushort?)GetInt(WmiProcessor.Family),
        InstallationDate: GetDate(WmiProcessor.InstallationDate),
        L2CacheSize: (uint?)GetInt(WmiProcessor.L2CacheSize),
        L2CacheSpeed: (uint?)GetInt(WmiProcessor.L2CacheSpeed),
        L3CacheSize: (uint?)GetInt(WmiProcessor.L3CacheSize),
        L3CacheSpeed: (uint?)GetInt(WmiProcessor.L3CacheSpeed),
        LastErrorCode: (uint?)GetInt(WmiProcessor.LastErrorCode),
        Level: (ushort?)GetInt(WmiProcessor.Level),
        LoadPercentage: (ushort?)GetInt(WmiProcessor.LoadPercentage),
        Manufacturer: GetStr(WmiProcessor.Manufacturer),
        MaxClockSpeed: (uint?)GetInt(WmiProcessor.MaxClockSpeed),
        Name: GetStr(WmiProcessor.Name),
        NumberOfCores: (uint?)GetInt(WmiProcessor.NumberOfCores),
        NumberOfEnabledCore: (uint?)GetInt(WmiProcessor.NumberOfEnabledCore),
        NumberOfLogicalProcessors: (uint?)GetInt(WmiProcessor.NumberOfLogicalProcessors),
        OtherFamilyDescription: GetStr(WmiProcessor.OtherFamilyDescription),
        PartNumber: GetStr(WmiProcessor.PartNumber),
        PNPDeviceID: GetStr(WmiProcessor.PnpDeviceId),
        PowerManagementCapabilities: GetUShortArr(WmiProcessor.PowerManagementCapabilities),
        PowerManagementSupported: GetBool(WmiProcessor.PowerManagementSupported),
        ProcessorId: GetStr(WmiProcessor.ProcessorId),
        ProcessorType: (ushort?)GetInt(WmiProcessor.ProcessorType),
        Revision: (ushort?)GetInt(WmiProcessor.Revision),
        Role: GetStr(WmiProcessor.Role),
        SecondLevelAddressTranslationExtensions: GetBool(WmiProcessor.SecondLevelAddressTranslationExtensions),
        SerialNumber: GetStr(WmiProcessor.SerialNumber),
        SocketDesignation: GetStr(WmiProcessor.SocketDesignation),
        StatusInfo: (ushort?)GetInt(WmiProcessor.StatusInfo),
        Status: GetStr(WmiProcessor.Status),
        Stepping: GetStr(WmiProcessor.Stepping),
        SystemCreationClassName: GetStr(WmiProcessor.SystemCreationClassName),
        SystemName: GetStr(WmiProcessor.SystemName),
        ThreadCount: (uint?)GetInt(WmiProcessor.ThreadCount),
        UniqueId: GetStr(WmiProcessor.UniqueId),
        UpgradeMethod: GetStr(WmiProcessor.UpgradeMethod),
        Version: GetStr(WmiProcessor.Version),
        VirtualizationFirmwareEnabled: GetBool(WmiProcessor.VirtualizationFirmwareEnabled),
        VMMonitorModeExtensions: GetBool(WmiProcessor.VMMonitorModeExtensions),
        VoltageCaps: (uint?)GetInt(WmiProcessor.VoltageCaps)
        );
    }
    catch(OperationCanceledException) {
      throw; // Propagate token cancellation bubble
    }
    catch {
      // Empty fail-safe backup fallback object instantiation
      return new ProcessorMetrics(
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null
      );
    }
  }
}
