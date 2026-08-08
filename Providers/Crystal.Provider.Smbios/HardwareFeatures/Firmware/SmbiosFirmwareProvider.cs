using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;

namespace Crystal.Provider.Smbios.HardwareFeatures.Firmware;

/// <summary>
/// Reads the system SMBIOS table and projects the firmware-related structures
/// (Types 0, 1, 2, 24, 32, 43, 45) into a single <see cref="SmbiosFirmwareInfo"/>.
/// </summary>
public sealed class SmbiosFirmwareProvider : ISmbiosFirmwareProvider {
  public Task<SmbiosFirmwareInfo> GetFirmwareInfoAsync(CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();

    var table = SmbiosTable.Load();

    var info = new SmbiosFirmwareInfo(
        SmbiosMajorVersion: table.MajorVersion,
        SmbiosMinorVersion: table.MinorVersion,
        Bios: ProjectBios(table.Bios),
        System: ProjectSystem(table.System),
        Baseboard: ProjectBaseboard(table.Baseboard),
        Chassis: ProjectChassis(table.Chassis),
        HardwareSecurity: ProjectHardwareSecurity(table.HardwareSecurity),
        Boot: ProjectBoot(table.BootInformation),
        Tpm: ProjectTpm(table.Tpm),
        FirmwareInventory: table.FirmwareInventory.Select(ProjectComponent).ToList());

    return Task.FromResult(info);
  }

  private static SmbiosBiosInfo? ProjectBios(T000_BiosInformation? b) {
    if (b is null) return null;
    return new SmbiosBiosInfo(
        Vendor: b.Vendor,
        Version: b.Version,
        ReleaseDate: b.ReleaseDate,
        RomSizeBytes: b.RomSizeBytes,
        IsUefiSupported: b.IsUefiSupported,
        BiosRevision: Revision(b.BiosMajorRelease, b.BiosMinorRelease),
        EcFirmwareRevision: Revision(b.EcFirmwareMajor, b.EcFirmwareMinor),
        FlashUpgradeable: b.Characteristics.HasFlag(BiosCharacteristics.BiosFlashUpgradeable),
        SelectableBoot: b.Characteristics.HasFlag(BiosCharacteristics.SelectableBootSupported),
        BootFromCd: b.Characteristics.HasFlag(BiosCharacteristics.BootFromCdSupported));
  }

  private static SmbiosChassisInfo? ProjectChassis(T003_ChassisInformation? c) {
    if (c is null) return null;
    return new SmbiosChassisInfo(
        Manufacturer: c.Manufacturer,
        ChassisType: c.ChassisType,
        SerialNumber: c.SerialNumber?.Trim(),
        AssetTag: c.AssetTag);
  }

  private static SmbiosSystemInfo? ProjectSystem(T001_SystemInformation? s) {
    if (s is null) return null;
    return new SmbiosSystemInfo(
        Manufacturer: s.Manufacturer,
        ProductName: s.ProductName,
        Version: s.Version,
        SerialNumber: s.SerialNumber?.Trim(),
        Uuid: s.Uuid == Guid.Empty ? null : s.Uuid.ToString(),
        SkuNumber: s.SkuNumber,
        Family: s.Family);
  }

  private static SmbiosBaseboardInfo? ProjectBaseboard(T002_BaseboardInformation? b) {
    if (b is null) return null;
    return new SmbiosBaseboardInfo(
        Manufacturer: b.Manufacturer,
        Product: b.Product,
        Version: b.Version,
        SerialNumber: b.SerialNumber?.Trim(),
        AssetTag: b.AssetTag);
  }

  private static SmbiosHardwareSecurityInfo? ProjectHardwareSecurity(T024_HardwareSecurity? h) {
    if (h is null) return null;
    return new SmbiosHardwareSecurityInfo(
        PowerOnPassword: h.PowerOnPasswordStatus,
        KeyboardPassword: h.KeyboardPasswordStatus,
        AdministratorPassword: h.AdministratorPasswordStatus,
        FrontPanelReset: h.FrontPanelResetStatus);
  }

  private static SmbiosBootInfo? ProjectBoot(T032_SystemBootInformation? b) {
    if (b is null) return null;
    return new SmbiosBootInfo(Status: b.Status, StatusRaw: b.BootStatusRaw);
  }

  private static SmbiosTpmInfo? ProjectTpm(T043_TpmDevice? t) {
    if (t is null) return null;
    return new SmbiosTpmInfo(
        VendorId: string.IsNullOrWhiteSpace(t.VendorId) ? null : t.VendorId,
        SpecVersion: $"{t.MajorSpecVersion}.{t.MinorSpecVersion}",
        Description: t.Description);
  }

  private static SmbiosFirmwareComponent ProjectComponent(T045_FirmwareInventoryInformation f) =>
      new(
          ComponentName: f.FirmwareComponentName,
          Version: f.FirmwareVersion,
          ReleaseDate: f.ReleaseDate,
          Manufacturer: f.Manufacturer,
          LowestSupportedVersion: f.LowestSupportedVersion,
          ImageSizeBytes: f.ImageSizeBytes,
          State: f.State);

  // SMBIOS reports the major/minor release as 0xFF when the field isn't present.
  private static string? Revision(byte major, byte minor) =>
      major == 0xFF ? null : $"{major}.{minor}";
}
