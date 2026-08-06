using System;

namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 0 — BIOS Information (DSP0134 §7.1)
/// </summary>
public sealed class T000_BiosInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public string? Vendor { get; init; }
  public string? Version { get; init; }
  public string? ReleaseDate { get; init; }
  public ushort StartingAddressSegment { get; init; }
  public byte RomSize { get; init; }
  public ushort ExtendedRomSize { get; init; }
  public BiosCharacteristics Characteristics { get; init; }
  public byte CharacteristicsExt1 { get; init; }
  public byte CharacteristicsExt2 { get; init; }
  public byte BiosMajorRelease { get; init; }
  public byte BiosMinorRelease { get; init; }
  public byte EcFirmwareMajor { get; init; }
  public byte EcFirmwareMinor { get; init; }

  /// <summary>
  /// Gets whether the system firmware supports UEFI (Bit 3 of BIOS Characteristics Extension Byte 2).
  /// </summary>
  public bool IsUefiSupported => (CharacteristicsExt2 & 0x08) != 0;

  public long RomSizeBytes {
    get {
      if (ExtendedRomSize != 0) {
        uint unit = (uint)(ExtendedRomSize >> 14) & 0x3;
        uint value = (uint)(ExtendedRomSize & 0x3FFF);
        return unit switch {
          0 => value * 1024L * 1024L,           // MB
          1 => value * 1024L * 1024L * 1024L,  // GB
          2 => value * 1024L,                   // KB
          _ => ((long)RomSize + 1) * 64 * 1024  // reserved → fallback
        };
      }
      return ((long)RomSize + 1) * 64 * 1024;
    }
  }

  internal static T000_BiosInformation Decode(SmbiosRawStructure s) {
    return new T000_BiosInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Vendor = s.GetString(s.ReadByte(0x04)),
      Version = s.GetString(s.ReadByte(0x05)),
      StartingAddressSegment = s.ReadWord(0x06),
      ReleaseDate = s.GetString(s.ReadByte(0x08)),
      RomSize = s.ReadByte(0x09),
      Characteristics = (BiosCharacteristics)s.ReadQWord(0x0A),
      CharacteristicsExt1 = s.Length > 0x12 ? s.ReadByte(0x12) : (byte)0,
      CharacteristicsExt2 = s.Length > 0x13 ? s.ReadByte(0x13) : (byte)0,
      BiosMajorRelease = s.Length > 0x14 ? s.ReadByte(0x14) : (byte)0xFF,
      BiosMinorRelease = s.Length > 0x15 ? s.ReadByte(0x15) : (byte)0xFF,
      EcFirmwareMajor = s.Length > 0x16 ? s.ReadByte(0x16) : (byte)0xFF,
      EcFirmwareMinor = s.Length > 0x17 ? s.ReadByte(0x17) : (byte)0xFF,
      ExtendedRomSize = s.Length > 0x19 ? s.ReadWord(0x18) : (ushort)0,
    };
  }
}

// DSP0134 §7.1 — BIOS Characteristics bitmask (QWORD at offset 0x0A)
[Flags]
public enum BiosCharacteristics : ulong {
  Unknown = 1UL << 2,
  NotSupported = 1UL << 3,
  IsaSupported = 1UL << 4,
  McaSupported = 1UL << 5,
  EisaSupported = 1UL << 6,
  PciSupported = 1UL << 7,
  PcCardSupported = 1UL << 8,
  PnpSupported = 1UL << 9,
  ApmSupported = 1UL << 10,
  BiosFlashUpgradeable = 1UL << 11,
  BiosShadowingAllowed = 1UL << 12,
  VlVesaSupported = 1UL << 13,
  EscdSupported = 1UL << 14,
  BootFromCdSupported = 1UL << 15,
  SelectableBootSupported = 1UL << 16,
  BiosRomSocketed = 1UL << 17,
  BootFromPcCardSupported = 1UL << 18,
  EddSpecSupported = 1UL << 19,
  Int13H360KbFloppySupported = 1UL << 20,
  Int13H12MbFloppySupported = 1UL << 21,
  Int13H720KbFloppySupported = 1UL << 22,
  Int13H2880KbFloppySupported = 1UL << 23,
  Int5HPrintScreenSupported = 1UL << 24,
  Int9H8042KeyboardSupported = 1UL << 25,
  Int14HSerialSupported = 1UL << 26,
  Int17HPrinterSupported = 1UL << 27,
  Int10HCgaMonoVideoSupported = 1UL << 28,
  NecPc98Supported = 1UL << 29,
}
