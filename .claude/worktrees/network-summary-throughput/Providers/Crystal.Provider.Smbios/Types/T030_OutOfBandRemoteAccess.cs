namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 30 — Out-of-Band Remote Access (DSP0134 §7.31).
/// Describes a hardware facility for remote access to the system when the
/// OS is unavailable (power-down, hardware failure, boot failure, etc).
/// </summary>
public sealed class T030_OutOfBandRemoteAccess : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public string? ManufacturerName { get; init; }
  public byte ConnectionsRaw { get; init; }

  /// <summary>Whether the facility can receive inbound connections (bit 0).</summary>
  public bool InboundConnectionEnabled { get; init; }
  /// <summary>Whether the facility can initiate outbound connections (bit 1).</summary>
  public bool OutboundConnectionEnabled { get; init; }

  internal static T030_OutOfBandRemoteAccess Decode(SmbiosRawStructure s) {
    byte raw = s.ReadByte(0x05);
    return new T030_OutOfBandRemoteAccess {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      ManufacturerName = s.GetString(s.ReadByte(0x04)),
      ConnectionsRaw = raw,
      InboundConnectionEnabled = (raw & 0x01) != 0,
      OutboundConnectionEnabled = (raw & 0x02) != 0,
    };
  }
}
