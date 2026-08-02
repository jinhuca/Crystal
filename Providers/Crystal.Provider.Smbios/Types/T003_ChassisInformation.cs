using System;

namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Chassis Lock State Indicator (DSP0134 §7.4.1)
/// </summary>
public enum ChassisLockState : byte {
  NotPresent = 0x00,
  Present = 0x01
}

/// <summary>
/// Physical Chassis Type Enumeration (DSP0134 §7.4.1)
/// </summary>
public enum PhysicalChassisType : byte {
  Other = 0x01,
  Unknown = 0x02,
  Desktop = 0x03,
  LowProfileDesktop = 0x04,
  PizzaBox = 0x05,
  MiniTower = 0x06,
  Tower = 0x07,
  Portable = 0x08,
  Laptop = 0x09,
  Notebook = 0x0A,
  HandHeld = 0x0B,
  DockingStation = 0x0C,
  AllInOne = 0x0D,
  SubNotebook = 0x0E,
  SpaceSaving = 0x0F,
  LunchBox = 0x10,
  MainServerChassis = 0x11,
  ExpansionChassis = 0x12,
  SubChassis = 0x13,
  BusExpansionChassis = 0x14,
  PeripheralChassis = 0x15,
  RAIDChassis = 0x16,
  RackMountChassis = 0x17,
  SealedCasePC = 0x18,
  MultiChassis = 0x19,
  CompactPCI = 0x1A,
  AdvancedTCA = 0x1B,
  Blade = 0x1C,
  BladeEnclosure = 0x1D,
  Tablet = 0x1E,
  Convertible = 0x1F,
  Detachable = 0x20,
  IoTGateway = 0x21,
  EmbeddedSystem = 0x22,
  MiniPC = 0x23,
  StickPC = 0x24
}

/// <summary>
/// Chassis State Enumeration (DSP0134 §7.4.2)
/// </summary>
public enum ChassisState : byte {
  Other = 0x01,
  Unknown = 0x02,
  Safe = 0x03,
  Warning = 0x04,
  Critical = 0x05,
  NonRecoverable = 0x06
}

/// <summary>
/// Chassis Security Status Enumeration (DSP0134 §7.4.3)
/// </summary>
public enum ChassisSecurityStatus : byte {
  Other = 0x01,
  Unknown = 0x02,
  None = 0x03,
  ExternalInterfaceLocked = 0x04,
  ExternalInterfaceDisabled = 0x05
}

/// <summary>
/// Type 3 — Chassis (or Enclosure) Information (DSP0134 §7.4)
/// </summary>
public sealed class T003_ChassisInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public string? Manufacturer { get; init; }
  public byte TypeRaw { get; init; }
  public PhysicalChassisType ChassisType { get; init; }
  public ChassisLockState LockState { get; init; }
  public string? Version { get; init; }
  public string? SerialNumber { get; init; }
  public string? AssetTag { get; init; }
  public ChassisState BootUpState { get; init; }
  public ChassisState PowerSupplyState { get; init; }
  public ChassisState ThermalState { get; init; }
  public ChassisSecurityStatus SecurityStatus { get; init; }
  public uint OEMDefined { get; init; }
  public byte Height { get; init; }
  public byte NumberOfPowerCords { get; init; }
  public byte ContainedElementCount { get; init; }
  public byte ContainedElementRecordLength { get; init; }
  public byte[] ContainedElements { get; init; } = Array.Empty<byte>();
  public string? SkuNumber { get; init; }

  // High-utility properties and boolean convenience flags
  public bool IsChassisLocked => LockState == ChassisLockState.Present;
  public bool HasHeightInfo => Height != 0;
  public bool HasPowerCordInfo => NumberOfPowerCords != 0;

  internal static T003_ChassisInformation Decode(SmbiosRawStructure s) {
    byte rawTypeByte = s.ReadByte(0x05);
    ChassisLockState lockState = (ChassisLockState)((rawTypeByte >> 7) & 0x01);
    PhysicalChassisType physicalType = (PhysicalChassisType)(rawTypeByte & 0x7F);

    // DSP0134 §7.4: Contained Element Count (n) @0x13, Record Length (m) @0x14,
    // Contained Elements (n*m) @0x15, SKU Number STRING @0x15+n*m.
    byte elemCount = s.Length > 0x13 ? s.ReadByte(0x13) : (byte)0;
    byte elemLength = s.Length > 0x14 ? s.ReadByte(0x14) : (byte)0;
    byte[] extractedElements = Array.Empty<byte>();

    if (elemCount > 0 && elemLength > 0) {
      int totalBytes = elemCount * elemLength;
      if (s.Length >= 0x15 + totalBytes) {
        extractedElements = new byte[totalBytes];
        for (int i = 0; i < totalBytes; i++) {
          extractedElements[i] = s.ReadByte(0x15 + i);
        }
      }
    }

    int skuOffset = 0x15 + (elemCount * elemLength);

    return new T003_ChassisInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Manufacturer = s.GetString(s.ReadByte(0x04)),
      TypeRaw = rawTypeByte,
      ChassisType = physicalType,
      LockState = lockState,
      Version = s.GetString(s.ReadByte(0x06)),
      SerialNumber = s.GetString(s.ReadByte(0x07)),
      AssetTag = s.GetString(s.ReadByte(0x08)),
      BootUpState = s.Length > 0x09 ? (ChassisState)s.ReadByte(0x09) : ChassisState.Unknown,
      PowerSupplyState = s.Length > 0x0A ? (ChassisState)s.ReadByte(0x0A) : ChassisState.Unknown,
      ThermalState = s.Length > 0x0B ? (ChassisState)s.ReadByte(0x0B) : ChassisState.Unknown,
      SecurityStatus = s.Length > 0x0C ? (ChassisSecurityStatus)s.ReadByte(0x0C) : ChassisSecurityStatus.Unknown,
      OEMDefined = s.Length > 0x10 ? (uint)s.ReadDWord(0x0D) : 0,
      Height = s.Length > 0x11 ? s.ReadByte(0x11) : (byte)0,
      NumberOfPowerCords = s.Length > 0x12 ? s.ReadByte(0x12) : (byte)0,
      ContainedElementCount = elemCount,
      ContainedElementRecordLength = elemLength,
      ContainedElements = extractedElements,
      SkuNumber = s.Length > skuOffset ? s.GetString(s.ReadByte(skuOffset)) : null
    };
  }
}
