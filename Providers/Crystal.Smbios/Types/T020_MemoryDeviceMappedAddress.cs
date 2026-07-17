namespace Crystal.Smbios.Types;

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
    public byte PartitionRowPosition { get; init; }

    public ulong StartAddressBytes { get; init; }
    public ulong EndAddressBytes { get; init; }
    public bool UsesExtendedAddresses { get; init; }

    public ulong SizeBytes => EndAddressBytes >= StartAddressBytes ? (EndAddressBytes - StartAddressBytes + 1UL) : 0UL;
    public long SizeKiB => (long)(SizeBytes / 1024UL);

    /// <summary>Optional interleave granularity in bytes (when present at offset 0x11 as a DWORD).</summary>
    public ulong? InterleaveGranularityBytes { get; init; }

    /// <summary>Optional interleave granularity in KiB (rounded down), derived from InterleaveGranularityBytes.</summary>
    public long? InterleaveGranularityKiB => InterleaveGranularityBytes is null ? null : (long?)(InterleaveGranularityBytes.Value / 1024UL);

    /// <summary>Optional interleave-row/column metadata: the interleave position within the row (if present at 0x0F).</summary>
    public byte? InterleavePosition { get; init; }

    /// <summary>Optional interleave-column (if present in newer spec variants).</summary>
    public byte? InterleaveColumn { get; init; }

    internal static T020_MemoryDeviceMappedAddress Decode(SmbiosRawStructure s)
    {
        uint startLegacy = s.ReadDWord(0x04);
        uint endLegacy   = s.ReadDWord(0x08);

        ulong startBytes = (ulong)startLegacy * 1024UL;
        ulong endBytes = (ulong)endLegacy * 1024UL;
        bool usesExtended = false;

        if (startLegacy == 0xFFFFFFFF || endLegacy == 0xFFFFFFFF)
        {
            // Guard must cover the *end* QWORD (0x18-0x1F), which is stricter than the start QWORD (0x10-0x17).
            usesExtended = s.Length > 0x1F;
            if (usesExtended)
            {
                startBytes = s.ReadQWord(0x10);
                endBytes = s.ReadQWord(0x18);
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
            PartitionRowPosition = s.Length > 0x0E ? s.ReadByte(0x0E) : (byte)0,
            InterleavePosition = s.Length > 0x0F ? s.ReadByte(0x0F) : (byte?)null,
            InterleaveColumn = s.Length > 0x10 ? s.ReadByte(0x10) : (byte?)null,
            InterleaveGranularityBytes = s.Length > 0x14 ? s.ReadDWord(0x11) : (ulong?)null,
            StartAddressBytes = startBytes,
            EndAddressBytes = endBytes,
            UsesExtendedAddresses = usesExtended,
        };
    }
}
