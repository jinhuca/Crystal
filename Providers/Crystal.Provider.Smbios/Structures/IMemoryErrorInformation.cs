namespace Crystal.Provider.Smbios.Structures;

/// <summary>
/// Common interface for Memory Error Information (Type 18 / Type 33) entries.
/// Provides a union view of the fields present in either 32-bit or 64-bit variants.
/// </summary>
public interface IMemoryErrorInformation
{
    byte ErrorType { get; }
    byte ErrorGranularity { get; }
    /// <summary>Memory error operation being performed when the error occurred (DSP0134 §7.19.3).</summary>
    byte ErrorOperation { get; }
    /// <summary>Vendor-specific ECC syndrome; 0 when unknown. Width varies; always exposed as ulong.</summary>
    ulong VendorSyndrome { get; }
    /// <summary>Physical address of the error relative to the start of the Memory Array. Expressed as ulong.</summary>
    ulong MemoryArrayErrorAddress { get; }
    /// <summary>Physical address of the error relative to the start of the Memory Device. Expressed as ulong.</summary>
    ulong DeviceErrorAddress { get; }
    /// <summary>Range within which this error can be determined, when detected. Expressed as ulong.</summary>
    ulong ErrorResolution { get; }
    /// <summary>True when this instance represents the 64-bit Type 33 variant.</summary>
    bool Is64Bit { get; }
}
