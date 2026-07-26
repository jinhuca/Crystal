using System;
using System.Collections.Generic;

namespace Crystal.Smbios.Types;

/// <summary>
/// A single onboard-device entry decoded from a Type 10 structure.
/// </summary>
public sealed class LegacyOnboardDeviceEntry {
  /// <summary>True when the device is enabled (bit 7 of the raw Device Type byte).</summary>
  public bool IsEnabled { get; init; }
  /// <summary>Device category (lower 7 bits of the raw Device Type byte). Reuses the
  /// Type 41 enumeration, whose values 0x01-0x0A match this obsolete type exactly.</summary>
  public OnboardDeviceType DeviceType { get; init; }
  public string? Description { get; init; }
}

/// <summary>
/// Type 10 — On Board Devices Information (Obsolete) (DSP0134 §7.11).
/// Superseded by Onboard Devices Extended Information (Type 41), which adds
/// PCI bus addressing; see <see cref="T041_OnboardDeviceExtendedInformation"/>.
/// </summary>
public sealed class T010_OnBoardDevicesInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>One entry per onboard device; count derived from the structure length.</summary>
  public IReadOnlyList<LegacyOnboardDeviceEntry> Devices { get; init; } = Array.Empty<LegacyOnboardDeviceEntry>();

  internal static T010_OnBoardDevicesInformation Decode(SmbiosRawStructure s) {
    var devices = new List<LegacyOnboardDeviceEntry>();
    int count = (s.Length - 4) / 2;
    for (int i = 0; i < count; i++) {
      int offset = 0x04 + i * 2;
      if (s.Length < offset + 2) break;
      byte raw = s.ReadByte(offset);
      devices.Add(new LegacyOnboardDeviceEntry {
        IsEnabled = (raw & 0x80) != 0,
        DeviceType = (OnboardDeviceType)(raw & 0x7F),
        Description = s.GetString(s.ReadByte(offset + 1)),
      });
    }

    return new T010_OnBoardDevicesInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Devices = devices,
    };
  }
}
