namespace BiosModule.Models;

/// <summary>Static BIOS/firmware identity from WMI (<c>Win32_BIOS</c>).</summary>
public record BiosSnapshot(
    string? Manufacturer,
    string? Version,
    string? SmbiosVersion,
    string? ReleaseDate,
    string? SerialNumber,
    string? SmbiosSpecVersion,
    bool? PrimaryBios,
    string? Status);
