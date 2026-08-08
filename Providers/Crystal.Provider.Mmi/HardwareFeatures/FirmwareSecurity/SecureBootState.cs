namespace Crystal.Provider.Mmi.HardwareFeatures.FirmwareSecurity;

/// <summary>
/// Secure Boot posture read from the firmware state registry key
/// (<c>HKLM\SYSTEM\CurrentControlSet\Control\SecureBoot\State</c>).
/// </summary>
/// <param name="Supported">
/// True when the platform exposes the SecureBoot state key at all — i.e. the
/// machine booted via UEFI. False on legacy/BIOS boots where the key is absent.
/// </param>
/// <param name="Enabled">
/// True when Secure Boot is active. Null when the state can't be determined
/// (key or value missing, or the read failed).
/// </param>
public record SecureBootState(bool Supported, bool? Enabled) {
  /// <summary>Represents "could not determine" (read failed or non-Windows).</summary>
  public static SecureBootState Unknown { get; } = new(false, null);
}
