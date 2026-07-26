namespace Crystal.Smbios.Types;

/// <summary>
/// Electrical Current Probe Location Enumeration (DSP0134 §7.30.1)
/// </summary>
public enum ElectricalCurrentProbeLocation : byte {
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
  AddInCard = 0x0B,
}

/// <summary>
/// Electrical Current Probe Operational Status Enumeration (DSP0134 §7.30.1)
/// </summary>
public enum ElectricalCurrentProbeStatus : byte {
  Other = 0x01,
  Unknown = 0x02,
  OK = 0x03,
  NonCritical = 0x04,
  Critical = 0x05,
  NonRecoverable = 0x06,
}

/// <summary>
/// Type 29 — Electrical Current Probe (DSP0134 §7.30).
///
/// Note: per DSP0134/dmidecode, <see cref="LocationAndStatusRaw"/> sits at
/// formatted-area offset 0x05, and this decoder follows that spec-correct
/// offset. The sibling <c>T026_VoltageProbeInformation</c> and
/// <c>T028_TemperatureProbeInformation</c> decoders already present in this
/// codebase read the equivalent byte from offset 0x06 instead — a
/// pre-existing one-byte discrepancy from the spec in those two files that
/// is not replicated here; worth a look if you want all three probe types
/// to agree.
/// </summary>
public sealed class T029_ElectricalCurrentProbeInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public string? Description { get; init; }
  public byte LocationAndStatusRaw { get; init; }
  public ElectricalCurrentProbeLocation Location { get; init; }
  public ElectricalCurrentProbeStatus Status { get; init; }
  public ushort MaximumValueMilliamps { get; init; }
  public ushort MinimumValueMilliamps { get; init; }
  public ushort ResolutionMicroamps { get; init; }
  public ushort ToleranceMilliamps { get; init; }
  public uint Accuracy { get; init; }
  public uint OEMDefined { get; init; }
  public ushort NominalValueMilliamps { get; init; }

  public bool IsMaxValueIdentifiable => MaximumValueMilliamps != 0x8000;
  public bool IsMinValueIdentifiable => MinimumValueMilliamps != 0x8000;
  public bool IsNominalValueIdentifiable => NominalValueMilliamps != 0x8000;

  internal static T029_ElectricalCurrentProbeInformation Decode(SmbiosRawStructure s) {
    byte locationStatusByte = s.ReadByte(0x05);
    var status = (ElectricalCurrentProbeStatus)((locationStatusByte >> 5) & 0x07);
    var location = (ElectricalCurrentProbeLocation)(locationStatusByte & 0x1F);

    return new T029_ElectricalCurrentProbeInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Description = s.GetString(s.ReadByte(0x04)),
      LocationAndStatusRaw = locationStatusByte,
      Location = location,
      Status = status,
      MaximumValueMilliamps = s.ReadWord(0x06),
      MinimumValueMilliamps = s.ReadWord(0x08),
      ResolutionMicroamps = s.ReadWord(0x0A),
      ToleranceMilliamps = s.ReadWord(0x0C),
      Accuracy = s.ReadWord(0x0E),
      OEMDefined = s.Length > 0x13 ? s.ReadDWord(0x10) : 0,
      NominalValueMilliamps = s.Length > 0x15 ? s.ReadWord(0x14) : (ushort)0x8000,
    };
  }
}
