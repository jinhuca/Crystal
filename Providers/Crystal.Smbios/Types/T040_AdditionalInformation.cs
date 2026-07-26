using System;
using System.Collections.Generic;

namespace Crystal.Smbios.Types;

/// <summary>
/// One entry within a Type 40 Additional Information structure: a field
/// override/extension for another structure's referenced offset.
/// </summary>
public sealed class AdditionalInformationEntry {
  /// <summary>Handle of the structure this entry refers to.</summary>
  public ushort ReferencedHandle { get; init; }
  /// <summary>Byte offset within the referenced structure's formatted area.</summary>
  public byte ReferencedOffset { get; init; }
  /// <summary>Human-readable description of the field.</summary>
  public string? EntryString { get; init; }
  /// <summary>The replacement/extension value's raw bytes (1, 2, or 4 bytes per spec).</summary>
  public IReadOnlyList<byte> Value { get; init; } = Array.Empty<byte>();
}

/// <summary>
/// Type 40 — Additional Information (DSP0134 §7.41).
/// Provides overrides/extensions for fields in other structures whose
/// spec-defined enumeration is insufficient, without requiring a new
/// structure type. Each entry references another structure's handle and
/// formatted-area offset.
/// </summary>
public sealed class T040_AdditionalInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public byte NumberOfAdditionalInformationEntries { get; init; }
  public IReadOnlyList<AdditionalInformationEntry> Entries { get; init; } = Array.Empty<AdditionalInformationEntry>();

  internal static T040_AdditionalInformation Decode(SmbiosRawStructure s) {
    byte count = s.Length > 0x04 ? s.ReadByte(0x04) : (byte)0;
    var entries = new List<AdditionalInformationEntry>(count);
    int offset = 0x05;

    for (int i = 0; i < count; i++) {
      if (s.Length < offset + 1) break;
      byte entryLength = s.ReadByte(offset);
      if (entryLength < 0x05 || s.Length < offset + entryLength) break;

      int valueLength = entryLength - 0x05;
      var value = new byte[valueLength];
      for (int b = 0; b < valueLength; b++)
        value[b] = s.ReadByte(offset + 0x05 + b);

      entries.Add(new AdditionalInformationEntry {
        ReferencedHandle = s.ReadWord(offset + 0x01),
        ReferencedOffset = s.ReadByte(offset + 0x03),
        EntryString = s.GetString(s.ReadByte(offset + 0x04)),
        Value = value,
      });

      offset += entryLength;
    }

    return new T040_AdditionalInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      NumberOfAdditionalInformationEntries = count,
      Entries = entries,
    };
  }
}
