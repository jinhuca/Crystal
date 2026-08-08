using Crystal.Provider.Smbios.HardwareFeatures.Firmware;

namespace Crystal.Service.Bios;

/// <summary>
/// Aggregate, static firmware identity for the whole platform, composed from WMI
/// (<c>Win32_BIOS</c>, <c>Win32_Tpm</c>), the registry (Secure Boot) and the SMBIOS
/// table (BIOS/UEFI, system/baseboard/chassis identity, hardware security, boot
/// status, TPM descriptor and firmware inventory). Every field is optional — a
/// section is null when no source populated it.
/// </summary>
public sealed record FirmwareSnapshot(
    // ── Core BIOS identity (WMI-first, SMBIOS fallback) ──────────────────────
    string? Manufacturer,
    string? Version,
    string? ReleaseDate,
    string? SerialNumber,
    string? SmbiosSpecVersion,
    bool? PrimaryBios,
    string? Status,
    // ── Firmware detail from SMBIOS ──────────────────────────────────────────
    long? RomSizeBytes,
    bool? IsUefi,
    string? BiosRevision,
    string? EmbeddedControllerRevision,
    // ── Capabilities (SMBIOS Type 0 characteristics) ─────────────────────────
    FirmwareCapabilities? Capabilities,
    // ── System / baseboard / chassis identity ────────────────────────────────
    SmbiosSystemInfo? System,
    SmbiosBaseboardInfo? Baseboard,
    SmbiosChassisInfo? Chassis,
    // ── Security / boot ──────────────────────────────────────────────────────
    SmbiosHardwareSecurityInfo? HardwareSecurity,
    SecureBootInfo SecureBoot,
    TpmInfo Tpm,
    SmbiosBootInfo? Boot,
    // ── Platform firmware inventory ──────────────────────────────────────────
    IReadOnlyList<SmbiosFirmwareComponent> FirmwareInventory);

/// <summary>Firmware capability flags projected from SMBIOS Type 0 characteristics.</summary>
public sealed record FirmwareCapabilities(
    bool FlashUpgradeable,
    bool SelectableBoot,
    bool BootFromCd);

/// <summary>Secure Boot posture (registry).</summary>
public sealed record SecureBootInfo(bool Supported, bool? Enabled) {
  public static SecureBootInfo Unknown { get; } = new(false, null);
}

/// <summary>
/// Merged TPM view: presence and live state from <c>Win32_Tpm</c>, spec version
/// preferring the live query and falling back to the SMBIOS Type 43 descriptor.
/// </summary>
public sealed record TpmInfo(
    bool Present,
    bool? Enabled,
    bool? Activated,
    bool? Owned,
    string? SpecVersion,
    string? Manufacturer) {
  public static TpmInfo Absent { get; } = new(false, null, null, null, null, null);
}
