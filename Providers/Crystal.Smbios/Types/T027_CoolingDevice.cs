using System;

namespace Crystal.Smbios.Types;

/// <summary>
/// Cooling Device Type Enumeration (DSP0134 §7.28.1)
/// </summary>
public enum CoolingDeviceType : byte {
  Other = 0x01,
  Unknown = 0x02,
  Fan = 0x03,
  CentrifugalBlower = 0x04,
  ChipFan = 0x05,
  CabinetFan = 0x06,
  PowerSupplyFan = 0x07,
  HeatPipe = 0x08,
  IntegratedRefrigeration = 0x09,
  ActiveChilling = 0x0A,
  PassiveCooling = 0x0B,
  LiquidCooling = 0x0C
}

/// <summary>
/// Cooling Device Operational Status Enumeration (DSP0134 §7.28.1)
/// </summary>
public enum CoolingDeviceStatus : byte {
  Other = 0x01,
  Unknown = 0x02,
  OK = 0x03,
  NonCritical = 0x04,
  Critical = 0x05,
  NonRecoverable = 0x06
}

/// <summary>
/// Type 27 — Cooling Device Information (DSP0134 §7.28)
/// </summary>
public sealed class T027_CoolingDevice : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public ushort TemperatureProbeHandle { get; init; }
  public byte DeviceTypeAndStatusRaw { get; init; }
  public CoolingDeviceType DeviceType { get; init; }
  public CoolingDeviceStatus Status { get; init; }
  public byte CoolingUnitGroup { get; init; }
  public uint NominalSpeedRpm { get; init; }
  public string? Description { get; init; }

  // High-utility quick checks for system builders
  public bool IsSpeedIdentifiable => NominalSpeedRpm != 0x80000000;
  public bool HasAssociatedProbe => TemperatureProbeHandle != 0xFFFF;

  internal static T027_CoolingDevice Decode(SmbiosRawStructure s) {
    byte typeStatusByte = s.ReadByte(0x06);

    // Bits 7:5 map the operational status
    CoolingDeviceStatus status = (CoolingDeviceStatus)((typeStatusByte >> 5) & 0x07);
    // Bits 4:0 map the structural component classification
    CoolingDeviceType deviceType = (CoolingDeviceType)(typeStatusByte & 0x1F);

    return new T027_CoolingDevice {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      TemperatureProbeHandle = s.ReadWord(0x04),
      DeviceTypeAndStatusRaw = typeStatusByte,
      DeviceType = deviceType,
      Status = status,
      CoolingUnitGroup = s.Length > 0x07 ? s.ReadByte(0x07) : (byte)0,
      NominalSpeedRpm = s.Length > 0x0B ? s.ReadDWord(0x08) : 0x80000000, // 0x80000000 indicates Unknown in spec
      Description = s.Length > 0x0C ? s.GetString(s.ReadByte(0x0C)) : null
    };
  }
}
