using System;
using System.Collections.Generic;

namespace Crystal.Smbios.Types;

/// <summary>
/// Memory Channel Type Enumeration (DSP0134 §7.38.1)
/// </summary>
public enum MemoryChannelType : byte {
  Other = 0x01,
  Unknown = 0x02,
  RamBus = 0x03,
  SyncLink = 0x04,
}

/// <summary>One Memory Device's load contribution to a Memory Channel.</summary>
public readonly record struct MemoryChannelDeviceLoad(byte DeviceLoad, ushort MemoryDeviceHandle);

/// <summary>
/// Type 37 — Memory Channel (DSP0134 §7.38).
/// Correlates a memory channel with the Memory Device (Type 17) structures
/// it feeds, and each device's load contribution against the channel maximum.
/// </summary>
public sealed class T037_MemoryChannel : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public MemoryChannelType ChannelType { get; init; }
  public byte MaximumChannelLoad { get; init; }

  /// <summary>Per-device load entries; count derived from the structure length.</summary>
  public IReadOnlyList<MemoryChannelDeviceLoad> Devices { get; init; } = Array.Empty<MemoryChannelDeviceLoad>();

  /// <summary>Sum of all per-device load values.</summary>
  public int TotalLoad {
    get {
      int total = 0;
      foreach (var d in Devices) total += d.DeviceLoad;
      return total;
    }
  }

  internal static T037_MemoryChannel Decode(SmbiosRawStructure s) {
    byte deviceCount = s.ReadByte(0x06);
    var devices = new List<MemoryChannelDeviceLoad>(deviceCount);
    for (int i = 0; i < deviceCount; i++) {
      int offset = 0x07 + i * 3;
      if (s.Length < offset + 3) break;
      devices.Add(new MemoryChannelDeviceLoad(s.ReadByte(offset), s.ReadWord(offset + 1)));
    }

    return new T037_MemoryChannel {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      ChannelType = (MemoryChannelType)s.ReadByte(0x04),
      MaximumChannelLoad = s.ReadByte(0x05),
      Devices = devices,
    };
  }
}
