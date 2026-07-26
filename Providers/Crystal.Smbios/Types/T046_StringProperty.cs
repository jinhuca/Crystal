using System;

namespace Crystal.Smbios.Types;

/// <summary>
/// String Property ID Enumeration (DSP0134 §7.47.1).
/// 0x0002-0x7FFF are reserved, 0x8000-0xBFFF are BIOS vendor-specific, and
/// 0xC000-0xFFFF are OEM-specific.
/// </summary>
public enum StringPropertyId : ushort {
  DevicePath = 0x0001,
}

/// <summary>
/// Type 46 — String Property (DSP0134 §7.47).
/// Attaches an additional named string value to another structure without
/// modifying that structure's own definition. Multiple Type 46 structures
/// may target the same parent handle (e.g. with different property IDs).
/// </summary>
public sealed class T046_StringProperty : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public ushort PropertyIdRaw { get; init; }
  public StringPropertyId? PropertyId =>
      Enum.IsDefined(typeof(StringPropertyId), PropertyIdRaw) ? (StringPropertyId)PropertyIdRaw : null;

  public string? PropertyValue { get; init; }

  /// <summary>Handle of the structure this string property applies to.</summary>
  public ushort ParentHandle { get; init; }

  internal static T046_StringProperty Decode(SmbiosRawStructure s) {
    return new T046_StringProperty {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      PropertyIdRaw = s.ReadWord(0x04),
      PropertyValue = s.GetString(s.ReadByte(0x06)),
      ParentHandle = s.ReadWord(0x07),
    };
  }
}
