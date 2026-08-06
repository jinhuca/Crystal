using System;

namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// System Wake-up Type Enumeration (DSP0134 §7.2.2)
/// </summary>
public enum SystemWakeUpType : byte {
  Reserved = 0x00,
  Other = 0x01,
  Unknown = 0x02,
  ApmTimer = 0x03,
  ModemRing = 0x04,
  LanRemote = 0x05,
  PowerSwitch = 0x06,
  PciPme = 0x07,
  AcPowerRestored = 0x08
}

/// <summary>
/// Type 1 — System Information (DSP0134 §7.2)
/// </summary>
public sealed class T001_SystemInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public string? Manufacturer { get; init; }
  public string? ProductName { get; init; }
  public string? Version { get; init; }
  public string? SerialNumber { get; init; }
  public Guid Uuid { get; init; }
  public SystemWakeUpType WakeUpType { get; init; }
  public string? SkuNumber { get; init; }
  public string? Family { get; init; }

  internal static T001_SystemInformation Decode(SmbiosRawStructure s) {
    // Read the UUID fields as defined by the SMBIOS specification mapping to a Guid
    Guid parsedUuid = Guid.Empty;
    if (s.Length > 0x17) {
      // In SMBIOS, UUID can be read natively if your helper supports a ReadGuid, 
      // or by assembling the bytes from offset 0x08 (16 bytes total)
      byte[] uuidBytes = new byte[16];
      for (int i = 0; i < 16; i++) {
        uuidBytes[i] = s.ReadByte(0x08 + i);
      }
      parsedUuid = new Guid(uuidBytes);
    }

    return new T001_SystemInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Manufacturer = s.GetString(s.ReadByte(0x04)),
      ProductName = s.GetString(s.ReadByte(0x05)),
      Version = s.GetString(s.ReadByte(0x06)),
      SerialNumber = s.GetString(s.ReadByte(0x07)),
      Uuid = parsedUuid,
      WakeUpType = s.Length > 0x18 ? (SystemWakeUpType)s.ReadByte(0x18) : SystemWakeUpType.Unknown,
      SkuNumber = s.Length > 0x19 ? s.GetString(s.ReadByte(0x19)) : null,
      Family = s.Length > 0x1A ? s.GetString(s.ReadByte(0x1A)) : null
    };
  }
}
