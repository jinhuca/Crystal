namespace Crystal.Smbios.Types;

/// <summary>
/// IPMI Device Information — BMC Interface Type Enumeration (DSP0134 §7.39.1)
/// </summary>
public enum BmcInterfaceType : byte {
  Unknown = 0x00,
  Kcs = 0x01,
  Smic = 0x02,
  Bt = 0x03,
  Ssif = 0x04,
}

/// <summary>
/// Type 38 — IPMI Device Information (DSP0134 §7.39).
/// Describes the Baseboard Management Controller's IPMI interface.
/// Type 42 (Management Controller Host Interface) can also describe this
/// interface and is recommended for interfaces shared with other protocols.
/// </summary>
public sealed class T038_IpmiDeviceInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public BmcInterfaceType InterfaceType { get; init; }

  /// <summary>BCD-packed IPMI specification revision: high nibble = major, low nibble = minor.</summary>
  public byte IpmiSpecificationRevisionRaw { get; init; }
  public int IpmiSpecificationMajor => IpmiSpecificationRevisionRaw >> 4;
  public int IpmiSpecificationMinor => IpmiSpecificationRevisionRaw & 0x0F;

  /// <summary>I2C slave address on the system's I2C bus.</summary>
  public byte I2CSlaveAddress { get; init; }
  /// <summary>Slave address of the BMC's non-volatile storage, or 0xFF if not present.</summary>
  public byte NVStorageDeviceAddress { get; init; }
  public bool HasNVStorage => NVStorageDeviceAddress != 0xFF;

  /// <summary>Base address of the BMC interface registers (interpretation depends on <see cref="InterfaceType"/>).</summary>
  public ulong BaseAddress { get; init; }

  public byte BaseAddressModifierAndInterruptInfo { get; init; }
  public byte InterruptNumber { get; init; }

  internal static T038_IpmiDeviceInformation Decode(SmbiosRawStructure s) {
    return new T038_IpmiDeviceInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      InterfaceType = (BmcInterfaceType)s.ReadByte(0x04),
      IpmiSpecificationRevisionRaw = s.ReadByte(0x05),
      I2CSlaveAddress = s.ReadByte(0x06),
      NVStorageDeviceAddress = s.ReadByte(0x07),
      BaseAddress = s.ReadQWord(0x08),
      BaseAddressModifierAndInterruptInfo = s.Length > 0x10 ? s.ReadByte(0x10) : (byte)0,
      InterruptNumber = s.Length > 0x11 ? s.ReadByte(0x11) : (byte)0,
    };
  }
}
