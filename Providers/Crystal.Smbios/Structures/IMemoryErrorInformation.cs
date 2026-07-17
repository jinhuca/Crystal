namespace CrystalMonitorLib.Smbios.Structures;

/// <summary>
/// Common interface for Memory Error Information (Type 18 / Type 33) entries.
/// Provides a union view of the fields present in either 32-bit or 64-bit variants.
/// </summary>
public interface IMemoryErrorInformation
{
    byte ErrorType { get; }
    byte ErrorGranularity { get; }
    /// <summary>Vendor-specific syndrome value when present (width varies). Always exposed as ulong.</summary>
    ulong VendorSyndrome { get; }
    ushort MemoryArrayHandle { get; }
    ushort DeviceHandle { get; }
    /// <summary>Physical address where the error occurred, when present. Width varies; expressed as ulong.</summary>
    ulong PhysicalAddress { get; }
    /// <summary>Address resolution / mask when present. Expressed as ulong.</summary>
    ulong AddressResolution { get; }
    /// <summary>True when this instance represents the 64-bit Type 33 variant.</summary>
    bool Is64Bit { get; }
}
