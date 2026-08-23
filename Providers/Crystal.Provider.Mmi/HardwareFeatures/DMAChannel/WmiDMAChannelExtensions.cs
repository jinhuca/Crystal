using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.DMAChannel;

/// <summary>
/// Provides extension methods for <see cref="IWmiHardwareProvider"/> to read DMA channel metrics from WMI 
/// (<c>Win32_DMAChannel</c>) and convert them into safe, null-tolerant <see cref="DMAChannelMetrics"/> instances.
/// </summary>
public static class WmiDMAChannelExtensions {
  /// <summary>
  /// The WMI class name for DMA channel metrics (<c>Win32_DMAChannel</c>).
  /// </summary>
  private const string WmiClassName = WmiDMAChannel.ClassName;

  /// <summary>
  /// Asynchronously retrieves DMA channel metrics from WMI and converts them into a list of 
  /// <see cref="DMAChannelMetrics"/> instances.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>A task that represents the asynchronous operation and returns a list of DMA channel metrics.</returns>
  public static async Task<IReadOnlyList<DMAChannelMetrics>> ToSafeDMAChannelMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance DMA channel data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<DMAChannelMetrics>();
      }

      var results = new List<DMAChannelMetrics>(instancesData.Count);

      // 2. Loop through every detected DMA channel instance sequentially
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

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new DMAChannelMetrics(
          AddressSize: (ushort?)GetInt(WmiDMAChannel.AddressSize),
          Availability: (ushort?)GetInt(WmiDMAChannel.Availability),
          BurstMode: GetBool(WmiDMAChannel.BurstMode),
          ByteMode: (ushort?)GetInt(WmiDMAChannel.ByteMode),
          Caption: GetStr(WmiDMAChannel.Caption),
          ChannelTiming: (ushort?)GetInt(WmiDMAChannel.ChannelTiming),
          CreationClassName: GetStr(WmiDMAChannel.CreationClassName),
          CSCreationClassName: GetStr(WmiDMAChannel.CSCreationClassName),
          CSName: GetStr(WmiDMAChannel.CSName),
          Description: GetStr(WmiDMAChannel.Description),
          DMAChannel: (uint?)GetInt(WmiDMAChannel.DMAChannel),
          InstallDate: GetDate(WmiDMAChannel.InstallDate),
          MaxTransferSize: (uint?)GetInt(WmiDMAChannel.MaxTransferSize),
          Name: GetStr(WmiDMAChannel.Name),
          Port: (uint?)GetInt(WmiDMAChannel.Port),
          Status: GetStr(WmiDMAChannel.Status),
          TransferWidths: GetUShortArr(WmiDMAChannel.TransferWidths),
          TypeCTiming: (ushort?)GetInt(WmiDMAChannel.TypeCTiming),
          WordMode: (ushort?)GetInt(WmiDMAChannel.WordMode)));
      }
      return results;
    }
    catch {
      return Array.Empty<DMAChannelMetrics>();
    }
  }
}
