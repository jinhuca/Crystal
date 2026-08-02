namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 19 — Memory Array Mapped Address (DSP0134 §7.18)
/// </summary>
public sealed class T019_MemoryArrayMappedAddress : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>Start address in KiB.</summary>
  public long StartAddressKiB { get; init; }

  /// <summary>End address in KiB.</summary>
  public long EndAddressKiB { get; init; }

  /// <summary>Handle of the parent PhysicalMemoryArray (Type 16).</summary>
  public ushort MemoryArrayHandle { get; init; }

  /// <summary>Number of partitions (width) for interleaved arrays.</summary>
  public byte PartitionWidth { get; init; }

  /// <summary>Raw start address in bytes (when extended fields used this is the QWORD value; otherwise legacy DWORD * 1024).</summary>
  public ulong StartAddressBytes { get; init; }

  /// <summary>Raw end address in bytes (same semantics as StartAddressBytes).</summary>
  public ulong EndAddressBytes { get; init; }

  /// <summary>True when the extended QWORD fields were used to determine addresses.</summary>
  public bool UsesExtendedAddresses { get; init; }

  /// <summary>Computed address range size in bytes (inclusive).</summary>
  public ulong SizeBytes => EndAddressBytes >= StartAddressBytes ? (EndAddressBytes - StartAddressBytes + 1UL) : 0UL;

  /// <summary>Computed size in KiB (rounded down).</summary>
  public long SizeKiB => (long)(SizeBytes / 1024UL);

  /// <summary>True when this address range is interleaved across multiple partitions.</summary>
  public bool IsInterleaved => PartitionWidth > 1;

  internal static T019_MemoryArrayMappedAddress Decode(SmbiosRawStructure s) {
    // DSP0134 §7.20: Starting Address DWORD @0x04, Ending Address DWORD @0x08
    // (both in KiB); Memory Array Handle WORD @0x0C; Partition Width BYTE @0x0E;
    // Extended Starting/Ending Address QWORDs (bytes) @0x0F/0x17 (v2.7), used
    // when a legacy address field is 0xFFFFFFFF.
    uint startLegacy = s.ReadDWord(0x04);
    uint endLegacy = s.ReadDWord(0x08);

    ulong startBytes = (ulong)startLegacy * 1024UL;
    ulong endBytes = (ulong)endLegacy * 1024UL;
    bool usesExtended = false;

    if (startLegacy == 0xFFFFFFFF || endLegacy == 0xFFFFFFFF) {
      // Guard must cover the *end* QWORD (0x17-0x1E), which is stricter than the start QWORD (0x0F-0x16).
      usesExtended = s.Length > 0x1E;
      if (usesExtended) {
        startBytes = s.ReadQWord(0x0F);
        endBytes = s.ReadQWord(0x17);
      }
    }

    byte partitionWidth = s.Length > 0x0E ? s.ReadByte(0x0E) : (byte)0;

    return new T019_MemoryArrayMappedAddress {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      StartAddressKiB = (long)(startBytes / 1024UL),
      EndAddressKiB = (long)(endBytes / 1024UL),
      MemoryArrayHandle = s.ReadWord(0x0C),
      PartitionWidth = partitionWidth,
      StartAddressBytes = startBytes,
      EndAddressBytes = endBytes,
      UsesExtendedAddresses = usesExtended,
    };
  }
}
