using Crystal.Provider.Mmi.HardwareFeatures.Bios;
using Crystal.Provider.Mmi.HardwareFeatures.FirmwareSecurity;
using Crystal.Provider.Mmi.HardwareFeatures.Tpm;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Firmware;

namespace Crystal.Service.Bios;

/// <summary>
/// Composes the platform firmware identity from WMI (<c>Win32_BIOS</c>, <c>Win32_Tpm</c>),
/// the registry (Secure Boot) and the SMBIOS table. WMI supplies the core BIOS fields
/// and the live TPM state; SMBIOS enriches with ROM size, UEFI support, revisions,
/// system/baseboard/chassis identity, hardware security, capabilities, boot status and
/// the firmware inventory. All sources are read once — this is static data.
/// </summary>
public sealed class FirmwareInfoBuilder {
  /// <summary>
  /// Supplies the core BIOS fields and live TPM state via WMI (<c>Win32_BIOS</c>, <c>Win32_Tpm</c>).
  /// </summary>
  private readonly IWmiHardwareProvider _wmi;

  /// <summary>
  /// Supplies the enriched SMBIOS detail (ROM size, revisions, identity, inventory, boot status).
  /// </summary>
  private readonly ISmbiosFirmwareProvider _smbios;

  /// <summary>
  /// Supplies the Secure Boot posture read from the registry.
  /// </summary>
  private readonly IFirmwareSecurityProvider _security;

  /// <summary>
  /// Creates the builder over the three firmware data sources.
  /// </summary>
  /// <param name="wmi">WMI provider for core BIOS fields and live TPM state.</param>
  /// <param name="smbios">SMBIOS provider for enriched firmware and identity detail.</param>
  /// <param name="security">Provider for the Secure Boot state.</param>
  public FirmwareInfoBuilder(
      IWmiHardwareProvider wmi,
      ISmbiosFirmwareProvider smbios,
      IFirmwareSecurityProvider security) {
    _wmi = wmi;
    _smbios = smbios;
    _security = security;
  }

  /// <summary>
  /// Reads every source once and composes them into a single <see cref="FirmwareSnapshot"/>. Core BIOS
  /// fields are WMI-first with an SMBIOS fallback; the remaining firmware detail comes from SMBIOS and the
  /// TPM view merges the live WMI query with the SMBIOS descriptor. Each source degrades independently, so a
  /// missing or failing source yields null sections rather than failing the whole snapshot.
  /// </summary>
  /// <param name="ct">Cancellation token for the underlying provider reads.</param>
  /// <returns>A task that resolves to the composed firmware snapshot.</returns>
  public async Task<FirmwareSnapshot> BuildAsync(CancellationToken ct) {
    var wmi = await _wmi.ToSafeBiosMetricsAsync(ct);
    var tpm = await _wmi.ToSafeTpmMetricsAsync(ct);
    var secureBoot = await ReadSecureBootAsync(ct);
    var smbios = ReadSmbios(ct);

    return new FirmwareSnapshot(
        Manufacturer: wmi.Manufacturer ?? smbios?.Bios?.Vendor,
        Version: wmi.SMBIOSBIOSVersion ?? wmi.BIOSVersion ?? wmi.Version ?? smbios?.Bios?.Version,
        ReleaseDate: wmi.ReleaseDate ?? smbios?.Bios?.ReleaseDate,
        SerialNumber: (wmi.SerialNumber ?? smbios?.System?.SerialNumber)?.Trim(),
        SmbiosSpecVersion: SpecVersion(wmi, smbios),
        PrimaryBios: wmi.PrimaryBIOS,
        Status: wmi.Status,
        RomSizeBytes: smbios?.Bios?.RomSizeBytes,
        IsUefi: smbios?.Bios?.IsUefiSupported,
        BiosRevision: smbios?.Bios?.BiosRevision,
        EmbeddedControllerRevision: smbios?.Bios?.EcFirmwareRevision ?? EcRevision(wmi),
        Capabilities: Capabilities(smbios?.Bios),
        System: MapSystem(smbios?.System),
        Baseboard: MapBaseboard(smbios?.Baseboard),
        Chassis: MapChassis(smbios?.Chassis),
        HardwareSecurity: MapHardwareSecurity(smbios?.HardwareSecurity),
        SecureBoot: secureBoot,
        Tpm: MergeTpm(tpm, smbios?.Tpm),
        Boot: MapBoot(smbios?.Boot),
        FirmwareInventory: MapInventory(smbios?.FirmwareInventory));
  }

  /// <summary>
  /// Reads the Secure Boot state, mapping any failure to <see cref="SecureBootInfo.Unknown"/> so a
  /// registry-access error never fails the whole snapshot.
  /// </summary>
  /// <param name="ct">Cancellation token for the security provider read.</param>
  /// <returns>The Secure Boot posture, or <see cref="SecureBootInfo.Unknown"/> if it could not be read.</returns>
  private async Task<SecureBootInfo> ReadSecureBootAsync(CancellationToken ct) {
    try {
      var state = await _security.GetSecureBootStateAsync(ct);
      return new SecureBootInfo(state.Supported, state.Enabled);
    } catch {
      return SecureBootInfo.Unknown;
    }
  }

  /// <summary>
  /// Reads the SMBIOS firmware table, returning null on failure.
  /// </summary>
  /// <remarks>
  /// SMBIOS reads can throw on non-Windows/locked-down hosts; the WMI half is still useful, so
  /// firmware detail degrades gracefully to null rather than failing the whole snapshot.
  /// </remarks>
  /// <param name="ct">Cancellation token for the SMBIOS provider read.</param>
  /// <returns>The SMBIOS firmware detail, or null when the table is unavailable.</returns>
  private SmbiosFirmwareInfo? ReadSmbios(CancellationToken ct) {
    try {
      return _smbios.GetFirmwareInfoAsync(ct).GetAwaiter().GetResult();
    } catch {
      return null;
    }
  }

  /// <summary>
  /// Re-projects SMBIOS Type 1 system identity into the neutral service record.
  /// </summary>
  /// <remarks>
  /// The SMBIOS provider DTOs and their enums are re-projected into neutral service records so the
  /// BiosViewModel never imports Crystal.Provider.Smbios. The enums mirror the provider's byte values
  /// one-for-one (both follow DSP0134), so an ordinal cast is faithful.
  /// </remarks>
  private static FirmwareSystemInfo? MapSystem(SmbiosSystemInfo? s) =>
      s is null ? null : new FirmwareSystemInfo(
          s.Manufacturer, s.ProductName, s.Version, s.SerialNumber, s.Uuid, s.SkuNumber, s.Family);

  /// <summary>
  /// Re-projects SMBIOS Type 2 baseboard identity into the neutral service record.
  /// </summary>
  private static FirmwareBaseboardInfo? MapBaseboard(SmbiosBaseboardInfo? b) =>
      b is null ? null : new FirmwareBaseboardInfo(
          b.Manufacturer, b.Product, b.Version, b.SerialNumber, b.AssetTag);

  /// <summary>
  /// Re-projects SMBIOS Type 3 chassis identity, casting the chassis-type byte to <see cref="ChassisType"/>.
  /// </summary>
  private static FirmwareChassisInfo? MapChassis(SmbiosChassisInfo? c) =>
      c is null ? null : new FirmwareChassisInfo(
          c.Manufacturer, (ChassisType)c.ChassisType, c.SerialNumber, c.AssetTag);

  /// <summary>
  /// Re-projects SMBIOS Type 24 hardware security, casting each status byte to <see cref="HardwareSecurityStatus"/>.
  /// </summary>
  private static FirmwareHardwareSecurityInfo? MapHardwareSecurity(SmbiosHardwareSecurityInfo? h) =>
      h is null ? null : new FirmwareHardwareSecurityInfo(
          (HardwareSecurityStatus)h.PowerOnPassword,
          (HardwareSecurityStatus)h.KeyboardPassword,
          (HardwareSecurityStatus)h.AdministratorPassword,
          (HardwareSecurityStatus)h.FrontPanelReset);

  /// <summary>
  /// Re-projects SMBIOS Type 32 boot status, preserving the raw byte alongside the mapped enum.
  /// </summary>
  private static FirmwareBootInfo? MapBoot(SmbiosBootInfo? b) =>
      b is null ? null : new FirmwareBootInfo((BootStatus?)b.Status, b.StatusRaw);

  /// <summary>
  /// Re-projects the SMBIOS Type 45 firmware inventory into service records; null input yields an empty list.
  /// </summary>
  private static IReadOnlyList<FirmwareComponent> MapInventory(IReadOnlyList<SmbiosFirmwareComponent>? components) =>
      components is null ? []
          : components.Select(c => new FirmwareComponent(
              c.ComponentName, c.Version, c.ReleaseDate, c.Manufacturer,
              c.LowestSupportedVersion, c.ImageSizeBytes, (FirmwareComponentState)c.State)).ToList();

  /// <summary>
  /// Projects the SMBIOS Type 0 BIOS characteristics into the capability flag record.
  /// </summary>
  private static FirmwareCapabilities? Capabilities(SmbiosBiosInfo? bios) =>
      bios is null ? null : new FirmwareCapabilities(
          FlashUpgradeable: bios.FlashUpgradeable,
          SelectableBoot: bios.SelectableBoot,
          BootFromCd: bios.BootFromCd);

  /// <summary>
  /// Merges the live <c>Win32_Tpm</c> query with the SMBIOS Type 43 descriptor into a single TPM view.
  /// Presence is inferred from either source; live state (enabled/activated/owned) comes from WMI, and the
  /// spec version and manufacturer prefer the live query with the SMBIOS descriptor as fallback.
  /// </summary>
  /// <param name="live">Live TPM metrics from WMI; all fields are null when no TPM instance exists.</param>
  /// <param name="descriptor">The SMBIOS TPM descriptor, or null when absent.</param>
  /// <returns>The merged TPM view, or <see cref="TpmInfo.Absent"/> when neither source indicates a TPM.</returns>
  private static TpmInfo MergeTpm(TpmMetrics live, SmbiosTpmInfo? descriptor) {
    // Win32_Tpm returns no instance (all fields null) when no TPM is present. The SMBIOS Type 43
    // descriptor can still be present, so treat either source as evidence of a TPM.
    bool present = live.InstanceName is not null
        || live.SpecVersion is not null
        || descriptor is not null;
    if (!present) return TpmInfo.Absent;

    return new TpmInfo(
        Present: true,
        Enabled: live.IsEnabled_InitialValue,
        Activated: live.IsActivated_InitialValue,
        Owned: live.IsOwned_InitialValue,
        SpecVersion: NormalizeSpec(live.SpecVersion) ?? descriptor?.SpecVersion,
        Manufacturer: live.ManufacturerIdTxt?.Trim() ?? descriptor?.VendorId);
  }

  /// <summary>
  /// Extracts the leading spec-family number from the raw <c>Win32_Tpm.SpecVersion</c> value.
  /// </summary>
  /// <param name="raw">The raw spec version, a CSV such as <c>"2.0, 0, 1.38"</c>.</param>
  /// <returns>The leading spec-family number (e.g. <c>"2.0"</c>), or null when the input is blank.</returns>
  private static string? NormalizeSpec(string? raw) {
    if (string.IsNullOrWhiteSpace(raw)) return null;
    int comma = raw.IndexOf(',');
    return (comma >= 0 ? raw[..comma] : raw).Trim();
  }

  /// <summary>
  /// Resolves the SMBIOS specification version as <c>major.minor</c>, preferring the WMI major/minor
  /// pair and falling back to the SMBIOS table's version when WMI does not report one.
  /// </summary>
  /// <param name="wmi">BIOS metrics from WMI.</param>
  /// <param name="smbios">SMBIOS firmware detail, or null when unavailable.</param>
  /// <returns>The spec version string, or null when no source reports one.</returns>
  private static string? SpecVersion(BiosMetrics wmi, SmbiosFirmwareInfo? smbios) {
    if (wmi.SMBIOSMajorVersion is { } major && wmi.SMBIOSMinorVersion is { } minor) {
      return $"{major}.{minor}";
    }
    if (smbios is { SmbiosMajorVersion: > 0 }) {
      return $"{smbios.SmbiosMajorVersion}.{smbios.SmbiosMinorVersion}";
    }
    return null;
  }

  /// <summary>
  /// Formats the embedded controller firmware revision as <c>major.minor</c> from the WMI metrics,
  /// or null when either component is missing. Used as the fallback when SMBIOS omits the EC revision.
  /// </summary>
  /// <param name="wmi">BIOS metrics from WMI.</param>
  /// <returns>The EC revision string, or null when unavailable.</returns>
  private static string? EcRevision(BiosMetrics wmi) =>
      wmi.EmbeddedControllerMajorVersion is { } major && wmi.EmbeddedControllerMinorVersion is { } minor
          ? $"{major}.{minor}"
          : null;
}
