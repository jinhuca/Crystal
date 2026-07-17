namespace Crystal.Smbios.Types;

using System;

/// <summary>
/// Type 15 — System Event Log (DSP0134 §7.15) — minimal safe decode.
/// This provides the common formatted-area fields in a defensive way so the
/// rest of the code can inspect event-log metadata without risking index errors.
/// </summary>
public sealed class T015_SystemEventLog : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>
  /// Length in bytes of the log-area (may be 0).
  /// </summary>
  public ushort LogAreaLength { get; init; }

  /// <summary>
  /// Offset within the log-area where the log header starts (in bytes).
  /// </summary>
  public ushort LogHeaderStartOffset { get; init; }

  /// <summary>
  /// Log header format (per spec).
  /// </summary>
  public byte LogHeaderFormat { get; init; }

  /// <summary>
  /// Length in bytes of the log header.
  /// </summary>
  public byte LogHeaderLength { get; init; }

  /// <summary>
  /// Access method (byte) — interpretation depends on LogHeaderFormat.
  /// </summary>
  public byte AccessMethod { get; init; }

  /// <summary>
  /// Raw formatted-area bytes for callers that need to parse the full log area.
  /// </summary>
  public ReadOnlyMemory<byte> FormattedArea { get; init; }

  internal static T015_SystemEventLog Decode(SmbiosRawStructure s) {
    // Offsets are read defensively based on available formatted-area length.
    var logAreaLength = s.Length > 0x05 ? s.ReadWord(0x04) : (ushort)0;
    var headerStart = s.Length > 0x07 ? s.ReadWord(0x06) : (ushort)0;
    var headerFormat = s.Length > 0x08 ? s.ReadByte(0x08) : (byte)0;
    var headerLen = s.Length > 0x09 ? s.ReadByte(0x09) : (byte)0;
    var accessMethod = s.Length > 0x0A ? s.ReadByte(0x0A) : (byte)0;

    return new T015_SystemEventLog {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      LogAreaLength = logAreaLength,
      LogHeaderStartOffset = headerStart,
      LogHeaderFormat = headerFormat,
      LogHeaderLength = headerLen,
      AccessMethod = accessMethod,
      FormattedArea = s.FormattedArea,
    };
  }
}