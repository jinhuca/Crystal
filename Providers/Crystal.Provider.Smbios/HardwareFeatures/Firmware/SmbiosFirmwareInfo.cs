using Crystal.Provider.Smbios.Types;
using System.Collections.Generic;

namespace Crystal.Provider.Smbios.HardwareFeatures.Firmware;

/// <summary>
/// A firmware-oriented projection of the system SMBIOS table: the BIOS/UEFI
/// identity (Type 0), the enclosing system and baseboard identity (Type 1/2),
/// hardware security posture (Type 24), the current boot status (Type 32), the
/// TPM device (Type 43) and the platform firmware inventory (Type 45). Every
/// section is nullable/optional — a given board only populates the structures
/// it supports.
/// </summary>
public sealed record SmbiosFirmwareInfo(
    byte SmbiosMajorVersion,
    byte SmbiosMinorVersion,
    SmbiosBiosInfo? Bios,
    SmbiosSystemInfo? System,
    SmbiosBaseboardInfo? Baseboard,
    SmbiosChassisInfo? Chassis,
    SmbiosHardwareSecurityInfo? HardwareSecurity,
    SmbiosBootInfo? Boot,
    SmbiosTpmInfo? Tpm,
    IReadOnlyList<SmbiosFirmwareComponent> FirmwareInventory);

/// <summary>BIOS/UEFI firmware identity and capability flags (SMBIOS Type 0).</summary>
public sealed record SmbiosBiosInfo(
    string? Vendor,
    string? Version,
    string? ReleaseDate,
    long RomSizeBytes,
    bool IsUefiSupported,
    string? BiosRevision,
    string? EcFirmwareRevision,
    bool FlashUpgradeable,
    bool SelectableBoot,
    bool BootFromCd);

/// <summary>System identity (SMBIOS Type 1).</summary>
public sealed record SmbiosSystemInfo(
    string? Manufacturer,
    string? ProductName,
    string? Version,
    string? SerialNumber,
    string? Uuid,
    string? SkuNumber,
    string? Family);

/// <summary>Baseboard/motherboard identity (SMBIOS Type 2).</summary>
public sealed record SmbiosBaseboardInfo(
    string? Manufacturer,
    string? Product,
    string? Version,
    string? SerialNumber,
    string? AssetTag);

/// <summary>Chassis/enclosure identity (SMBIOS Type 3).</summary>
public sealed record SmbiosChassisInfo(
    string? Manufacturer,
    PhysicalChassisType ChassisType,
    string? SerialNumber,
    string? AssetTag);

/// <summary>Hardware security posture (SMBIOS Type 24).</summary>
public sealed record SmbiosHardwareSecurityInfo(
    HardwareSecurityStatus PowerOnPassword,
    HardwareSecurityStatus KeyboardPassword,
    HardwareSecurityStatus AdministratorPassword,
    HardwareSecurityStatus FrontPanelReset);

/// <summary>System boot status (SMBIOS Type 32).</summary>
public sealed record SmbiosBootInfo(
    SystemBootStatus? Status,
    byte StatusRaw);

/// <summary>Trusted Platform Module device (SMBIOS Type 43).</summary>
public sealed record SmbiosTpmInfo(
    string? VendorId,
    string SpecVersion,
    string? Description);

/// <summary>One platform firmware component from the inventory (SMBIOS Type 45).</summary>
public sealed record SmbiosFirmwareComponent(
    string? ComponentName,
    string? Version,
    string? ReleaseDate,
    string? Manufacturer,
    string? LowestSupportedVersion,
    ulong ImageSizeBytes,
    FirmwareInventoryState State);
