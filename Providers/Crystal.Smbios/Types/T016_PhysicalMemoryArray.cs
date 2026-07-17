namespace Crystal.Smbios.Types;

/// <summary>
/// Type 16 — describes one physically-addressable memory array
/// (e.g. all DIMM slots on the system board) and its aggregate
/// capacity/error-correction properties. Parent of Type 17 (Memory Device)
/// records via <see cref="SmbiosRawStructure.Handle"/> ↔
/// <see cref="T017_MemoryDevice.PhysicalArrayHandle"/>.
/// </summary>
public sealed class T016_PhysicalMemoryArray : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  /// <summary>This structure's own handle — matched against MemoryDevice.PhysicalArrayHandle.</summary>
  public ushort Handle { get; init; }

  public MemoryArrayLocation Location { get; init; }
  public MemoryArrayUse Use { get; init; }
  public MemoryErrorCorrection ErrorCorrection { get; init; }

  /// <summary>
  /// Maximum capacity the array can hold, in KiB.
  /// Legacy field is capped at 0x80000000 (2 TiB); if reached, the
  /// v2.7 extended QWORD at offset 0x0F holds the real value — in BYTES,
  /// unlike every other legacy/extended pair in the spec, which stays in
  /// the same unit as the legacy field (DSP0134 §7.17.4).
  /// </summary>
  public long MaxCapacityKiB { get; init; }

  /// <summary>
  /// Handle of the associated Memory Error Information structure
  /// (Type 18/33); 0xFFFE = none provided, 0xFFFF = disabled.
  /// </summary>
  public ushort MemoryErrorInformationHandle { get; init; }

  /// <summary>Number of memory-device slots (Type 17 entries) that belong to this array.</summary>
  public ushort NumberOfMemoryDevices { get; init; }

  internal static T016_PhysicalMemoryArray Decode(SmbiosRawStructure s) {
    // DSP0134 §7.17 formatted-area layout:
    // 04 Location                     BYTE
    // 05 Use                          BYTE
    // 06 MemoryErrorCorrection        BYTE
    // 07 MaximumCapacity              DWORD  (KiB; 0x80000000 = "see extended")
    // 0B MemoryErrorInformationHandle WORD
    // 0D NumberOfMemoryDevices        WORD
    // 0F ExtendedMaximumCapacity      QWORD  (v2.7+, BYTES — only if legacy == 0x80000000)
    uint legacyCapacity = s.ReadDWord(0x07);

    long maxCapacityKiB = legacyCapacity == 0x80000000 && s.Length >= 0x17
        ? (long)(s.ReadQWord(0x0F) / 1024)   // extended field is in BYTES, convert to KiB
        : legacyCapacity;

    return new T016_PhysicalMemoryArray {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Location = (MemoryArrayLocation)s.ReadByte(0x04),
      Use = (MemoryArrayUse)s.ReadByte(0x05),
      ErrorCorrection = (MemoryErrorCorrection)s.ReadByte(0x06),
      MaxCapacityKiB = maxCapacityKiB,
      MemoryErrorInformationHandle = s.ReadWord(0x0B),
      NumberOfMemoryDevices = s.ReadWord(0x0D),
    };
  }
}

/// <summary>DSP0134 §7.17.1 — Location.</summary>
public enum MemoryArrayLocation : byte {
  Other = 0x01,
  Unknown = 0x02,
  SystemBoard = 0x03,
  IsaAddOnCard = 0x04,
  EisaAddOnCard = 0x05,
  PciAddOnCard = 0x06,
  McaAddOnCard = 0x07,
  PcmciaAddOnCard = 0x08,
  ProprietaryAddOnCard = 0x09,
  NuBus = 0x0A,
  PC98C20AddOnCard = 0xA0,
  PC98C24AddOnCard = 0xA1,
  PC98EAddOnCard = 0xA2,
  PC98LocalBusAddOnCard = 0xA3,
  Cxl = 0xA4,
}

/// <summary>DSP0134 §7.17.2 — Use.</summary>
public enum MemoryArrayUse : byte {
  Other = 0x01,
  Unknown = 0x02,
  SystemMemory = 0x03,
  VideoMemory = 0x04,
  FlashMemory = 0x05,
  NonVolatileRam = 0x06,
  CacheMemory = 0x07,
}

/// <summary>DSP0134 §7.17.3 — Memory Error Correction.</summary>
public enum MemoryErrorCorrection : byte {
  Other = 0x01,
  Unknown = 0x02,
  None = 0x03,
  Parity = 0x04,
  SingleBitEcc = 0x05,
  MultiBitEcc = 0x06,
  Crc = 0x07,
}
