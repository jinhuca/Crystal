using Crystal.Provider.Mmi.MmiEngine;
namespace Crystal.Provider.Mmi.HardwareFeatures.CacheMemory;

public static class WmiCacheMemoryExtensions {
  public static async Task<IReadOnlyList<CacheMemoryMetrics>> ToSafeCacheMemoryMetricsAsync(this IWmiHardwareProvider provider, CancellationToken cancellationToken) {
    try {
      var rows = await provider.GetMultiMetricsForClassAsync(WmiCacheMemory.ClassName, cancellationToken);
      if (rows == null || rows.Count == 0) return Array.Empty<CacheMemoryMetrics>();
      var results = new List<CacheMemoryMetrics>(rows.Count);
      foreach (var data in rows) {
        cancellationToken.ThrowIfCancellationRequested();
        string? S(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.String ? v.AsString() : null;
        int? I(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Int ? v.AsInt() : null;
        bool? B(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Bool ? v.AsBool() : null;
        DateTime? D(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.DateTime ? v.AsDateTime() : null;
        ushort[]? A(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.UShortArray ? v.AsUShortArray() : null;
        ulong? U64(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.ULong ? v.AsReadOnlyULong() : null;
        results.Add(new CacheMemoryMetrics((ushort?)I(WmiCacheMemory.Access), S(WmiCacheMemory.AdditionalErrorData), (ushort?)I(WmiCacheMemory.Associativity), (ushort?)I(WmiCacheMemory.Availability), U64(WmiCacheMemory.BlockSize), (ushort?)I(WmiCacheMemory.CacheSpeed), (ushort?)I(WmiCacheMemory.CacheType), S(WmiCacheMemory.Caption), (uint?)I(WmiCacheMemory.ConfigManagerErrorCode), B(WmiCacheMemory.ConfigManagerUserConfig), B(WmiCacheMemory.CorrectableError), S(WmiCacheMemory.CreationClassName), A(WmiCacheMemory.CurrentSRAM), S(WmiCacheMemory.Description), S(WmiCacheMemory.DeviceID), U64(WmiCacheMemory.EndingAddress), (ushort?)I(WmiCacheMemory.ErrorAccess), U64(WmiCacheMemory.ErrorAddress), B(WmiCacheMemory.ErrorCleared), (ushort?)I(WmiCacheMemory.ErrorCorrectType), S(WmiCacheMemory.ErrorData), (ushort?)I(WmiCacheMemory.ErrorDataOrder), S(WmiCacheMemory.ErrorDescription), (ushort?)I(WmiCacheMemory.ErrorInfo), S(WmiCacheMemory.ErrorMethodology), U64(WmiCacheMemory.ErrorResolution), D(WmiCacheMemory.ErrorTime), (uint?)I(WmiCacheMemory.ErrorTransferSize), (uint?)I(WmiCacheMemory.FlushTimer), D(WmiCacheMemory.InstallationDate), (uint?)I(WmiCacheMemory.InstalledSize), (uint?)I(WmiCacheMemory.LastErrorCode), (ushort?)I(WmiCacheMemory.Level), (uint?)I(WmiCacheMemory.LineSize), S(WmiCacheMemory.Location), (uint?)I(WmiCacheMemory.MaxCacheSize), S(WmiCacheMemory.Name), U64(WmiCacheMemory.NumberOfBlocks), S(WmiCacheMemory.OtherErrorDescription), S(WmiCacheMemory.PNPDeviceID), A(WmiCacheMemory.PowerManagementCapabilities), B(WmiCacheMemory.PowerManagementSupported), S(WmiCacheMemory.Purpose), (ushort?)I(WmiCacheMemory.ReadPolicy), (ushort?)I(WmiCacheMemory.ReplacementPolicy), U64(WmiCacheMemory.StartingAddress), S(WmiCacheMemory.Status), (ushort?)I(WmiCacheMemory.StatusInfo), A(WmiCacheMemory.SupportedSRAM), S(WmiCacheMemory.SystemCreationClassName), B(WmiCacheMemory.SystemLevelAddress), S(WmiCacheMemory.SystemName), (ushort?)I(WmiCacheMemory.WritePolicy)));
      }
      return results;
    }
    catch { return Array.Empty<CacheMemoryMetrics>(); }
  }
}
