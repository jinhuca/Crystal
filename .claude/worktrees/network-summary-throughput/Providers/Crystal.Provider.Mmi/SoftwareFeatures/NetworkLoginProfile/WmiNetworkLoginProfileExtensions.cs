using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.SoftwareFeatures.NetworkLoginProfile;
public static class WmiNetworkLoginProfileExtensions {
  private const string WmiClassName = WmiNetworkLoginProfile.ClassName;

  public static async Task<IReadOnlyList<NetworkLoginProfileMetrics>> ToSafeNetworkLoginProfileMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance network login profile data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<NetworkLoginProfileMetrics>();
      }

      var results = new List<NetworkLoginProfileMetrics>(instancesData.Count);

      // 2. Loop through every detected network login profile instance sequentially
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime
          ? v.AsDateTime() : null;
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong
          ? v.AsReadOnlyULong() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new NetworkLoginProfileMetrics(
          AccountExpires: GetDate(WmiNetworkLoginProfile.AccountExpires),
          AuthorizationFlags: (uint?)GetInt(WmiNetworkLoginProfile.AuthorizationFlags),
          BadPasswordCount: (uint?)GetInt(WmiNetworkLoginProfile.BadPasswordCount),
          Caption: GetStr(WmiNetworkLoginProfile.Caption),
          CodePage: (uint?)GetInt(WmiNetworkLoginProfile.CodePage),
          Comment: GetStr(WmiNetworkLoginProfile.Comment),
          CountryCode: (uint?)GetInt(WmiNetworkLoginProfile.CountryCode),
          Description: GetStr(WmiNetworkLoginProfile.Description),
          Flags: (uint?)GetInt(WmiNetworkLoginProfile.Flags),
          FullName: GetStr(WmiNetworkLoginProfile.FullName),
          HomeDirectory: GetStr(WmiNetworkLoginProfile.HomeDirectory),
          HomeDirectoryDrive: GetStr(WmiNetworkLoginProfile.HomeDirectoryDrive),
          LastLogoff: GetDate(WmiNetworkLoginProfile.LastLogoff),
          LastLogon: GetDate(WmiNetworkLoginProfile.LastLogon),
          LogonHours: GetStr(WmiNetworkLoginProfile.LogonHours),
          LogonServer: GetStr(WmiNetworkLoginProfile.LogonServer),
          MaximumStorage: GetULong(WmiNetworkLoginProfile.MaximumStorage),
          Name: GetStr(WmiNetworkLoginProfile.Name),
          NumberOfLogons: (uint?)GetInt(WmiNetworkLoginProfile.NumberOfLogons),
          Parameters: GetStr(WmiNetworkLoginProfile.Parameters),
          PasswordAge: GetDate(WmiNetworkLoginProfile.PasswordAge),
          PasswordExpires: GetDate(WmiNetworkLoginProfile.PasswordExpires),
          PrimaryGroupId: (uint?)GetInt(WmiNetworkLoginProfile.PrimaryGroupId),
          Privileges: (uint?)GetInt(WmiNetworkLoginProfile.Privileges),
          Profile: GetStr(WmiNetworkLoginProfile.Profile),
          ScriptPath: GetStr(WmiNetworkLoginProfile.ScriptPath),
          SettingID: GetStr(WmiNetworkLoginProfile.SettingID),
          UnitsPerWeek: (uint?)GetInt(WmiNetworkLoginProfile.UnitsPerWeek),
          UserComment: GetStr(WmiNetworkLoginProfile.UserComment),
          UserId: (uint?)GetInt(WmiNetworkLoginProfile.UserId),
          UserType: GetStr(WmiNetworkLoginProfile.UserType),
          Workstations: GetStr(WmiNetworkLoginProfile.Workstations)));
      }
      return results;
    }
    catch {
      return Array.Empty<NetworkLoginProfileMetrics>();
    }
  }
}
