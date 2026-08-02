using System;

namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Voltage Probe Location Enumeration (DSP0134 §7.27.1)
/// </summary>
public enum VoltageProbeLocation : byte {
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
/// Voltage Probe Operational Status Enumeration (DSP0134 §7.27.1)
/// </summary>
public enum VoltageProbeStatus : byte {
  Other = 0x01,
  Unknown = 0x02,
  OK = 0x03,
  NonCritical = 0x04,
  Critical = 0x05,
  NonRecoverable = 0x06
}

/// <summary>
/// Type 26 — Voltage Probe Information (DSP0134 §7.27)
/// </summary>
public sealed class T026_VoltageProbeInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public string? Description { get; init; }
  public byte LocationAndStatusRaw { get; init; }
  public VoltageProbeLocation Location { get; init; }
  public VoltageProbeStatus Status { get; init; }
  public ushort MaximumValueMillivolts { get; init; }
  public ushort MinimumValueMillivolts { get; init; }
  public ushort ResolutionMillivolts { get; init; }
  public ushort ToleranceMillivolts { get; init; }
  public uint Accuracy { get; init; }
  public uint OEMDefined { get; init; }
  public ushort NominalValueMillivolts { get; init; }

  // High-utility properties handling special 0x8000 (Unknown) flags defined in spec
  public bool IsMaxValuedIdentifiable => MaximumValueMillivolts != 0x8000;
  public bool IsMinValuedIdentifiable => MinimumValueMillivolts != 0x8000;
  public bool IsNominalValuedIdentifiable => NominalValueMillivolts != 0x8000;

  internal static T026_VoltageProbeInformation Decode(SmbiosRawStructure s) {
    // DSP0134 §7.27: Description @0x04, Location and Status @0x05, Maximum @0x06,
    // Minimum @0x08, Resolution @0x0A, Tolerance @0x0C, Accuracy @0x0E,
    // OEM-defined DWORD @0x10, Nominal Value @0x14.
    byte locationStatusByte = s.ReadByte(0x05);

    // Bits 7:5 map the operational status
    VoltageProbeStatus status = (VoltageProbeStatus)((locationStatusByte >> 5) & 0x07);
    // Bits 4:0 map the structural placement classification
    VoltageProbeLocation location = (VoltageProbeLocation)(locationStatusByte & 0x1F);

    return new T026_VoltageProbeInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Description = s.GetString(s.ReadByte(0x04)),
      LocationAndStatusRaw = locationStatusByte,
      Location = location,
      Status = status,
      MaximumValueMillivolts = s.ReadWord(0x06),
      MinimumValueMillivolts = s.ReadWord(0x08),
      ResolutionMillivolts = s.ReadWord(0x0A),
      ToleranceMillivolts = s.ReadWord(0x0C),
      Accuracy = s.ReadWord(0x0E),
      OEMDefined = s.Length > 0x13 ? s.ReadDWord(0x10) : 0,
      NominalValueMillivolts = s.Length > 0x15 ? s.ReadWord(0x14) : (ushort)0x8000
    };
  }
}
