using System;

namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Temperature Probe Location Enumeration (DSP0134 §7.29.1)
/// </summary>
public enum TemperatureProbeLocation : byte {
  Other = 0x01,
  Unknown = 0x02,
  Processor = 0x03,
  Disk = 0x04,
  PeripheralBay = 0x05,
  SystemManagementModule = 0x06,
  Motherboard = 0x07,
  MemoryModule = 0x08,
  ProcessorModule = 0x09,
  PowerUnit = 0x0A,
  AddInCard = 0x0B
}

/// <summary>
/// Temperature Probe Operational Status Enumeration (DSP0134 §7.29.1)
/// </summary>
public enum TemperatureProbeStatus : byte {
  Other = 0x01,
  Unknown = 0x02,
  OK = 0x03,
  NonCritical = 0x04,
  Critical = 0x05,
  NonRecoverable = 0x06
}

/// <summary>
/// Type 28 — Temperature Probe Information (DSP0134 §7.29)
/// </summary>
public sealed class T028_TemperatureProbeInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public string? Description { get; init; }
  public byte LocationAndStatusRaw { get; init; }
  public TemperatureProbeLocation Location { get; init; }
  public TemperatureProbeStatus Status { get; init; }

  // Temperature values are stored in 1/10th degrees C (DSP0134 §7.29)
  public short MaximumValueRaw { get; init; }
  public short MinimumValueRaw { get; init; }
  public ushort ResolutionRaw { get; init; }
  public ushort ToleranceRaw { get; init; }
  public ushort Accuracy { get; init; }
  public uint OEMDefined { get; init; }
  public short NominalValueRaw { get; init; }

  // High-utility properties converting raw 1/10th °C integers into user-friendly floating-point degrees Celsius
  public double? MaximumValueCelsius => MaximumValueRaw != unchecked((short)0x8000) ? MaximumValueRaw / 10.0 : null;
  public double? MinimumValueCelsius => MinimumValueRaw != unchecked((short)0x8000) ? MinimumValueRaw / 10.0 : null;
  public double? NominalValueCelsius => NominalValueRaw != unchecked((short)0x8000) ? NominalValueRaw / 10.0 : null;

  public double? ResolutionCelsius => ResolutionRaw != 0x8000 ? ResolutionRaw / 10.0 : null;
  public double? ToleranceCelsius => ToleranceRaw != 0x8000 ? ToleranceRaw / 10.0 : null;

  public static T028_TemperatureProbeInformation Decode(SmbiosRawStructure s) {
    // DSP0134 §7.29: Description @0x04, Location and Status @0x05, Maximum @0x06,
    // Minimum @0x08, Resolution @0x0A, Tolerance @0x0C, Accuracy @0x0E,
    // OEM-defined DWORD @0x10, Nominal Value @0x14.
    byte locationStatusByte = s.ReadByte(0x05);

    // Bits 7:5 map the operational status
    TemperatureProbeStatus status = (TemperatureProbeStatus)((locationStatusByte >> 5) & 0x07);
    // Bits 4:0 map the structural placement classification
    TemperatureProbeLocation location = (TemperatureProbeLocation)(locationStatusByte & 0x1F);

    return new T028_TemperatureProbeInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Description = s.GetString(s.ReadByte(0x04)),
      LocationAndStatusRaw = locationStatusByte,
      Location = location,
      Status = status,
      MaximumValueRaw = (short)s.ReadWord(0x06),
      MinimumValueRaw = (short)s.ReadWord(0x08),
      ResolutionRaw = s.ReadWord(0x0A),
      ToleranceRaw = s.ReadWord(0x0C),
      Accuracy = s.ReadWord(0x0E),
      OEMDefined = s.Length > 0x13 ? s.ReadDWord(0x10) : 0,
      NominalValueRaw = s.Length > 0x15 ? (short)s.ReadWord(0x14) : unchecked((short)0x8000) // 0x8000 indicates Unknown in spec
    };
  }
}
