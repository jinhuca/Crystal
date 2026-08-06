using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.UserAccount;
public static class WmiUserAccountExtensions {
  private const string WmiClassName = WmiUserAccount.ClassName;

  public static async Task<IReadOnlyList<UserAccountMetrics>> ToSafeUserAccountMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance user account data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<UserAccountMetrics>();
      }

      var results = new List<UserAccountMetrics>(instancesData.Count);

      // 2. Loop through every detected user account instance sequentially
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

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new UserAccountMetrics(
          AccountType: (uint?)GetInt(WmiUserAccount.AccountType),
          Caption: GetStr(WmiUserAccount.Caption),
          Description: GetStr(WmiUserAccount.Description),
          Disabled: GetBool(WmiUserAccount.Disabled),
          Domain: GetStr(WmiUserAccount.Domain),
          FullName: GetStr(WmiUserAccount.FullName),
          InstallDate: GetDate(WmiUserAccount.InstallDate),
          LocalAccount: GetBool(WmiUserAccount.LocalAccount),
          Lockout: GetBool(WmiUserAccount.Lockout),
          Name: GetStr(WmiUserAccount.Name),
          PasswordChangeable: GetBool(WmiUserAccount.PasswordChangeable),
          PasswordExpires: GetBool(WmiUserAccount.PasswordExpires),
          PasswordRequired: GetBool(WmiUserAccount.PasswordRequired),
          SID: GetStr(WmiUserAccount.SID),
          SIDType: (byte?)GetInt(WmiUserAccount.SIDType),
          Status: GetStr(WmiUserAccount.Status)));
      }
      return results;
    }
    catch {
      return Array.Empty<UserAccountMetrics>();
    }
  }
}
