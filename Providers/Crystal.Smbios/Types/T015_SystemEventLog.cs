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

  /// <summary>Length in bytes of the overall event-log area (0x04 WORD).</summary>
  public ushort LogAreaLength { get; init; }

  /// <summary>Byte offset within the log area of the first byte of the log header (0x06 WORD).</summary>
  public ushort LogHeaderStartOffset { get; init; }

  /// <summary>Byte offset within the log area of the first byte of log data (0x08 WORD).</summary>
  public ushort LogDataStartOffset { get; init; }

  /// <summary>Access method used to retrieve the log (0x0A BYTE).</summary>
  public byte AccessMethod { get; init; }

  /// <summary>Current status of the log: bits 0 (valid) and 1 (full) (0x0B BYTE).</summary>
  public byte LogStatus { get; init; }

  /// <summary>Whether the log area is valid (LogStatus bit 0).</summary>
  public bool IsValid => (LogStatus & 0x01) != 0;
  /// <summary>Whether the log area is full (LogStatus bit 1).</summary>
  public bool IsFull => (LogStatus & 0x02) != 0;

  /// <summary>Unique token updated each time the log changes (0x0C DWORD).</summary>
  public uint LogChangeToken { get; init; }

  /// <summary>Address associated with the access method (0x10 DWORD); interpretation depends on <see cref="AccessMethod"/>.</summary>
  public uint AccessMethodAddress { get; init; }

  /// <summary>Format of the log header (0x14 BYTE, v2.1+).</summary>
  public byte LogHeaderFormat { get; init; }

  /// <summary>Number of supported event-log type descriptors that follow (0x15 BYTE, v2.1+).</summary>
  public byte SupportedLogTypeDescriptorCount { get; init; }

  /// <summary>Length in bytes of each event-log type descriptor (0x16 BYTE, v2.1+).</summary>
  public byte LogTypeDescriptorLength { get; init; }

  /// <summary>Raw supported event-log type descriptor list (0x17+, v2.1+); each entry is <see cref="LogTypeDescriptorLength"/> bytes.</summary>
  public ReadOnlyMemory<byte> LogTypeDescriptors { get; init; }

  /// <summary>Raw formatted-area bytes for callers that need to parse the full log area.</summary>
  public ReadOnlyMemory<byte> FormattedArea { get; init; }

  internal static T015_SystemEventLog Decode(SmbiosRawStructure s) {
    // DSP0134 §7.16 formatted-area layout.
    byte descCount = s.Length > 0x15 ? s.ReadByte(0x15) : (byte)0;
    byte descLen = s.Length > 0x16 ? s.ReadByte(0x16) : (byte)0;

    ReadOnlyMemory<byte> descriptors = ReadOnlyMemory<byte>.Empty;
    int descBytes = descCount * descLen;
    if (descBytes > 0 && s.Length >= 0x17 + descBytes)
      descriptors = s.FormattedArea.Slice(0x17, descBytes);

    return new T015_SystemEventLog {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      LogAreaLength = s.Length > 0x05 ? s.ReadWord(0x04) : (ushort)0,
      LogHeaderStartOffset = s.Length > 0x07 ? s.ReadWord(0x06) : (ushort)0,
      LogDataStartOffset = s.Length > 0x09 ? s.ReadWord(0x08) : (ushort)0,
      AccessMethod = s.Length > 0x0A ? s.ReadByte(0x0A) : (byte)0,
      LogStatus = s.Length > 0x0B ? s.ReadByte(0x0B) : (byte)0,
      LogChangeToken = s.Length > 0x0F ? s.ReadDWord(0x0C) : 0u,
      AccessMethodAddress = s.Length > 0x13 ? s.ReadDWord(0x10) : 0u,
      LogHeaderFormat = s.Length > 0x14 ? s.ReadByte(0x14) : (byte)0,
      SupportedLogTypeDescriptorCount = descCount,
      LogTypeDescriptorLength = descLen,
      LogTypeDescriptors = descriptors,
      FormattedArea = s.FormattedArea,
    };
  }
}