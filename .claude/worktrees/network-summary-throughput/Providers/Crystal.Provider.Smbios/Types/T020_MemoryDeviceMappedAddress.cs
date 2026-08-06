namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 20 — Memory Device Mapped Address (DSP0134 §7.19)
/// </summary>
public sealed class T020_MemoryDeviceMappedAddress : ISmbiosDecodedStructure
{
    public SmbiosStructureType StructureType { get; init; }
    public byte Length { get; init; }
    public ushort Handle { get; init; }
    public long StartAddressKiB { get; init; }
    public long EndAddressKiB { get; init; }
    public ushort MemoryDeviceHandle { get; init; }

    /// <summary>Handle of the parent Memory Array Mapped Address (Type 19) structure.</summary>
    public ushort MemoryArrayMappedAddressHandle { get; init; }

    /// <summary>Position of this device in a row of the address partition (1-based); 0xFF = unknown, 0 = one device per row.</summary>
    public byte PartitionRowPosition { get; init; }

    public ulong StartAddressBytes { get; init; }
    public ulong EndAddressBytes { get; init; }
    public bool UsesExtendedAddresses { get; init; }

    public ulong SizeBytes => EndAddressBytes >= StartAddressBytes ? (EndAddressBytes - StartAddressBytes + 1UL) : 0UL;
    public long SizeKiB => (long)(SizeBytes / 1024UL);

    /// <summary>Position of the referenced device in an interleave (1-based); 0 = non-interleaved, 0xFF = unknown.</summary>
    public byte InterleavePosition { get; init; }

    /// <summary>Maximum number of consecutive rows accessed in a single interleaved transfer; 0 = unknown, 0xFF = a multiple of the row size.</summary>
    public byte InterleavedDataDepth { get; init; }

    internal static T020_MemoryDeviceMappedAddress Decode(SmbiosRawStructure s)
    {
        // DSP0134 §7.21: Starting/Ending Address DWORDs (KiB) @0x04/0x08;
        // Memory Device Handle @0x0C; Memory Array Mapped Address Handle @0x0E;
        // Partition Row Position @0x10; Interleave Position @0x11;
        // Interleaved Data Depth @0x12; Extended Starting/Ending Address QWORDs
        // (bytes) @0x13/0x1B (v2.7).
        uint startLegacy = s.ReadDWord(0x04);
        uint endLegacy   = s.ReadDWord(0x08);

        ulong startBytes = (ulong)startLegacy * 1024UL;
        ulong endBytes = (ulong)endLegacy * 1024UL;
        bool usesExtended = false;

        if (startLegacy == 0xFFFFFFFF || endLegacy == 0xFFFFFFFF)
        {
            // Guard must cover the *end* QWORD (0x1B-0x22), which is stricter than the start QWORD (0x13-0x1A).
            usesExtended = s.Length > 0x22;
            if (usesExtended)
            {
                startBytes = s.ReadQWord(0x13);
                endBytes = s.ReadQWord(0x1B);
            }
        }

        return new T020_MemoryDeviceMappedAddress
        {
            StructureType = s.Type,
            Length = s.Length,
            Handle = s.Handle,
            StartAddressKiB = (long)(startBytes / 1024UL),
            EndAddressKiB = (long)(endBytes / 1024UL),
            MemoryDeviceHandle = s.ReadWord(0x0C),
            MemoryArrayMappedAddressHandle = s.Length > 0x0F ? s.ReadWord(0x0E) : (ushort)0xFFFF,
            PartitionRowPosition = s.Length > 0x10 ? s.ReadByte(0x10) : (byte)0,
            InterleavePosition = s.Length > 0x11 ? s.ReadByte(0x11) : (byte)0,
            InterleavedDataDepth = s.Length > 0x12 ? s.ReadByte(0x12) : (byte)0,
            StartAddressBytes = startBytes,
            EndAddressBytes = endBytes,
            UsesExtendedAddresses = usesExtended,
        };
    }
}
