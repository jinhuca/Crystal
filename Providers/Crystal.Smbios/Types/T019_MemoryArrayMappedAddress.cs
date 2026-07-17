namespace Crystal.Smbios.Types;

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

  /// <summary>
  /// Position within the interleave set (when present). Optional — only
  /// available when the formatted area contains the interleave-position byte
  /// at offset 0x0F.
  /// </summary>
  public byte? InterleavePosition { get; init; }

  /// <summary>Optional interleave granularity in bytes (when present at offset 0x11 as a DWORD).</summary>
  public ulong? InterleaveGranularityBytes { get; init; }

  /// <summary>Optional interleave granularity in KiB (rounded down), derived from InterleaveGranularityBytes.</summary>
  public long? InterleaveGranularityKiB => InterleaveGranularityBytes is null ? null : (long?)(InterleaveGranularityBytes.Value / 1024UL);

  internal static T019_MemoryArrayMappedAddress Decode(SmbiosRawStructure s) {
    // Legacy fields are DWORDs in KiB; 0xFFFFFFFF indicates use of extended QWORDs at 0x10 (bytes).
    uint startLegacy = s.ReadDWord(0x04);
    uint endLegacy = s.ReadDWord(0x08);

    long startKiB;
    long endKiB;

    ulong startBytes = (ulong)startLegacy * 1024UL;
    ulong endBytes = (ulong)endLegacy * 1024UL;
    bool usesExtended = false;

    if (startLegacy == 0xFFFFFFFF || endLegacy == 0xFFFFFFFF) {
      // Extended fields are present at offsets 0x10 (start) and 0x18 (end) as QWORDs in BYTES.
      // Guard must cover the *end* QWORD (0x18-0x1F), which is stricter than the start QWORD (0x10-0x17).
      usesExtended = s.Length > 0x1F;
      if (usesExtended) {
        startBytes = s.ReadQWord(0x10);
        endBytes = s.ReadQWord(0x18);
      }
    }

    byte partitionWidth = s.Length > 0x0E ? s.ReadByte(0x0E) : (byte)0;
    byte? interleavePos = (s.Length > 0x0F && partitionWidth > 0) ? s.ReadByte(0x0F) : (byte?)null;
    ulong? interleaveGran = (s.Length > 0x14 && partitionWidth > 0) ? s.ReadDWord(0x11) : (ulong?)null;

    return new T019_MemoryArrayMappedAddress {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      StartAddressKiB = (long)(startBytes / 1024UL),
      EndAddressKiB = (long)(endBytes / 1024UL),
      MemoryArrayHandle = s.ReadWord(0x0C),
      PartitionWidth = partitionWidth,
      InterleavePosition = interleavePos,
      InterleaveGranularityBytes = interleaveGran,
      StartAddressBytes = startBytes,
      EndAddressBytes = endBytes,
      UsesExtendedAddresses = usesExtended,
    };
  }
}
