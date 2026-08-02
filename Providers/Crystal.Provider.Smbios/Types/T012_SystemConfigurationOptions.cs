namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 12 — System Configuration Options (DSP0134 §7.13).
/// Free-form strings describing the base board's jumper and switch settings
/// (e.g. "JP1: Clear CMOS"). Content is entirely vendor-defined.
/// </summary>
public sealed class T012_SystemConfigurationOptions : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>Number of configuration-option strings reported (may be 0).</summary>
  public byte StringCount { get; init; }

  /// <summary>Decoded string table (may be empty).</summary>
  public System.Collections.Generic.IReadOnlyList<string> Options { get; init; } = System.Array.Empty<string>();

  internal static T012_SystemConfigurationOptions Decode(SmbiosRawStructure s) {
    return new T012_SystemConfigurationOptions {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      StringCount = s.Length > 0x04 ? s.ReadByte(0x04) : (byte)0,
      Options = s.Strings,
    };
  }
}
