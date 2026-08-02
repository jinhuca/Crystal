namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Management Device Type Enumeration (DSP0134 §7.35.1)
/// </summary>
public enum ManagementDeviceType : byte {
  Other = 0x01,
  Unknown = 0x02,
  Lm75 = 0x03,
  Lm78 = 0x04,
  Lm79 = 0x05,
  Lm80 = 0x06,
  Lm81 = 0x07,
  Adm9240 = 0x08,
  Ds1780 = 0x09,
  Maxim1617 = 0x0A,
  Gl518Sm = 0x0B,
  W83781D = 0x0C,
  Ht82H791 = 0x0D,
}

/// <summary>
/// Management Device Address Type Enumeration (DSP0134 §7.35.2)
/// </summary>
public enum ManagementDeviceAddressType : byte {
  Other = 0x01,
  Unknown = 0x02,
  IOPort = 0x03,
  Memory = 0x04,
  Smbus = 0x05,
}

/// <summary>
/// Type 34 — Management Device (DSP0134 §7.35).
/// Describes a hardware-monitoring chip (temperature/voltage/fan sensor
/// controller) that one or more Type 35/36 structures attach to.
/// </summary>
public sealed class T034_ManagementDevice : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public string? Description { get; init; }
  public ManagementDeviceType Type { get; init; }
  public uint Address { get; init; }
  public ManagementDeviceAddressType AddressType { get; init; }

  internal static T034_ManagementDevice Decode(SmbiosRawStructure s) {
    return new T034_ManagementDevice {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Description = s.GetString(s.ReadByte(0x04)),
      Type = (ManagementDeviceType)s.ReadByte(0x05),
      Address = s.ReadDWord(0x06),
      AddressType = (ManagementDeviceAddressType)s.ReadByte(0x0A),
    };
  }
}
