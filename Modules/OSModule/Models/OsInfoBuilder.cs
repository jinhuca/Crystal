using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace OSModule.Models;

/// <summary>Builds the static <see cref="OsSnapshot"/> once from the environment and the Windows
/// registry (the <c>CurrentVersion</c> key carries the marketing edition/build fields that
/// <see cref="Environment.OSVersion"/> alone doesn't expose). Every read is best-effort: a missing
/// key or an access error leaves that field null rather than throwing, so an unreadable source
/// degrades to a placeholder in the view instead of failing the whole build.</summary>
public sealed class OsInfoBuilder {
  private const string CurrentVersionKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

  public OsSnapshot Build() {
    using var key = Registry.LocalMachine.OpenSubKey(CurrentVersionKey);

    string? caption = ReadString(key, "ProductName");
    string? displayVersion = ReadString(key, "DisplayVersion");
    string? build = ReadBuildNumber(key);
    string? edition = ReadString(key, "EditionID");
    string? owner = ReadString(key, "RegisteredOwner");
    string? organization = ReadString(key, "RegisteredOrganization");

    return new OsSnapshot(
        Caption: caption,
        Edition: edition,
        Version: Environment.OSVersion.Version.ToString(),
        BuildNumber: build,
        DisplayVersion: displayVersion,
        Architecture: RuntimeInformation.OSArchitecture.ToString(),
        RegisteredOwner: owner,
        RegisteredOrganization: organization,
        MachineName: SafeGet(() => Environment.MachineName),
        UserName: SafeGet(() => Environment.UserName),
        SystemDirectory: SafeGet(() => Environment.SystemDirectory),
        Locale: SafeGet(() => CultureInfo.CurrentCulture.DisplayName),
        TimeZone: SafeGet(() => TimeZoneInfo.Local.DisplayName),
        InstallDate: ReadInstallDate(key),
        LastBootTime: ReadLastBootTime());
  }

  // Prefer the newer "UBR" (update build revision) suffix so the build reads "22631.4169" rather
  // than the base "22631"; fall back to the plain build number when UBR is absent.
  private static string? ReadBuildNumber(RegistryKey? key) {
    string? build = ReadString(key, "CurrentBuildNumber");
    if (build is null) return null;
    return key?.GetValue("UBR") is int ubr ? $"{build}.{ubr}" : build;
  }

  // InstallDate is stored as a Unix timestamp (seconds since 1970-01-01 UTC) in a DWORD.
  private static DateTimeOffset? ReadInstallDate(RegistryKey? key) =>
      key?.GetValue("InstallDate") is int seconds
          ? DateTimeOffset.FromUnixTimeSeconds(seconds).ToLocalTime()
          : null;

  // The kernel tick count since boot; subtract from now to get the boot instant.
  private static DateTimeOffset? ReadLastBootTime() =>
      SafeGet<DateTimeOffset?>(() =>
          DateTimeOffset.Now - TimeSpan.FromMilliseconds(Environment.TickCount64));

  private static string? ReadString(RegistryKey? key, string name) {
    var value = key?.GetValue(name) as string;
    return string.IsNullOrWhiteSpace(value) ? null : value;
  }

  private static T? SafeGet<T>(Func<T> read) {
    try { return read(); }
    catch { return default; }
  }
}
