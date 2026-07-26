namespace Crystal.Smbios.Types;

/// <summary>
/// Type 31 — Boot Integrity Services (BIS) Entry Point (DSP0134 §7.32).
/// Reserved for the (now-obsolete) Boot Integrity Services specification;
/// rarely found on modern firmware.
/// </summary>
public sealed class T031_BootIntegrityServicesEntryPoint : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>Checksum such that summing all bytes of the structure yields 0.</summary>
  public byte Checksum { get; init; }

  /// <summary>Real-mode entry point, expressed as segment:offset.</summary>
  public ushort BisEntry16Segment { get; init; }
  public ushort BisEntry16Offset { get; init; }

  /// <summary>Flat 32-bit physical entry point address.</summary>
  public uint BisEntry32Address { get; init; }

  internal static T031_BootIntegrityServicesEntryPoint Decode(SmbiosRawStructure s) {
    uint packed16 = s.ReadDWord(0x08);
    return new T031_BootIntegrityServicesEntryPoint {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Checksum = s.ReadByte(0x04),
      BisEntry16Segment = (ushort)(packed16 >> 16),
      BisEntry16Offset = (ushort)(packed16 & 0xFFFF),
      BisEntry32Address = s.ReadDWord(0x0C),
    };
  }
}
