using System.Collections.Generic;

namespace Crystal.Smbios.Types;

/// <summary>
/// Type 11 — OEM Strings (DSP0134 §7.11?)
/// Contains an array of free-form strings provided by the OEM/BIOS.
/// </summary>
public sealed class T011_OemStrings : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>Number of strings reported by the formatted area (may be 0).</summary>
  public byte NumberOfStrings { get; init; }

  /// <summary>Decoded string table (may be empty).</summary>
  public IReadOnlyList<string> Strings { get; init; } = System.Array.Empty<string>();

  internal static T011_OemStrings Decode(SmbiosRawStructure s) {
    return new T011_OemStrings {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      NumberOfStrings = s.Length > 0x04 ? s.ReadByte(0x04) : (byte)0,
      Strings = s.Strings,
    };
  }
}
