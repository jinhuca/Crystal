using Crystal.Provider.Mmi.HardwareFeatures.FirmwareSecurity;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Smbios.HardwareFeatures.Firmware;
using Crystal.Provider.Smbios.Types;
using Xunit;

namespace Crystal.Service.Bios.Tests;

/// <summary>
/// Covers <see cref="FirmwareInfoBuilder.BuildAsync"/> field-precedence, TPM merge, spec-version
/// selection and graceful degradation. The WMI half is driven through real property bags so the
/// <c>ToSafe*MetricsAsync</c> extensions run for real; SMBIOS and Secure Boot use hand fakes.
/// </summary>
public class FirmwareInfoBuilderTests {
  // ── property-bag builders keyed by the exact WMI property names the extensions read ──
  private static Dictionary<string, WmiValue> Bios(Action<Dictionary<string, WmiValue>> fill) {
    var bag = new Dictionary<string, WmiValue>();
    fill(bag);
    return bag;
  }

  private static FirmwareInfoBuilder Builder(
      IReadOnlyDictionary<string, WmiValue>? bios = null,
      IReadOnlyDictionary<string, WmiValue>? tpm = null,
      SmbiosFirmwareInfo? smbios = null,
      SecureBootState? secureBoot = null) =>
      new(new FakeWmiProvider(bios, tpm),
          new FakeSmbiosProvider(smbios),
          new FakeSecurityProvider(secureBoot ?? SecureBootState.Unknown));

  private static SmbiosFirmwareInfo Smbios(
      SmbiosBiosInfo? biosInfo = null,
      SmbiosSystemInfo? system = null,
      SmbiosTpmInfo? tpm = null,
      byte major = 3, byte minor = 4) =>
      new(major, minor, biosInfo, system, Baseboard: null, Chassis: null,
          HardwareSecurity: null, Boot: null, Tpm: tpm, FirmwareInventory: []);

  private static SmbiosBiosInfo BiosInfo(
      string? vendor = null, string? version = null, string? releaseDate = null,
      long romSize = 0, bool uefi = false, string? biosRevision = null,
      string? ecRevision = null, bool flash = false, bool selectableBoot = false,
      bool bootFromCd = false) =>
      new(vendor, version, releaseDate, romSize, uefi, biosRevision, ecRevision,
          flash, selectableBoot, bootFromCd);

  // ─────────────────────────── Field precedence (WMI first) ───────────────────────────

  [Fact]
  public async Task Manufacturer_prefers_wmi_over_smbios() {
    var builder = Builder(
        bios: Bios(b => b["Manufacturer"] = new WmiValue("American Megatrends")),
        smbios: Smbios(BiosInfo(vendor: "SMBIOS Vendor")));

    var snap = await builder.BuildAsync(default);

    Assert.Equal("American Megatrends", snap.Manufacturer);
  }

  [Fact]
  public async Task Manufacturer_falls_back_to_smbios_vendor_when_wmi_absent() {
    var builder = Builder(smbios: Smbios(BiosInfo(vendor: "SMBIOS Vendor")));

    var snap = await builder.BuildAsync(default);

    Assert.Equal("SMBIOS Vendor", snap.Manufacturer);
  }

  [Fact]
  public async Task Version_follows_wmi_precedence_chain() {
    // SMBIOSBIOSVersion wins over BIOSVersion/Version when present.
    var builder = Builder(bios: Bios(b => {
      b["SMBIOSBIOSVersion"] = new WmiValue("F.42");
      b["BIOSVersion"] = new WmiValue(new[] { "ALASKA - 1" });
      b["Version"] = new WmiValue("legacy");
    }));

    var snap = await builder.BuildAsync(default);

    Assert.Equal("F.42", snap.Version);
  }

  [Fact]
  public async Task Version_falls_through_to_biosversion_then_version() {
    var onlyBiosVersion = Builder(bios: Bios(b =>
        b["BIOSVersion"] = new WmiValue(new[] { "ALASKA - 1", "extra" })));
    var onlyVersion = Builder(bios: Bios(b => b["Version"] = new WmiValue("v9")));

    Assert.Equal("ALASKA - 1, extra", (await onlyBiosVersion.BuildAsync(default)).Version);
    Assert.Equal("v9", (await onlyVersion.BuildAsync(default)).Version);
  }

  [Fact]
  public async Task Version_falls_back_to_smbios_when_no_wmi_version() {
    var builder = Builder(smbios: Smbios(BiosInfo(version: "SMB-V")));

    Assert.Equal("SMB-V", (await builder.BuildAsync(default)).Version);
  }

  [Fact]
  public async Task SerialNumber_is_trimmed() {
    var builder = Builder(bios: Bios(b => b["SerialNumber"] = new WmiValue("  ABC123  ")));

    Assert.Equal("ABC123", (await builder.BuildAsync(default)).SerialNumber);
  }

  [Fact]
  public async Task PrimaryBios_and_Status_come_from_wmi() {
    var builder = Builder(bios: Bios(b => {
      b["PrimaryBIOS"] = new WmiValue(true);
      b["Status"] = new WmiValue("OK");
    }));

    var snap = await builder.BuildAsync(default);

    Assert.True(snap.PrimaryBios);
    Assert.Equal("OK", snap.Status);
  }

  // ─────────────────────────── SMBIOS-only firmware detail ───────────────────────────

  [Fact]
  public async Task Firmware_detail_projected_from_smbios() {
    var builder = Builder(smbios: Smbios(BiosInfo(
        romSize: 16 * 1024 * 1024, uefi: true, biosRevision: "5.19")));

    var snap = await builder.BuildAsync(default);

    Assert.Equal(16 * 1024 * 1024, snap.RomSizeBytes);
    Assert.True(snap.IsUefi);
    Assert.Equal("5.19", snap.BiosRevision);
  }

  [Fact]
  public async Task Capabilities_null_when_no_smbios_bios() {
    var builder = Builder(bios: Bios(b => b["Manufacturer"] = new WmiValue("X")));

    Assert.Null((await builder.BuildAsync(default)).Capabilities);
  }

  [Fact]
  public async Task Capabilities_projected_from_smbios_characteristics() {
    var builder = Builder(smbios: Smbios(BiosInfo(
        flash: true, selectableBoot: true, bootFromCd: false)));

    var caps = (await builder.BuildAsync(default)).Capabilities;

    Assert.NotNull(caps);
    Assert.True(caps!.FlashUpgradeable);
    Assert.True(caps.SelectableBoot);
    Assert.False(caps.BootFromCd);
  }

  // ─────────────────────────── EC revision (SMBIOS preferred, WMI fallback) ──────────

  [Fact]
  public async Task EcRevision_prefers_smbios() {
    var builder = Builder(
        bios: Bios(b => {
          b["EmbeddedControllerMajorVersion"] = new WmiValue("1");
          b["EmbeddedControllerMinorVersion"] = new WmiValue("2");
        }),
        smbios: Smbios(BiosInfo(ecRevision: "9.9")));

    Assert.Equal("9.9", (await builder.BuildAsync(default)).EmbeddedControllerRevision);
  }

  [Fact]
  public async Task EcRevision_falls_back_to_wmi_major_minor() {
    var builder = Builder(bios: Bios(b => {
      b["EmbeddedControllerMajorVersion"] = new WmiValue("1");
      b["EmbeddedControllerMinorVersion"] = new WmiValue("2");
    }));

    Assert.Equal("1.2", (await builder.BuildAsync(default)).EmbeddedControllerRevision);
  }

  [Fact]
  public async Task EcRevision_null_when_neither_source_has_it() {
    Assert.Null((await Builder().BuildAsync(default)).EmbeddedControllerRevision);
  }

  // ─────────────────────────── Spec version (WMI major.minor, else SMBIOS) ────────────

  [Fact]
  public async Task SpecVersion_prefers_wmi_major_minor() {
    var builder = Builder(
        bios: Bios(b => {
          b["SMBIOSMajorVersion"] = new WmiValue(3);
          b["SMBIOSMinorVersion"] = new WmiValue(2);
        }),
        smbios: Smbios(major: 9, minor: 9));

    Assert.Equal("3.2", (await builder.BuildAsync(default)).SmbiosSpecVersion);
  }

  [Fact]
  public async Task SpecVersion_falls_back_to_smbios_when_major_positive() {
    var builder = Builder(smbios: Smbios(major: 3, minor: 4));

    Assert.Equal("3.4", (await builder.BuildAsync(default)).SmbiosSpecVersion);
  }

  [Fact]
  public async Task SpecVersion_null_when_smbios_major_zero_and_no_wmi() {
    var builder = Builder(smbios: Smbios(major: 0, minor: 0));

    Assert.Null((await builder.BuildAsync(default)).SmbiosSpecVersion);
  }

  // ─────────────────────────── TPM merge ───────────────────────────

  [Fact]
  public async Task Tpm_absent_when_no_live_instance_and_no_descriptor() {
    var snap = await Builder().BuildAsync(default);

    Assert.Same(TpmInfo.Absent, snap.Tpm);
    Assert.False(snap.Tpm.Present);
  }

  [Fact]
  public async Task Tpm_present_from_live_instance() {
    var builder = Builder(tpm: new Dictionary<string, WmiValue> {
      ["InstanceName"] = new WmiValue("TPM_1"),
      ["IsEnabled_InitialValue"] = new WmiValue(true),
      ["IsActivated_InitialValue"] = new WmiValue(true),
      ["IsOwned_InitialValue"] = new WmiValue(false),
      ["SpecVersion"] = new WmiValue("2.0, 0, 1.38"),
      ["ManufacturerIdTxt"] = new WmiValue("  INTC  "),
    });

    var tpm = (await builder.BuildAsync(default)).Tpm;

    Assert.True(tpm.Present);
    Assert.True(tpm.Enabled);
    Assert.True(tpm.Activated);
    Assert.False(tpm.Owned);
    Assert.Equal("2.0", tpm.SpecVersion);      // NormalizeSpec keeps leading family number
    Assert.Equal("INTC", tpm.Manufacturer);     // trimmed
  }

  [Fact]
  public async Task Tpm_present_from_smbios_descriptor_only() {
    var builder = Builder(smbios: Smbios(tpm: new SmbiosTpmInfo(
        VendorId: "IFX", SpecVersion: "2.0", Description: "desc")));

    var tpm = (await builder.BuildAsync(default)).Tpm;

    Assert.True(tpm.Present);
    Assert.Equal("2.0", tpm.SpecVersion);
    Assert.Equal("IFX", tpm.Manufacturer);
  }

  [Fact]
  public async Task Tpm_spec_and_manufacturer_fall_back_to_descriptor() {
    // Live instance present (InstanceName) but no spec/manufacturer from live → descriptor fills in.
    var builder = Builder(
        tpm: new Dictionary<string, WmiValue> { ["InstanceName"] = new WmiValue("TPM_1") },
        smbios: Smbios(tpm: new SmbiosTpmInfo(
            VendorId: "IFX", SpecVersion: "2.0", Description: "d")));

    var tpm = (await builder.BuildAsync(default)).Tpm;

    Assert.Equal("2.0", tpm.SpecVersion);
    Assert.Equal("IFX", tpm.Manufacturer);
  }

  // ─────────────────────────── Secure Boot ───────────────────────────

  [Fact]
  public async Task SecureBoot_projected_from_provider() {
    var builder = Builder(secureBoot: new SecureBootState(Supported: true, Enabled: true));

    var sb = (await builder.BuildAsync(default)).SecureBoot;

    Assert.True(sb.Supported);
    Assert.True(sb.Enabled);
  }

  // ─────────────────────────── Graceful degradation ───────────────────────────

  [Fact]
  public async Task SecureBoot_unknown_when_provider_throws() {
    var builder = new FirmwareInfoBuilder(
        new FakeWmiProvider(),
        new FakeSmbiosProvider(null),
        FakeSecurityProvider.Throwing());

    var snap = await builder.BuildAsync(default);

    Assert.Same(SecureBootInfo.Unknown, snap.SecureBoot);
  }

  [Fact]
  public async Task Smbios_fields_null_when_smbios_throws_but_wmi_still_populates() {
    var builder = new FirmwareInfoBuilder(
        new FakeWmiProvider(bios: Bios(b => b["Manufacturer"] = new WmiValue("AMI"))),
        FakeSmbiosProvider.Throwing(),
        new FakeSecurityProvider(SecureBootState.Unknown));

    var snap = await builder.BuildAsync(default);

    Assert.Equal("AMI", snap.Manufacturer);   // WMI half survives
    Assert.Null(snap.RomSizeBytes);            // SMBIOS half degraded to null
    Assert.Null(snap.System);
    Assert.Empty(snap.FirmwareInventory);      // defaults to []
    Assert.Same(TpmInfo.Absent, snap.Tpm);
  }
}
