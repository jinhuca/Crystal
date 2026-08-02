using System;

namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Baseboard Feature Flags Bitmask (DSP0134 §7.3.1)
/// </summary>
[Flags]
public enum BaseboardFeatureFlags : byte {
  None = 0x00,
  HostingBoard = 0x01,       // Bit 0: Board is a hosting board (e.g., a motherboard)
  RequiresDaughterboard = 0x02, // Bit 1: Board requires at least one daughterboard
  Removable = 0x04,          // Bit 2: Board is removable
  Replaceable = 0x08,        // Bit 3: Board is replaceable
  HotSwappable = 0x10        // Bit 4: Board is hot swappable
}

/// <summary>
/// Baseboard Board Type Enumeration (DSP0134 §7.3.2)
/// </summary>
public enum BaseboardType : byte {
  Unknown = 0x01,
  Other = 0x02,
  ServerBlade = 0x03,
  ConnectivitySwitch = 0x04,
  SystemManagementModule = 0x05,
  ProcessorModule = 0x06,
  IOMobile = 0x07,
  MemoryModule = 0x08,
  Daughterboard = 0x09,
  Motherboard = 0x0A,
  ProcessorMemoryModule = 0x0B,
  ProcessorIOModule = 0x0C,
  InterconnectBoard = 0x0D
}

/// <summary>
/// Type 2 — Baseboard (or Module) Information (DSP0134 §7.3)
/// </summary>
public sealed class T002_BaseboardInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public string? Manufacturer { get; init; }
  public string? Product { get; init; }
  public string? Version { get; init; }
  public string? SerialNumber { get; init; }
  public string? AssetTag { get; init; }
  public BaseboardFeatureFlags FeatureFlags { get; init; }
  public string? LocationInChassis { get; init; }
  public ushort ChassisHandle { get; init; }
  public BaseboardType BoardType { get; init; }
  public ushort[] ContainedObjectHandles { get; init; } = Array.Empty<ushort>();

  // Explicit helper boolean expressions built off the Flags enum
  public bool IsHostingBoard => (FeatureFlags & BaseboardFeatureFlags.HostingBoard) != 0;
  public bool IsRemovable => (FeatureFlags & BaseboardFeatureFlags.Removable) != 0;
  public bool IsHotSwappable => (FeatureFlags & BaseboardFeatureFlags.HotSwappable) != 0;

  internal static T002_BaseboardInformation Decode(SmbiosRawStructure s) {
    // Determine contained handles count if structural boundary allows it
    ushort[] handles = Array.Empty<ushort>();
    if (s.Length > 0x0E) {
      byte count = s.ReadByte(0x0E);
      if (count > 0 && s.Length >= 0x0F + (count * 2)) {
        handles = new ushort[count];
        for (int i = 0; i < count; i++) {
          handles[i] = s.ReadWord(0x0F + (i * 2));
        }
      }
    }

    return new T002_BaseboardInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Manufacturer = s.GetString(s.ReadByte(0x04)),
      Product = s.GetString(s.ReadByte(0x05)),
      Version = s.GetString(s.ReadByte(0x06)),
      SerialNumber = s.GetString(s.ReadByte(0x07)),
      AssetTag = s.Length > 0x08 ? s.GetString(s.ReadByte(0x08)) : null,
      FeatureFlags = s.Length > 0x09 ? (BaseboardFeatureFlags)s.ReadByte(0x09) : BaseboardFeatureFlags.None,
      LocationInChassis = s.Length > 0x0A ? s.GetString(s.ReadByte(0x0A)) : null,
      ChassisHandle = s.Length > 0x0C ? s.ReadWord(0x0B) : (ushort)0,
      BoardType = s.Length > 0x0D ? (BaseboardType)s.ReadByte(0x0D) : BaseboardType.Unknown,
      ContainedObjectHandles = handles
    };
  }
}
