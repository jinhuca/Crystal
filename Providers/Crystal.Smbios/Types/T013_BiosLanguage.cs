namespace Crystal.Smbios.Types;

/// <summary>
/// Type 13 — BIOS Language (DSP0134 §7.13)
/// Describes the current BIOS language and the set of installable languages.
/// </summary>
public sealed class T013_BiosLanguage : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>Number of installable languages reported by the BIOS.</summary>
  public byte InstallableLanguages { get; init; }

  /// <summary>Flags byte (spec-defined flags).</summary>
  public byte Flags { get; init; }

  /// <summary>Human-readable current language string (e.g. "en-US").</summary>
  public string? CurrentLanguage { get; init; }

  internal static T013_BiosLanguage Decode(SmbiosRawStructure s) {
    return new T013_BiosLanguage {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      InstallableLanguages = s.Length > 0x04 ? s.ReadByte(0x04) : (byte)0,
      Flags = s.Length > 0x05 ? s.ReadByte(0x05) : (byte)0,
      // Current language is a 1-based string number at offset 0x06 per spec
      CurrentLanguage = s.Length > 0x06 ? s.GetString(s.ReadByte(0x06)) : null,
    };
  }
}
