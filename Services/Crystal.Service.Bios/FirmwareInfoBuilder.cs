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
  private readonly IWmiHardwareProvider _wmi;
  private readonly ISmbiosFirmwareProvider _smbios;
  private readonly IFirmwareSecurityProvider _security;

  public FirmwareInfoBuilder(
      IWmiHardwareProvider wmi,
      ISmbiosFirmwareProvider smbios,
      IFirmwareSecurityProvider security) {
    _wmi = wmi;
    _smbios = smbios;
    _security = security;
  }

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

  private async Task<SecureBootInfo> ReadSecureBootAsync(CancellationToken ct) {
    try {
      var state = await _security.GetSecureBootStateAsync(ct);
      return new SecureBootInfo(state.Supported, state.Enabled);
    } catch {
      return SecureBootInfo.Unknown;
    }
  }

  // SMBIOS reads can throw on non-Windows/locked-down hosts; the WMI half is still useful, so
  // firmware detail degrades gracefully to null rather than failing the whole snapshot.
  private SmbiosFirmwareInfo? ReadSmbios(CancellationToken ct) {
    try {
      return _smbios.GetFirmwareInfoAsync(ct).GetAwaiter().GetResult();
    } catch {
      return null;
    }
  }

  // The SMBIOS provider DTOs and their enums are re-projected into neutral service records so the
  // BiosViewModel never imports Crystal.Provider.Smbios. The enums mirror the provider's byte values
  // one-for-one (both follow DSP0134), so an ordinal cast is faithful.
  private static FirmwareSystemInfo? MapSystem(SmbiosSystemInfo? s) =>
      s is null ? null : new FirmwareSystemInfo(
          s.Manufacturer, s.ProductName, s.Version, s.SerialNumber, s.Uuid, s.SkuNumber, s.Family);

  private static FirmwareBaseboardInfo? MapBaseboard(SmbiosBaseboardInfo? b) =>
      b is null ? null : new FirmwareBaseboardInfo(
          b.Manufacturer, b.Product, b.Version, b.SerialNumber, b.AssetTag);

  private static FirmwareChassisInfo? MapChassis(SmbiosChassisInfo? c) =>
      c is null ? null : new FirmwareChassisInfo(
          c.Manufacturer, (ChassisType)c.ChassisType, c.SerialNumber, c.AssetTag);

  private static FirmwareHardwareSecurityInfo? MapHardwareSecurity(SmbiosHardwareSecurityInfo? h) =>
      h is null ? null : new FirmwareHardwareSecurityInfo(
          (HardwareSecurityStatus)h.PowerOnPassword,
          (HardwareSecurityStatus)h.KeyboardPassword,
          (HardwareSecurityStatus)h.AdministratorPassword,
          (HardwareSecurityStatus)h.FrontPanelReset);

  private static FirmwareBootInfo? MapBoot(SmbiosBootInfo? b) =>
      b is null ? null : new FirmwareBootInfo((BootStatus?)b.Status, b.StatusRaw);

  private static IReadOnlyList<FirmwareComponent> MapInventory(IReadOnlyList<SmbiosFirmwareComponent>? components) =>
      components is null ? []
          : components.Select(c => new FirmwareComponent(
              c.ComponentName, c.Version, c.ReleaseDate, c.Manufacturer,
              c.LowestSupportedVersion, c.ImageSizeBytes, (FirmwareComponentState)c.State)).ToList();

  private static FirmwareCapabilities? Capabilities(SmbiosBiosInfo? bios) =>
      bios is null ? null : new FirmwareCapabilities(
          FlashUpgradeable: bios.FlashUpgradeable,
          SelectableBoot: bios.SelectableBoot,
          BootFromCd: bios.BootFromCd);

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

  // Win32_Tpm.SpecVersion is a CSV like "2.0, 0, 1.38"; keep the leading spec-family number.
  private static string? NormalizeSpec(string? raw) {
    if (string.IsNullOrWhiteSpace(raw)) return null;
    int comma = raw.IndexOf(',');
    return (comma >= 0 ? raw[..comma] : raw).Trim();
  }

  private static string? SpecVersion(BiosMetrics wmi, SmbiosFirmwareInfo? smbios) {
    if (wmi.SMBIOSMajorVersion is { } major && wmi.SMBIOSMinorVersion is { } minor) {
      return $"{major}.{minor}";
    }
    if (smbios is { SmbiosMajorVersion: > 0 }) {
      return $"{smbios.SmbiosMajorVersion}.{smbios.SmbiosMinorVersion}";
    }
    return null;
  }

  private static string? EcRevision(BiosMetrics wmi) =>
      wmi.EmbeddedControllerMajorVersion is { } major && wmi.EmbeddedControllerMinorVersion is { } minor
          ? $"{major}.{minor}"
          : null;
}
