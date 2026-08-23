using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.BaseBoard;

/// <summary>
/// Provides extension methods for <see cref="IWmiHardwareProvider"/> to retrieve baseboard metrics from WMI (<c>Win32_BaseBoard</c>).
/// </summary>
public static class WmiBaseBoardExtensions {
  /// <summary>
  /// The WMI class name for baseboard information.
  /// </summary>
  private const string WmiClassName = WmiClasses.BaseBoard;

  /// <summary>
  /// Asynchronously retrieves baseboard metrics from WMI using the provided <see cref="IWmiHardwareProvider"/>.
  /// </summary>
  /// <param name="provider">The WMI hardware provider.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>A task that represents the asynchronous operation and returns the baseboard metrics.</returns>
  public static async Task<BaseBoardMetrics> ToSafeBaseBoardMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch data from our asynchronous query cache channel
      var instances = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      var data = instances.FirstOrDefault();

      // --- FULL NULL/CRASH FALLBACK RETRIEVAL ---
      if (data == null || data.Count == 0) {
        return new BaseBoardMetrics(
          null, null, null, null, null, null, null, null, null, null,
          null, null, null, null, null, null, null, null, null, null,
          null, null
        );
      }

      cancellationToken.ThrowIfCancellationRequested();

      // --- CLEAN LOOKUP CONDITIONAL WRAPPERS ---
      string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
        ? v.AsString() : null;
      bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool
        ? v.AsBool() : null;

      // Note: Float variables in WMI can occasionally stream as numeric variants, 
      // fallback to explicit checks to keep processing completely zero-allocation
      float? GetFloat(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
        ? (float)v.AsInt() : null;

      // --- INSTANTIATE SORTED EXTRACTED VALUES ---
      return new BaseBoardMetrics(
        Caption: GetStr(WmiBaseBoard.Caption),
        CreationClassName: GetStr(WmiBaseBoard.CreationClassName),
        Description: GetStr(WmiBaseBoard.Description),
        HostingBoard: GetBool(WmiBaseBoard.HostingBoard),
        HotSwappable: GetBool(WmiBaseBoard.HotSwappable),
        InstallationDate: GetStr(WmiBaseBoard.InstallationDate),
        Manufacturer: GetStr(WmiBaseBoard.Manufacturer),
        Model: GetStr(WmiBaseBoard.Model),
        Name: GetStr(WmiBaseBoard.Name),
        PartNumber: GetStr(WmiBaseBoard.PartNumber),
        Removable: GetBool(WmiBaseBoard.Removable),
        Replaceable: GetBool(WmiBaseBoard.Replaceable),
        Requirements: GetStr(WmiBaseBoard.RequirementsDescription),
        SerialNumber: GetStr(WmiBaseBoard.SerialNumber),
        SKU: GetStr(WmiBaseBoard.SKU),
        SlotLayout: GetStr(WmiBaseBoard.SlotLayout),
        SpecialRequirements: GetStr(WmiBaseBoard.SpecialRequirements),
        Status: GetStr(WmiBaseBoard.Status),
        Tag: GetStr(WmiBaseBoard.Tag),
        Version: GetStr(WmiBaseBoard.Version),
        Weight: GetFloat(WmiBaseBoard.Weight),
        Width: GetFloat(WmiBaseBoard.Width)
        );
    }
    catch {
      return new BaseBoardMetrics(
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null,
        null, null
        );
    }
  }
}
