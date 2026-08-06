using Crystal.Provider.Mmi.HardwareFeatures.Bios;
using Crystal.Provider.Mmi.MmiEngine;

namespace BiosModule.Models;

/// <summary>Builds the static BIOS identity from WMI (<c>Win32_BIOS</c>).</summary>
public sealed class BiosInfoBuilder {
  private readonly IWmiHardwareProvider _wmi;

  public BiosInfoBuilder(IWmiHardwareProvider wmi) => _wmi = wmi;

  public async Task<BiosSnapshot> BuildAsync(CancellationToken ct) {
    var b = await _wmi.ToSafeBiosMetricsAsync(ct);

    return new BiosSnapshot(
        Manufacturer: b.Manufacturer,
        Version: b.SMBIOSBIOSVersion ?? b.BIOSVersion ?? b.Version,
        SmbiosVersion: b.SMBIOSBIOSVersion,
        ReleaseDate: b.ReleaseDate,
        SerialNumber: b.SerialNumber?.Trim(),
        SmbiosSpecVersion: b.SMBIOSMajorVersion is { } major && b.SMBIOSMinorVersion is { } minor
            ? $"{major}.{minor}"
            : null,
        PrimaryBios: b.PrimaryBIOS,
        Status: b.Status);
  }
}
