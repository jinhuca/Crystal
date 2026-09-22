namespace Crystal.Service.Bios;

/// <summary>
/// Aggregate, static firmware identity for the whole platform, composed from WMI
/// (<c>Win32_BIOS</c>, <c>Win32_Tpm</c>), the registry (Secure Boot) and the SMBIOS
/// table (BIOS/UEFI, system/baseboard/chassis identity, hardware security, boot
/// status, TPM descriptor and firmware inventory). Every field is optional — a
/// section is null when no source populated it.
/// </summary>
/// <param name="Manufacturer">BIOS/firmware vendor (WMI-first, SMBIOS fallback).</param>
/// <param name="Version">BIOS version string (WMI-first, SMBIOS fallback).</param>
/// <param name="ReleaseDate">BIOS release date as reported by the source.</param>
/// <param name="SerialNumber">System serial number, trimmed (WMI-first, SMBIOS fallback).</param>
/// <param name="SmbiosSpecVersion">SMBIOS specification version as <c>major.minor</c>.</param>
/// <param name="PrimaryBios">Whether this is the primary BIOS on the platform.</param>
/// <param name="Status">BIOS status string reported by WMI.</param>
/// <param name="RomSizeBytes">BIOS ROM size in bytes (SMBIOS).</param>
/// <param name="IsUefi">Whether UEFI is supported (SMBIOS).</param>
/// <param name="BiosRevision">BIOS revision (SMBIOS).</param>
/// <param name="EmbeddedControllerRevision">Embedded controller firmware revision (SMBIOS, WMI fallback).</param>
/// <param name="Capabilities">BIOS capability flags from SMBIOS Type 0 characteristics.</param>
/// <param name="System">System identity (SMBIOS Type 1).</param>
/// <param name="Baseboard">Baseboard/motherboard identity (SMBIOS Type 2).</param>
/// <param name="Chassis">Chassis/enclosure identity (SMBIOS Type 3).</param>
/// <param name="HardwareSecurity">Hardware security posture (SMBIOS Type 24).</param>
/// <param name="SecureBoot">Secure Boot posture read from the registry.</param>
/// <param name="Tpm">Merged TPM view from WMI and the SMBIOS descriptor.</param>
/// <param name="Boot">System boot status (SMBIOS Type 32).</param>
/// <param name="FirmwareInventory">Platform firmware component inventory (SMBIOS Type 45); empty when none.</param>
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
    FirmwareSystemInfo? System,
    FirmwareBaseboardInfo? Baseboard,
    FirmwareChassisInfo? Chassis,
    // ── Security / boot ──────────────────────────────────────────────────────
    FirmwareHardwareSecurityInfo? HardwareSecurity,
    SecureBootInfo SecureBoot,
    TpmInfo Tpm,
    FirmwareBootInfo? Boot,
    // ── Platform firmware inventory ──────────────────────────────────────────
    IReadOnlyList<FirmwareComponent> FirmwareInventory);

/// <summary>
/// Firmware capability flags projected from SMBIOS Type 0 characteristics.
/// </summary>
/// <param name="FlashUpgradeable">Whether the BIOS can be flash-upgraded in the field.</param>
/// <param name="SelectableBoot">Whether the boot device is user-selectable.</param>
/// <param name="BootFromCd">Whether booting from CD is supported.</param>
public sealed record FirmwareCapabilities(
    bool FlashUpgradeable,
    bool SelectableBoot,
    bool BootFromCd);

/// <summary>
/// Secure Boot posture (registry).
/// </summary>
/// <param name="Supported">Whether the platform supports Secure Boot.</param>
/// <param name="Enabled">Whether Secure Boot is currently enabled; null when it could not be determined.</param>
public sealed record SecureBootInfo(bool Supported, bool? Enabled) {
  /// <summary>
  /// Sentinel used when the Secure Boot state cannot be read (unsupported, unknown enabled state).
  /// </summary>
  public static SecureBootInfo Unknown { get; } = new(false, null);
}

/// <summary>
/// Merged TPM view: presence and live state from <c>Win32_Tpm</c>, spec version
/// preferring the live query and falling back to the SMBIOS Type 43 descriptor.
/// </summary>
/// <param name="Present">Whether a TPM is present, inferred from either the live query or the SMBIOS descriptor.</param>
/// <param name="Enabled">Whether the TPM is enabled (live query); null when unknown.</param>
/// <param name="Activated">Whether the TPM is activated (live query); null when unknown.</param>
/// <param name="Owned">Whether the TPM is owned (live query); null when unknown.</param>
/// <param name="SpecVersion">TPM specification version, live query preferred over the SMBIOS descriptor.</param>
/// <param name="Manufacturer">TPM manufacturer, live query preferred over the SMBIOS descriptor.</param>
public sealed record TpmInfo(
    bool Present,
    bool? Enabled,
    bool? Activated,
    bool? Owned,
    string? SpecVersion,
    string? Manufacturer) {
  /// <summary>
  /// Sentinel used when no TPM is present on the platform.
  /// </summary>
  public static TpmInfo Absent { get; } = new(false, null, null, null, null, null);
}

/// <summary>
/// System identity (SMBIOS Type 1).
/// </summary>
/// <param name="Manufacturer">System manufacturer.</param>
/// <param name="ProductName">System product name/model.</param>
/// <param name="Version">System version.</param>
/// <param name="SerialNumber">System serial number.</param>
/// <param name="Uuid">System UUID.</param>
/// <param name="SkuNumber">System SKU number.</param>
/// <param name="Family">System product family.</param>
public sealed record FirmwareSystemInfo(
    string? Manufacturer,
    string? ProductName,
    string? Version,
    string? SerialNumber,
    string? Uuid,
    string? SkuNumber,
    string? Family);

/// <summary>
/// Baseboard/motherboard identity (SMBIOS Type 2).
/// </summary>
/// <param name="Manufacturer">Baseboard manufacturer.</param>
/// <param name="Product">Baseboard product name.</param>
/// <param name="Version">Baseboard version.</param>
/// <param name="SerialNumber">Baseboard serial number.</param>
/// <param name="AssetTag">Baseboard asset tag.</param>
public sealed record FirmwareBaseboardInfo(
    string? Manufacturer,
    string? Product,
    string? Version,
    string? SerialNumber,
    string? AssetTag);

/// <summary>
/// Chassis/enclosure identity (SMBIOS Type 3).
/// </summary>
/// <param name="Manufacturer">Chassis manufacturer.</param>
/// <param name="ChassisType">Physical chassis form factor.</param>
/// <param name="SerialNumber">Chassis serial number.</param>
/// <param name="AssetTag">Chassis asset tag.</param>
public sealed record FirmwareChassisInfo(
    string? Manufacturer,
    ChassisType ChassisType,
    string? SerialNumber,
    string? AssetTag);

/// <summary>
/// Hardware security posture (SMBIOS Type 24).
/// </summary>
/// <param name="PowerOnPassword">Power-on password status.</param>
/// <param name="KeyboardPassword">Keyboard password status.</param>
/// <param name="AdministratorPassword">Administrator password status.</param>
/// <param name="FrontPanelReset">Front-panel reset status.</param>
public sealed record FirmwareHardwareSecurityInfo(
    HardwareSecurityStatus PowerOnPassword,
    HardwareSecurityStatus KeyboardPassword,
    HardwareSecurityStatus AdministratorPassword,
    HardwareSecurityStatus FrontPanelReset);

/// <summary>
/// System boot status (SMBIOS Type 32).
/// </summary>
/// <param name="Status">The decoded boot status, or null when the raw value is unrecognized.</param>
/// <param name="StatusRaw">The raw boot-status byte as read from SMBIOS.</param>
public sealed record FirmwareBootInfo(
    BootStatus? Status,
    byte StatusRaw);

/// <summary>
/// One platform firmware component from the inventory (SMBIOS Type 45).
/// </summary>
/// <param name="ComponentName">Human-readable component name.</param>
/// <param name="Version">Component firmware version.</param>
/// <param name="ReleaseDate">Component firmware release date.</param>
/// <param name="Manufacturer">Component manufacturer.</param>
/// <param name="LowestSupportedVersion">Lowest firmware version to which the component can be downgraded.</param>
/// <param name="ImageSizeBytes">Firmware image size in bytes.</param>
/// <param name="State">Current operational state of the component.</param>
public sealed record FirmwareComponent(
    string? ComponentName,
    string? Version,
    string? ReleaseDate,
    string? Manufacturer,
    string? LowestSupportedVersion,
    ulong ImageSizeBytes,
    FirmwareComponentState State);

/// <summary>
/// Physical chassis type (SMBIOS Type 3, DSP0134 §7.4.1). Values match the spec.
/// </summary>
public enum ChassisType : byte {
  Other = 0x01,
  Unknown = 0x02,
  Desktop = 0x03,
  LowProfileDesktop = 0x04,
  PizzaBox = 0x05,
  MiniTower = 0x06,
  Tower = 0x07,
  Portable = 0x08,
  Laptop = 0x09,
  Notebook = 0x0A,
  HandHeld = 0x0B,
  DockingStation = 0x0C,
  AllInOne = 0x0D,
  SubNotebook = 0x0E,
  SpaceSaving = 0x0F,
  LunchBox = 0x10,
  MainServerChassis = 0x11,
  ExpansionChassis = 0x12,
  SubChassis = 0x13,
  BusExpansionChassis = 0x14,
  PeripheralChassis = 0x15,
  RAIDChassis = 0x16,
  RackMountChassis = 0x17,
  SealedCasePC = 0x18,
  MultiChassis = 0x19,
  CompactPCI = 0x1A,
  AdvancedTCA = 0x1B,
  Blade = 0x1C,
  BladeEnclosure = 0x1D,
  Tablet = 0x1E,
  Convertible = 0x1F,
  Detachable = 0x20,
  IoTGateway = 0x21,
  EmbeddedSystem = 0x22,
  MiniPC = 0x23,
  StickPC = 0x24,
}

/// <summary>
/// Hardware security password/reset status (SMBIOS Type 24, DSP0134 §7.25).
/// </summary>
public enum HardwareSecurityStatus : byte {
  Disabled = 0x00,
  Enabled = 0x01,
  NotImplemented = 0x02,
  Unknown = 0x03,
}

/// <summary>
/// System boot status (SMBIOS Type 32, DSP0134 §7.33.1).
/// </summary>
public enum BootStatus : byte {
  NoError = 0x00,
  NoBootableMedia = 0x01,
  NormalOSFailedLoading = 0x02,
  FirmwareDetectedFailure = 0x03,
  OSDetectedFailure = 0x04,
  UserRequestedBoot = 0x05,
  SystemSecurityViolation = 0x06,
  PreviousRequestedImage = 0x07,
  WatchdogTimerExpired = 0x08,
}

/// <summary>
/// Firmware component state (SMBIOS Type 45, DSP0134 §7.46).
/// </summary>
public enum FirmwareComponentState : byte {
  Other = 0x01,
  Unknown = 0x02,
  Disabled = 0x03,
  Enabled = 0x04,
  Absent = 0x05,
  StandbyOffline = 0x06,
  StandbySpare = 0x07,
  UnavailableOffline = 0x08,
}
