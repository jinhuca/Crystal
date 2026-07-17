using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Crystal.Smbios.Tests")]

namespace Crystal.Smbios.Types;

/// <summary>
/// Type 7 — describes one cache level (L1/L2/L3) associated with a processor
/// via <see cref="SmbiosRawStructure.Handle"/>, referenced from
/// <see cref="T004_ProcessorInformation.L1CacheHandle"/> /
/// <see cref="T004_ProcessorInformation.L2CacheHandle"/> /
/// <see cref="T004_ProcessorInformation.L3CacheHandle"/>.
/// </summary>
public sealed class T007_CacheInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  /// <summary>This structure's own handle — matched against Type 4's cache handle fields.</summary>
  public ushort Handle { get; init; }

  public string? SocketDesignation { get; init; }
  public CacheConfiguration Configuration { get; init; }

  /// <summary>Maximum cache size the system supports, in KiB.</summary>
  public long MaxSizeKiB { get; init; }
  /// <summary>Currently installed cache size, in KiB.</summary>
  public long InstalledSizeKiB { get; init; }

  public CacheSramType SupportedSramType { get; init; }
  public CacheSramType CurrentSramType { get; init; }

  /// <summary>Cache speed in nanoseconds; 0 = unknown (speed not provided).</summary>
  public byte SpeedNs { get; init; }
  public CacheErrorCorrectionType ErrorCorrectionType { get; init; }
  public SystemCacheType SystemCacheType { get; init; }
  public CacheAssociativity Associativity { get; init; }

  internal static T007_CacheInformation Decode(SmbiosRawStructure s) {
    // DSP0134 §7.8 formatted-area layout:
    // 04 SocketDesignation          STRING
    // 05 CacheConfiguration         WORD
    // 07 MaximumCacheSize           WORD  (legacy)
    // 09 InstalledSize              WORD  (legacy)
    // 0B SupportedSRAMType          WORD
    // 0D CurrentSRAMType            WORD
    // 0F CacheSpeed                 BYTE
    // 10 ErrorCorrectionType        BYTE
    // 11 SystemCacheType            BYTE
    // 12 Associativity              BYTE
    // 13 MaximumCacheSize2          DWORD (v3.1+, extended)
    // 17 InstalledSize2             DWORD (v3.1+, extended)
    ushort configRaw = s.ReadWord(0x05);
    ushort maxLegacy = s.ReadWord(0x07);
    ushort instLegacy = s.ReadWord(0x09);

    uint maxExtended = s.Length > 0x16 ? s.ReadDWord(0x13) : 0u;
    uint instExtended = s.Length > 0x1A ? s.ReadDWord(0x17) : 0u;

    return new T007_CacheInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      SocketDesignation = s.GetString(s.ReadByte(0x04)),
      Configuration = new CacheConfiguration(configRaw),
      MaxSizeKiB = CacheSizeDecoder.DecodeKiB(maxLegacy, maxExtended),
      InstalledSizeKiB = CacheSizeDecoder.DecodeKiB(instLegacy, instExtended),
      SupportedSramType = (CacheSramType)s.ReadWord(0x0B),
      CurrentSramType = (CacheSramType)s.ReadWord(0x0D),
      SpeedNs = s.ReadByte(0x0F),
      ErrorCorrectionType = (CacheErrorCorrectionType)s.ReadByte(0x10),
      SystemCacheType = (SystemCacheType)s.ReadByte(0x11),
      Associativity = (CacheAssociativity)s.ReadByte(0x12),
    };
  }
}

/// <summary>DSP0134 §7.8.1 — Cache Configuration bits 5-6 (Location).</summary>
public enum CacheLocation : byte {
  Internal = 0b00,
  External = 0b01,
  Reserved = 0b10,
  Unknown = 0b11,
}

/// <summary>DSP0134 §7.8.1 — Cache Configuration bits 8-9 (Operational Mode).</summary>
public enum CacheOperationalMode : byte {
  WriteThrough = 0b00,
  WriteBack = 0b01,
  VariesWithAddress = 0b10,
  Unknown = 0b11,
}

/// <summary>DSP0134 §7.8.2 — Cache SRAM Type bitfield (Supported / Current).</summary>
[Flags]
public enum CacheSramType : ushort {
  Other = 1 << 0,
  Unknown = 1 << 1,
  NonBurst = 1 << 2,
  Burst = 1 << 3,
  PipelineBurst = 1 << 4,
  Synchronous = 1 << 5,
  Asynchronous = 1 << 6,
}

/// <summary>DSP0134 §7.8.3 — Error Correction Type.</summary>
public enum CacheErrorCorrectionType : byte {
  Other = 0x01,
  Unknown = 0x02,
  None = 0x03,
  Parity = 0x04,
  SingleBitEcc = 0x05,
  MultiBitEcc = 0x06,
}

/// <summary>DSP0134 §7.8.4 — System Cache Type.</summary>
public enum SystemCacheType : byte {
  Other = 0x01,
  Unknown = 0x02,
  Instruction = 0x03,
  Data = 0x04,
  Unified = 0x05,
}

/// <summary>DSP0134 §7.8.5 — Associativity.</summary>
public enum CacheAssociativity : byte {
  Other = 0x01,
  Unknown = 0x02,
  DirectMapped = 0x03,
  TwoWay = 0x04,
  FourWay = 0x05,
  FullyAssociative = 0x06,
  EightWay = 0x07,
  SixteenWay = 0x08,
  TwelveWay = 0x09,
  TwentyFourWay = 0x0A,
  ThirtyTwoWay = 0x0B,
  FortyEightWay = 0x0C,
  SixtyFourWay = 0x0D,
  TwentyWay = 0x0E,
}

/// <summary>
/// Decodes the packed Cache Configuration WORD (DSP0134 §7.8.1, offset 0x05
/// of the Type 7 formatted area).
/// <code>
///   Bits 0-2  Cache Level (0-based; add 1 for the human-readable level)
///   Bit  3    Socketed (1) / Not socketed (0)
///   Bit  4    Reserved
///   Bits 5-6  Location (00=Internal, 01=External, 10=Reserved, 11=Unknown)
///   Bit  7    Enabled (1) / Disabled (0) at boot
///   Bits 8-9  Operational Mode (00=WriteThrough, 01=WriteBack,
///                                10=VariesWithAddress, 11=Unknown)
///   Bit  10   Reserved
/// </code>
/// </summary>
public readonly struct CacheConfiguration {
  private readonly ushort _raw;

  public CacheConfiguration(ushort raw) => _raw = raw;

  /// <summary>Raw packed value, in case a consumer needs bits this type doesn't expose.</summary>
  public ushort RawValue => _raw;

  /// <summary>Cache level: 1, 2, or 3 (stored 0-based in bits 0-2).</summary>
  public int Level => (_raw & 0b111) + 1;

  /// <summary>True if the cache module is in a socket (removable).</summary>
  public bool Socketed => (_raw & (1 << 3)) != 0;

  /// <summary>Whether the cache is internal or external to the processor module.</summary>
  public CacheLocation Location => (CacheLocation)((_raw >> 5) & 0b11);

  /// <summary>Whether the cache was enabled at boot time.</summary>
  public bool EnabledAtBoot => (_raw & (1 << 7)) != 0;

  /// <summary>Write policy of the cache.</summary>
  public CacheOperationalMode OperationalMode => (CacheOperationalMode)((_raw >> 8) & 0b11);

  public override string ToString() =>
      $"L{Level}, {Location}, {(EnabledAtBoot ? "Enabled" : "Disabled")}, {OperationalMode}";
}

/// <summary>
/// Decodes a cache size pair (legacy WORD + v3.1 extended DWORD) into KiB,
/// per DSP0134 §7.8.6 (Max Cache Size) / §7.8.7 (Installed Size).
///
/// Both fields use the same bit-as-granularity-selector encoding:
///   legacy:   bit 15 = granularity (0 = 1K, 1 = 64K); bits 14-0 = value
///   extended: bit 31 = granularity (0 = 1K, 1 = 64K); bits 30-0 = value
/// The extended field is only meaningful when legacy == 0xFFFF.
/// </summary>
internal static class CacheSizeDecoder {
  /// <summary>
  /// Returns the decoded size in KiB. Falls back to the legacy WORD unless
  /// it is the 0xFFFF sentinel, in which case the extended DWORD is used.
  /// </summary>
  public static long DecodeKiB(ushort legacy, uint extended) {
    if (legacy == 0xFFFF && extended != 0) {
      uint value = extended & 0x7FFFFFFF;
      bool granularity64K = (extended & 0x80000000) != 0;
      return granularity64K ? value * 64L : value;
    }

    uint legacyValue = (uint)(legacy & 0x7FFF);
    bool legacyGranularity64K = (legacy & 0x8000) != 0;
    return legacyGranularity64K ? legacyValue * 64L : legacyValue;
  }
}
