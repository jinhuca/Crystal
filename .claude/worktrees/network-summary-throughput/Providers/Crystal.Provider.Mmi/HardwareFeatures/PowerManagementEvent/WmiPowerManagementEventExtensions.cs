using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.PowerManagementEvent;
public static class WmiPowerManagementEventExtensions {
  private const string WmiClassName = WmiPowerManagementEvent.ClassName;

  public static async Task<IReadOnlyList<PowerManagementEventMetrics>> ToSafePowerManagementEventMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance power management event data blocks asynchronously.
      //    Note: this is an extrinsic event class — expect an empty result outside of
      //    an active event subscription (see remarks in PowerManagementEventMetrics.cs).
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<PowerManagementEventMetrics>();
      }

      var results = new List<PowerManagementEventMetrics>(instancesData.Count);

      // 2. Loop through every detected event instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new PowerManagementEventMetrics(
          EventType: (ushort?)GetInt(WmiPowerManagementEvent.EventType),
          OEMEventCode: (ushort?)GetInt(WmiPowerManagementEvent.OEMEventCode)));
      }
      return results;
    }
    catch {
      return Array.Empty<PowerManagementEventMetrics>();
    }
  }
}
