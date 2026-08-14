using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Wmi;
using System.Collections.Frozen;

namespace Crystal.Provider.Mmi.HardwareFeatures.Processor;

public static class WmiProcessorMetricsExtensions {
  private const string WmiClassName = WmiClasses.Processor;

  /// <summary>
  /// Enumerates Win32_Processor and returns one <see cref="WmiProcessorMetrics"/>
  /// per socket. Unlike <see cref="WmiProcessorExtensions.ToSafeProcessorMetricsAsync"/>
  /// (which collapses to the first instance), this preserves every socket so the
  /// inventory pipeline can correlate multi-socket systems by SocketDesignation.
  /// </summary>
  public static async Task<IReadOnlyList<WmiProcessorMetrics>> ToProcessorMetricsListAsync(
      this IWmiHardwareProvider provider,
      CancellationToken cancellationToken) {
    try {
      IReadOnlyList<FrozenDictionary<string, WmiValue>> instances =
          await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);

      if (instances.Count == 0) return [];

      cancellationToken.ThrowIfCancellationRequested();

      var result = new List<WmiProcessorMetrics>(instances.Count);
      foreach (var data in instances) {
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String ? v.AsString() : null;
        uint? GetUInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int ? (uint)v.AsInt() : null;
        bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool ? v.AsBool() : null;

        result.Add(new WmiProcessorMetrics(
            SocketDesignation: GetStr(WmiProcessor.SocketDesignation),
            NumberOfLogicalProcessors: GetUInt(WmiProcessor.NumberOfLogicalProcessors),
            NumberOfCores: GetUInt(WmiProcessor.NumberOfCores),
            VirtualizationFirmwareEnabled: GetBool(WmiProcessor.VirtualizationFirmwareEnabled)));
      }
      return result;
    }
    catch (OperationCanceledException) {
      throw;
    }
    catch {
      return [];
    }
  }
}
