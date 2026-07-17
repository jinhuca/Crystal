using System;

namespace Crystal.Smbios.Types;

/// <summary>
/// Type 9 — describes one physical expansion slot (PCIe, M.2, legacy PCI, etc.)
/// and whether it is currently populated. Unlike Types 7/16, System Slots do
/// not reference or get referenced by another structure's handle — each one
/// stands alone as a description of physical slot hardware.
///
/// NOTE: The v3.4+ "Peer Grouping" variable-length array (used for bifurcated
/// / multi-function slots) is intentionally not decoded here — it is rare on
/// consumer boards and adds significant complexity. <see cref="PeerGroupingCount"/>
/// is exposed so callers at least know how many peer-group entries exist,
/// even though their contents aren't parsed.
/// </summary>
public sealed class T009_SystemSlotInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public string? SlotDesignation { get; init; }
  public SystemSlotType SlotType { get; init; }
  public SlotDataBusWidth DataBusWidth { get; init; }
  public SlotUsage CurrentUsage { get; init; }
  public SlotLength SlotLength { get; init; }

  /// <summary>Slot ID — meaning depends on SlotType (e.g. PCI slot number).</summary>
  public ushort SlotId { get; init; }

  public SlotCharacteristics1 Characteristics1 { get; init; }
  public SlotCharacteristics2 Characteristics2 { get; init; }

  /// <summary>PCI segment group number (v2.6+); 0 on single-segment systems.</summary>
  public ushort SegmentGroupNumber { get; init; }
  /// <summary>PCI bus number (v2.6+) of the device currently in this slot.</summary>
  public byte BusNumber { get; init; }
  /// <summary>Bits 7-3: device number; bits 2-0: function number (v2.6+).</summary>
  public byte DeviceFunctionNumber { get; init; }

  /// <summary>Physical slot width in lanes (v3.2+); 0xFF = not applicable.</summary>
  public byte SlotPhysicalWidth { get; init; }
  /// <summary>Slot pitch in 0.1 mm units (v3.2+); 0 = not given.</summary>
  public ushort SlotPitch { get; init; }
  /// <summary>Slot height classification (v3.4+).</summary>
  public SlotHeight SlotHeight { get; init; }

  /// <summary>
  /// Number of Peer Grouping entries that follow this structure (v3.2+);
  /// the entries themselves are not decoded (see class remarks).
  /// 0 on structures without peer grouping.
  /// </summary>
  public byte PeerGroupingCount { get; init; }

  /// <summary>PCI device number (upper 5 bits of <see cref="DeviceFunctionNumber"/>).</summary>
  public int DeviceNumber => DeviceFunctionNumber >> 3;
  /// <summary>PCI function number (lower 3 bits of <see cref="DeviceFunctionNumber"/>).</summary>
  public int FunctionNumber => DeviceFunctionNumber & 0b111;

  /// <summary>True when a card/module is currently installed in this slot.</summary>
  public bool IsInUse => CurrentUsage == SlotUsage.InUse;

  internal static T009_SystemSlotInformation Decode(SmbiosRawStructure s) {
    // DSP0134 §7.10 formatted-area layout:
    // 04 SlotDesignation           STRING
    // 05 SlotType                  BYTE
    // 06 SlotDataBusWidth          BYTE
    // 07 CurrentUsage              BYTE
    // 08 SlotLength                BYTE
    // 09 SlotID                    WORD
    // 0B SlotCharacteristics1      BYTE
    // 0C SlotCharacteristics2      BYTE   (v2.1+)
    // 0D SegmentGroupNumber        WORD   (v2.6+)
    // 0F BusNumber                 BYTE   (v2.6+)
    // 10 DeviceFunctionNumber      BYTE   (v2.6+)
    // 11 DataBusWidth (physical)   BYTE   (v3.2+) — SlotPhysicalWidth
    // 12 SlotPitch                 WORD   (v3.2+)
    // 14 SlotHeight                BYTE   (v3.4+)
    // 15 PeerGroupingCount         BYTE   (v3.2+; peer-group array follows, not decoded)
    return new T009_SystemSlotInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      SlotDesignation = s.GetString(s.ReadByte(0x04)),
      SlotType = (SystemSlotType)s.ReadByte(0x05),
      DataBusWidth = (SlotDataBusWidth)s.ReadByte(0x06),
      CurrentUsage = (SlotUsage)s.ReadByte(0x07),
      SlotLength = (SlotLength)s.ReadByte(0x08),
      SlotId = s.ReadWord(0x09),
      Characteristics1 = (SlotCharacteristics1)s.ReadByte(0x0B),
      Characteristics2 = s.Length > 0x0C ? (SlotCharacteristics2)s.ReadByte(0x0C) : 0,
      SegmentGroupNumber = s.Length > 0x0E ? s.ReadWord(0x0D) : (ushort)0,
      BusNumber = s.Length > 0x0F ? s.ReadByte(0x0F) : (byte)0,
      DeviceFunctionNumber = s.Length > 0x10 ? s.ReadByte(0x10) : (byte)0,
      SlotPhysicalWidth = s.Length > 0x11 ? s.ReadByte(0x11) : (byte)0xFF,
      SlotPitch = s.Length > 0x13 ? s.ReadWord(0x12) : (ushort)0,
      SlotHeight = s.Length > 0x14 ? (SlotHeight)s.ReadByte(0x14) : SlotHeight.NotApplicable,
      PeerGroupingCount = s.Length > 0x15 ? s.ReadByte(0x15) : (byte)0,
    };
  }
}

// ── Type 9 — System Slots enums (DSP0134 §7.10) ──────────────────────────────

/// <summary>DSP0134 §7.10.1 — Slot Type (partial — covers legacy through modern PCIe/M.2/CXL).</summary>
public enum SystemSlotType : byte {
  Other = 0x01,
  Unknown = 0x02,
  Isa = 0x03,
  Mca = 0x04,
  Eisa = 0x05,
  Pci = 0x06,
  Pcmcia = 0x07,
  VlVesa = 0x08,
  Proprietary = 0x09,
  ProcessorCardSlot = 0x0A,
  ProprietaryMemoryCard = 0x0B,
  IoRiserCard = 0x0C,
  NuBus = 0x0D,
  Pci66MHzCapable = 0x0E,
  Agp = 0x0F,
  Agp2X = 0x10,
  Agp4X = 0x11,
  PciX = 0x12,
  Agp8X = 0x13,
  M2Socket1DP = 0x14,
  M2Socket1SD = 0x15,
  M2Socket2 = 0x16,
  M2Socket3 = 0x17,
  MxmTypeI = 0x18,
  MxmTypeII = 0x19,
  MxmTypeIIIStandard = 0x1A,
  MxmTypeIIIHe = 0x1B,
  MxmTypeIV = 0x1C,
  MxmType5 = 0x1D,
  PciExpressGen2Sff8639 = 0x1E,
  PciExpressGen3Sff8639 = 0x1F,
  PciExpressMini52WithKeepouts = 0x20,
  PciExpressMini52WithoutKeepouts = 0x21,
  PciExpressMini76 = 0x22,
  PciExpressGen4Sff8639 = 0x23,
  PciExpressGen5Sff8639 = 0x24,
  OcpNic30SmallFormFactor = 0x25,
  OcpNic30LargeFormFactor = 0x26,
  OcpNicPriorTo30 = 0x27,
  CxlFlexbus10 = 0x30,
  PC98Country = 0xA0,
  PC98C20 = 0xA1,
  PC98C24 = 0xA2,
  PC98E = 0xA3,
  PC98LocalBus = 0xA4,
  PC98Card = 0xA5,
  PciExpress = 0xA6,
  PciExpressX1 = 0xA7,
  PciExpressX2 = 0xA8,
  PciExpressX4 = 0xA9,
  PciExpressX8 = 0xAA,
  PciExpressX16 = 0xAB,
  PciExpressGen2 = 0xAC,
  PciExpressGen2X1 = 0xAD,
  PciExpressGen2X2 = 0xAE,
  PciExpressGen2X4 = 0xAF,
  PciExpressGen2X8 = 0xB0,
  PciExpressGen2X16 = 0xB1,
  PciExpressGen3 = 0xB2,
  PciExpressGen3X1 = 0xB3,
  PciExpressGen3X2 = 0xB4,
  PciExpressGen3X4 = 0xB5,
  PciExpressGen3X8 = 0xB6,
  PciExpressGen3X16 = 0xB7,
  PciExpressGen4 = 0xB9,
  PciExpressGen4X1 = 0xBA,
  PciExpressGen4X2 = 0xBB,
  PciExpressGen4X4 = 0xBC,
  PciExpressGen4X8 = 0xBD,
  PciExpressGen4X16 = 0xBE,
  PciExpressGen5 = 0xBF,
  PciExpressGen5X1 = 0xC0,
  PciExpressGen5X2 = 0xC1,
  PciExpressGen5X4 = 0xC2,
  PciExpressGen5X8 = 0xC3,
  PciExpressGen5X16 = 0xC4,
  PciExpressGen6AndBeyond = 0xC5,
  EnterpriseAndDatacenter1UE1 = 0xC6,
  EnterpriseAndDatacenter3InE3 = 0xC7,
}

/// <summary>DSP0134 §7.10.2 — Slot Data Bus Width.</summary>
public enum SlotDataBusWidth : byte {
  Other = 0x01,
  Unknown = 0x02,
  Bit8 = 0x03,
  Bit16 = 0x04,
  Bit32 = 0x05,
  Bit64 = 0x06,
  Bit128 = 0x07,
  X1 = 0x08,
  X2 = 0x09,
  X4 = 0x0A,
  X8 = 0x0B,
  X12 = 0x0C,
  X16 = 0x0D,
  X32 = 0x0E,
}

/// <summary>DSP0134 §7.10.3 — Current Usage.</summary>
public enum SlotUsage : byte {
  Other = 0x01,
  Unknown = 0x02,
  Available = 0x03,
  InUse = 0x04,
  Unavailable = 0x05,
}

/// <summary>DSP0134 §7.10.4 — Slot Length.</summary>
public enum SlotLength : byte {
  Other = 0x01,
  Unknown = 0x02,
  ShortLength = 0x03,
  LongLength = 0x04,
  DriveFormFactor25 = 0x05,
  DriveFormFactor35 = 0x06,
}

/// <summary>DSP0134 §7.10.6 — Slot Characteristics 1 (bitmask).</summary>
[Flags]
public enum SlotCharacteristics1 : byte {
  Unknown = 1 << 0,
  Provides5Volts = 1 << 1,
  Provides33Volts = 1 << 2,
  SharedSlot = 1 << 3,
  PcCard16Supported = 1 << 4,
  CardBusSupported = 1 << 5,
  ZoomVideoSupported = 1 << 6,
  ModemRingResumeSupported = 1 << 7,
}

/// <summary>DSP0134 §7.10.7 — Slot Characteristics 2 (bitmask, v2.1+).</summary>
[Flags]
public enum SlotCharacteristics2 : byte {
  PciMeSupported = 1 << 0,
  PciHotPlugSupported = 1 << 1,
  ProvidesPmeSupport = 1 << 2,
  HotPlugDeviceSupport = 1 << 3,
  SmbusSignalSupported = 1 << 4,
  BifurcationSupported = 1 << 5,
  SurpriseRemovalAsync = 1 << 6,
  FlexbusCxl10Capable = 1 << 7,
}

/// <summary>DSP0134 §7.10.15 — Slot Height (v3.4+).</summary>
public enum SlotHeight : byte {
  NotApplicable = 0x00,
  Other = 0x01,
  Unknown = 0x02,
  FullHeight = 0x03,
  LowProfile = 0x04,
}
