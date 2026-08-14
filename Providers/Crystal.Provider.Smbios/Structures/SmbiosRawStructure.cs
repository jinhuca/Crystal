using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Provider.Smbios.Structures;

/// <summary>
/// Represents a single raw SMBIOS structure together with its decoded string table.
/// DSP0134 §6.1.2 — each structure is a fixed-length formatted area followed by
/// a variable-length string table terminated by a double-null (00 00).
/// </summary>
public class SmbiosRawStructure {
  // ── Header fields (DSP0134 Table 3) ──────────────────────────────────────
  /// <summary>Structure type (§7).</summary>
  public SmbiosStructureType Type { get; }

  /// <summary>
  /// Length of the formatted area including the 4-byte header.
  /// Does NOT include the string table.
  /// </summary>
  public byte Length { get; }

  /// <summary>Firmware-assigned handle unique within the SMBIOS table.</summary>
  public ushort Handle { get; }

  // ── Raw bytes ─────────────────────────────────────────────────────────────
  /// <summary>
  /// The full formatted area bytes (length == <see cref="Length"/>).
  /// Offset 0 = Type, 1 = Length, 2-3 = Handle, 4+ = structure-specific data.
  /// </summary>
  public ReadOnlyMemory<byte> FormattedArea { get; }

  /// <summary>
  /// Decoded string table.  Index is 1-based per the SMBIOS spec —
  /// string number 0 means "not present" and maps to <see langword="null"/>.
  /// </summary>
  public IReadOnlyList<string> Strings { get; }

  internal SmbiosRawStructure(
      SmbiosStructureType type,
      byte length,
      ushort handle,
      ReadOnlyMemory<byte> formattedArea,
      IReadOnlyList<string> strings) {
    Type = type;
    Length = length;
    Handle = handle;
    FormattedArea = formattedArea;
    Strings = strings;
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  /// <summary>
  /// Returns the string referenced by a 1-based string number field,
  /// or <see langword="null"/> if the number is 0 (not present).
  /// </summary>
  public string? GetString(byte stringNumber)
      => stringNumber == 0 || stringNumber > Strings.Count
          ? null
          : Strings[stringNumber - 1];

  /// <summary>Reads a byte at the given absolute offset within the formatted area.</summary>
  public byte ReadByte(int offset) => FormattedArea.Span[offset];

  /// <summary>Reads a little-endian WORD (UInt16) at the given absolute offset.</summary>
  public ushort ReadWord(int offset) {
    var span = FormattedArea.Span;
    return (ushort)(span[offset] | (span[offset + 1] << 8));
  }

  /// <summary>Reads a little-endian DWORD (UInt32) at the given absolute offset.</summary>
  public uint ReadDWord(int offset) {
    var span = FormattedArea.Span;
    return (uint)(span[offset]
                | (span[offset + 1] << 8)
                | (span[offset + 2] << 16)
                | (span[offset + 3] << 24));
  }

  /// <summary>Reads a little-endian QWORD (UInt64) at the given absolute offset.</summary>
  public ulong ReadQWord(int offset) {
    return (ulong)ReadDWord(offset) | ((ulong)ReadDWord(offset + 4) << 32);
  }

  /// <summary>Reads a 16-byte UUID at the given absolute offset (RFC 4122 byte order).</summary>
  public Guid ReadGuid(int offset) {
    // DSP0134 §7.2.1 — only the first three components are little-endian;
    // the last 8 bytes are big-endian.
    var span = FormattedArea.Span.Slice(offset, 16);
    return new Guid(
        (uint)(span[0] | (span[1] << 8) | (span[2] << 16) | (span[3] << 24)),
        (ushort)(span[4] | (span[5] << 8)),
        (ushort)(span[6] | (span[7] << 8)),
        span[8], span[9], span[10], span[11],
        span[12], span[13], span[14], span[15]);
  }

  public override string ToString() =>
      $"Type={Type} (0x{(byte)Type:X2}), Handle=0x{Handle:X4}, Length={Length}, Strings={Strings.Count}";
}

/// <summary>
/// Parses a raw SMBIOS firmware-table blob into <see cref="SmbiosRawStructure"/> records.
/// Supports both SMBIOS 2.x (32-bit entry point) and 3.x (64-bit entry point) table layouts.
/// </summary>
public static class SmbiosTableParser {
  private static readonly Encoding Latin1 = Encoding.Latin1;

  /// <summary>
  /// Parses the structure table region (not the entry-point header) into raw structures.
  /// Pass the slice that starts at the first structure (after the entry-point block).
  /// </summary>
  public static IReadOnlyList<SmbiosRawStructure> Parse(ReadOnlySpan<byte> tableData) {
    var results = new List<SmbiosRawStructure>();
    int pos = 0;

    while (pos < tableData.Length - 3) {
      byte type = tableData[pos];
      byte length = tableData[pos + 1];
      ushort handle = (ushort)(tableData[pos + 2] | (tableData[pos + 3] << 8));

      // Safety: the formatted area must fit in the remaining data,
      // and length must be at least the 4-byte header.
      if (length < 4 || pos + length > tableData.Length)
        break;

      // Copy formatted area into owned memory.
      var formattedArea = tableData.Slice(pos, length).ToArray();

      // Advance past the formatted area and parse the string table.
      int strStart = pos + length;
      var strings = ParseStringTable(tableData, strStart, out int endOfStrings);

      results.Add(new SmbiosRawStructure(
          (SmbiosStructureType)type,
          length,
          handle,
          formattedArea.AsMemory(),
          strings));

      // End-of-Table marker (Type 127)?
      if (type == (byte)SmbiosStructureType.EndOfTable)
        break;

      pos = endOfStrings;
    }

    return results;
  }

  // ── Private helpers ───────────────────────────────────────────────────────

  /// <summary>
  /// Reads the null-delimited string table that follows each structure's formatted area.
  /// The table ends with a double-null (00 00). An empty table is represented by 00 00
  /// immediately (no strings present).
  /// </summary>
  private static IReadOnlyList<string> ParseStringTable(
      ReadOnlySpan<byte> data, int start, out int endPos) {
    var strings = new List<string>();
    int pos = start;

    // Handle the degenerate case: double-null at start means no strings.
    if (pos < data.Length && data[pos] == 0x00) {
      endPos = pos + 2; // skip 00 00
      return strings;
    }

    while (pos < data.Length) {
      if (data[pos] == 0x00) {
        // End of one string; check for double-null.
        pos++;
        if (pos < data.Length && data[pos] == 0x00) {
          pos++; // consume the second null
          break;
        }
        // Another string follows immediately (rare but valid).
        continue;
      }

      // Find end of current string.
      int stringStart = pos;
      while (pos < data.Length && data[pos] != 0x00)
        pos++;

      strings.Add(Latin1.GetString(data.Slice(stringStart, pos - stringStart)));
      // The loop will hit the 0x00 byte next iteration.
    }

    endPos = pos;
    return strings;
  }
}
