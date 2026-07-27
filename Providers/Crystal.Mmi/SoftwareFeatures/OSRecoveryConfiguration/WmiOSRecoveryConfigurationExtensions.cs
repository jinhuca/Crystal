using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.SoftwareFeatures.OSRecoveryConfiguration;

public static class WmiOSRecoveryConfigurationExtensions {
  public static async Task<OSRecoveryConfigurationMetrics> ToSafeOSRecoveryConfigurationMetricsAsync(
      this IWmiHardwareProvider provider,
      CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance data collection asynchronously (normally exactly one instance)
      var instances = await provider.GetMultiMetricsForClassAsync(WmiOSRecoveryConfiguration.ClassName, cancellationToken);
      var data = instances.FirstOrDefault();

      // --- FULL NULL/CRASH FALLBACK RETRIEVAL ---
      if (data == null || data.Count == 0) {
        return new OSRecoveryConfigurationMetrics(
            null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null
        );
      }

      cancellationToken.ThrowIfCancellationRequested();

      // --- CLEAN LOOKUP CONDITIONAL WRAPPERS ---
      string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String ? v.AsString() : null;
      int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int ? v.AsInt() : null;
      bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool ? v.AsBool() : null;

      // --- INSTANTIATE SORTED EXTRACTED VALUES ---
      return new OSRecoveryConfigurationMetrics(
          AutoReboot: GetBool(WmiOSRecoveryConfiguration.AutoReboot),
          Caption: GetStr(WmiOSRecoveryConfiguration.Caption),
          DebugFilePath: GetStr(WmiOSRecoveryConfiguration.DebugFilePath),
          DebugInfoType: (uint?)GetInt(WmiOSRecoveryConfiguration.DebugInfoType),
          Description: GetStr(WmiOSRecoveryConfiguration.Description),
          ExpandedDebugFilePath: GetStr(WmiOSRecoveryConfiguration.ExpandedDebugFilePath),
          ExpandedMiniDumpDirectory: GetStr(WmiOSRecoveryConfiguration.ExpandedMiniDumpDirectory),
          KernelDumpOnly: GetBool(WmiOSRecoveryConfiguration.KernelDumpOnly),
          MiniDumpDirectory: GetStr(WmiOSRecoveryConfiguration.MiniDumpDirectory),
          Name: GetStr(WmiOSRecoveryConfiguration.Name),
          OverwriteExistingDebugFile: GetBool(WmiOSRecoveryConfiguration.OverwriteExistingDebugFile),
          SendAdminAlert: GetBool(WmiOSRecoveryConfiguration.SendAdminAlert),
          SettingID: GetStr(WmiOSRecoveryConfiguration.SettingID),
          WriteDebugInfo: GetBool(WmiOSRecoveryConfiguration.WriteDebugInfo),
          WriteToSystemLog: GetBool(WmiOSRecoveryConfiguration.WriteToSystemLog)
      );
    }
    catch {
      return new OSRecoveryConfigurationMetrics(
          null, null, null, null, null, null, null,
          null, null, null, null, null, null, null, null
      );
    }
  }
}
