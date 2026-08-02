using System;
using System.Collections.Generic;

namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Memory Controller Error Detecting Method Enumeration (DSP0134 §7.6.1)
/// </summary>
public enum MemoryControllerErrorDetectingMethod : byte {
  Other = 0x01,
  Unknown = 0x02,
  None = 0x03,
  Parity = 0x04,
  Ecc32Bit = 0x05,
  Ecc64Bit = 0x06,
  Ecc128Bit = 0x07,
  Crc = 0x08,
}

/// <summary>
/// Memory Controller Error Correcting Capability bit-field (DSP0134 §7.6.2)
/// </summary>
[Flags]
public enum MemoryControllerErrorCorrectingCapability : byte {
  Other = 0x01,
  Unknown = 0x02,
  None = 0x04,
  SingleBitErrorCorrecting = 0x08,
  DoubleBitErrorCorrecting = 0x10,
  ErrorScrubbing = 0x20,
}

/// <summary>
/// Memory Controller Supported/Current Interleave Enumeration (DSP0134 §7.6.3/§7.6.4)
/// </summary>
public enum MemoryControllerInterleaveType : byte {
  Other = 0x01,
  Unknown = 0x02,
  OneWay = 0x03,
  TwoWay = 0x04,
  FourWay = 0x05,
  EightWay = 0x06,
  SixteenWay = 0x07,
}

/// <summary>
/// Memory Controller Supported Speeds bit-field (DSP0134 §7.6.5)
/// </summary>
[Flags]
public enum MemoryControllerSpeedFlags : ushort {
  Other = 0x0001,
  Unknown = 0x0002,
  SeventyNs = 0x0004,
  SixtyNs = 0x0008,
  FiftyNs = 0x0010,
}

/// <summary>
/// Memory Module/Controller Supported Types bit-field, shared by Type 5's
/// SupportedMemoryTypes and Type 6's CurrentMemoryType (DSP0134 §7.6.6/§7.7.2)
/// </summary>
[Flags]
public enum MemoryModuleTypeFlags : ushort {
  Other = 0x0001,
  Unknown = 0x0002,
  Standard = 0x0004,
  FastPageMode = 0x0008,
  Edo = 0x0010,
  Parity = 0x0020,
  Ecc = 0x0040,
  Simm = 0x0080,
  Dimm = 0x0100,
  BurstEdo = 0x0200,
  Sdram = 0x0400,
}

/// <summary>
/// Memory Module Voltage bit-field (DSP0134 §7.6.7)
/// </summary>
[Flags]
public enum MemoryModuleVoltageFlags : byte {
  FiveVolt = 0x01,
  ThreePoint3Volt = 0x02,
  TwoPoint9Volt = 0x04,
}

/// <summary>
/// Type 5 — Memory Controller Information (Obsolete) (DSP0134 §7.6).
/// Superseded by the Physical Memory Array (Type 16) and Memory Device
/// (Type 17) structures since SMBIOS 2.1; retained for legacy DMI browsers.
/// </summary>
public sealed class T005_MemoryControllerInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public MemoryControllerErrorDetectingMethod ErrorDetectingMethod { get; init; }
  public MemoryControllerErrorCorrectingCapability ErrorCorrectingCapability { get; init; }
  public MemoryControllerInterleaveType SupportedInterleave { get; init; }
  public MemoryControllerInterleaveType CurrentInterleave { get; init; }

  /// <summary>Maximum size (n) of a supported memory module: 2^n MB.</summary>
  public byte MaximumMemoryModuleSizeRaw { get; init; }
  public long? MaximumMemoryModuleSizeMiB => MaximumMemoryModuleSizeRaw < 63 ? 1L << MaximumMemoryModuleSizeRaw : null;

  public MemoryControllerSpeedFlags SupportedSpeeds { get; init; }
  public MemoryModuleTypeFlags SupportedMemoryTypes { get; init; }
  public MemoryModuleVoltageFlags SupportedVoltages { get; init; }

  /// <summary>Number of memory-slot handles associated with this controller.</summary>
  public byte AssociatedMemorySlotCount { get; init; }

  /// <summary>Handles of the Memory Module Information (Type 6) structures controlled by this controller.</summary>
  public IReadOnlyList<ushort> AssociatedMemorySlotHandles { get; init; } = Array.Empty<ushort>();

  /// <summary>Enabled Error Correcting Capabilities (v2.1+); located after the associated-handle array.</summary>
  public MemoryControllerErrorCorrectingCapability EnabledErrorCorrectingCapabilities { get; init; }

  internal static T005_MemoryControllerInformation Decode(SmbiosRawStructure s) {
    byte slotCount = s.Length > 0x0E ? s.ReadByte(0x0E) : (byte)0;
    var handles = new List<ushort>(slotCount);
    for (int i = 0; i < slotCount; i++) {
      int offset = 0x0F + i * 2;
      if (s.Length < offset + 2) break;
      handles.Add(s.ReadWord(offset));
    }

    // Enabled Error Correcting Capabilities BYTE follows the handle array (v2.1+).
    int enabledEccOffset = 0x0F + slotCount * 2;
    var enabledEcc = s.Length > enabledEccOffset
        ? (MemoryControllerErrorCorrectingCapability)s.ReadByte(enabledEccOffset)
        : default;

    return new T005_MemoryControllerInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      ErrorDetectingMethod = (MemoryControllerErrorDetectingMethod)s.ReadByte(0x04),
      ErrorCorrectingCapability = (MemoryControllerErrorCorrectingCapability)s.ReadByte(0x05),
      SupportedInterleave = (MemoryControllerInterleaveType)s.ReadByte(0x06),
      CurrentInterleave = (MemoryControllerInterleaveType)s.ReadByte(0x07),
      MaximumMemoryModuleSizeRaw = s.ReadByte(0x08),
      SupportedSpeeds = (MemoryControllerSpeedFlags)s.ReadWord(0x09),
      SupportedMemoryTypes = (MemoryModuleTypeFlags)s.ReadWord(0x0B),
      SupportedVoltages = (MemoryModuleVoltageFlags)s.ReadByte(0x0D),
      AssociatedMemorySlotCount = slotCount,
      AssociatedMemorySlotHandles = handles,
      EnabledErrorCorrectingCapabilities = enabledEcc,
    };
  }
}
