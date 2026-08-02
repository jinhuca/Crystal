using System;

namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// TPM Device Characteristics bit-field (DSP0134 §7.44.1)
/// </summary>
[Flags]
public enum TpmDeviceCharacteristics : ulong {
  CharacteristicsNotSupported = 1UL << 2,
  FamilyConfigurableViaFirmwareUpdate = 1UL << 3,
  FamilyConfigurableViaPlatformSoftware = 1UL << 4,
  FamilyConfigurableViaOemMechanism = 1UL << 5,
}

/// <summary>
/// Type 43 — TPM Device (DSP0134 §7.44).
/// Describes a Trusted Platform Module present in the system.
/// </summary>
public sealed class T043_TpmDevice : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>4-character ASCII vendor ID (e.g. "IFX" for Infineon, left-justified, NUL-padded).</summary>
  public string VendorId { get; init; } = string.Empty;

  public byte MajorSpecVersion { get; init; }
  public byte MinorSpecVersion { get; init; }

  /// <summary>
  /// Firmware version. For spec version 1.2, both fields together form
  /// vendor-specific info; for 2.0, FirmwareVersion1 is the major.minor
  /// pair (each a 16-bit value) and FirmwareVersion2 is vendor-specific.
  /// </summary>
  public uint FirmwareVersion1 { get; init; }
  public uint FirmwareVersion2 { get; init; }

  public string? Description { get; init; }
  public TpmDeviceCharacteristics Characteristics { get; init; }

  /// <summary>OEM- or BIOS vendor-specific information; 0 when not present (structures shorter than 0x1F bytes).</summary>
  public uint OemDefined { get; init; }

  internal static T043_TpmDevice Decode(SmbiosRawStructure s) {
    var vendorChars = new char[4];
    for (int i = 0; i < 4; i++) {
      byte b = s.ReadByte(0x04 + i);
      vendorChars[i] = b >= 32 && b < 127 ? (char)b : '\0';
    }
    string vendorId = new string(vendorChars).TrimEnd('\0');

    return new T043_TpmDevice {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      VendorId = vendorId,
      MajorSpecVersion = s.ReadByte(0x08),
      MinorSpecVersion = s.ReadByte(0x09),
      FirmwareVersion1 = s.ReadDWord(0x0A),
      FirmwareVersion2 = s.ReadDWord(0x0E),
      Description = s.GetString(s.ReadByte(0x12)),
      Characteristics = (TpmDeviceCharacteristics)s.ReadQWord(0x13),
      OemDefined = s.Length > 0x1E ? s.ReadDWord(0x1B) : 0,
    };
  }
}
