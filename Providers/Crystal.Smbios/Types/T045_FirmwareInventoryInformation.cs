using System;
using System.Collections.Generic;

namespace Crystal.Smbios.Types;

/// <summary>
/// Firmware Inventory Version Format Type Enumeration (DSP0134 §7.46.1)
/// </summary>
public enum FirmwareVersionFormat : byte {
  FreeForm = 0x00,
  MajorMinor = 0x01,
  Hex32Bit = 0x02,
  Hex64Bit = 0x03,
}

/// <summary>
/// Firmware Inventory Firmware ID Format Type Enumeration (DSP0134 §7.46.2)
/// </summary>
public enum FirmwareIdFormat : byte {
  FreeForm = 0x00,
  Uuid = 0x01,
}

/// <summary>
/// Firmware Inventory Characteristics bit-field (DSP0134 §7.46.3)
/// </summary>
[Flags]
public enum FirmwareCharacteristics : ushort {
  Updatable = 0x0001,
  WriteProtected = 0x0002,
}

/// <summary>
/// Firmware Inventory State Enumeration (DSP0134 §7.46.4)
/// </summary>
public enum FirmwareInventoryState : byte {
  Other = 0x01,
  Unknown = 0x02,
  Disabled = 0x03,
  Enabled = 0x04,
  Absent = 0x05,
  StandbyOffline = 0x06,
  StandbySpare = 0x07,
  UnavailableOffline = 0x08,
}

/// <summary>
/// Type 45 — Firmware Inventory Information (DSP0134 §7.46).
/// One structure per firmware component in the system (system BIOS, BMC,
/// drive firmware, etc), letting management software present a uniform
/// firmware inventory across the platform.
/// </summary>
public sealed class T045_FirmwareInventoryInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public string? FirmwareComponentName { get; init; }
  public string? FirmwareVersion { get; init; }
  public FirmwareVersionFormat FirmwareVersionFormat { get; init; }
  public string? FirmwareId { get; init; }
  public FirmwareIdFormat FirmwareIdFormat { get; init; }
  public string? ReleaseDate { get; init; }
  public string? Manufacturer { get; init; }
  public string? LowestSupportedVersion { get; init; }
  public ulong ImageSizeBytes { get; init; }
  public FirmwareCharacteristics Characteristics { get; init; }
  public FirmwareInventoryState State { get; init; }

  /// <summary>Handles of other Type 45 structures this component depends on/contains.</summary>
  public IReadOnlyList<ushort> AssociatedComponentHandles { get; init; } = Array.Empty<ushort>();

  internal static T045_FirmwareInventoryInformation Decode(SmbiosRawStructure s) {
    byte associatedCount = s.Length > 0x17 ? s.ReadByte(0x17) : (byte)0;
    var handles = new List<ushort>(associatedCount);
    for (int i = 0; i < associatedCount; i++) {
      int offset = 0x18 + i * 2;
      if (s.Length < offset + 2) break;
      handles.Add(s.ReadWord(offset));
    }

    return new T045_FirmwareInventoryInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      FirmwareComponentName = s.GetString(s.ReadByte(0x04)),
      FirmwareVersion = s.GetString(s.ReadByte(0x05)),
      FirmwareVersionFormat = (FirmwareVersionFormat)s.ReadByte(0x06),
      FirmwareId = s.GetString(s.ReadByte(0x07)),
      FirmwareIdFormat = (FirmwareIdFormat)s.ReadByte(0x08),
      ReleaseDate = s.GetString(s.ReadByte(0x09)),
      Manufacturer = s.GetString(s.ReadByte(0x0A)),
      LowestSupportedVersion = s.GetString(s.ReadByte(0x0B)),
      ImageSizeBytes = s.ReadQWord(0x0C),
      Characteristics = (FirmwareCharacteristics)s.ReadWord(0x14),
      State = (FirmwareInventoryState)s.ReadByte(0x16),
      AssociatedComponentHandles = handles,
    };
  }
}
