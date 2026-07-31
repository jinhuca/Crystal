namespace Crystal.Smbios.Types;

/// <summary>DSP0134 §7.40.1 — DMTF Power Supply Type (Characteristics bits 5:3).</summary>
public enum PowerSupplyType : byte {
  Other = 0x01,
  Unknown = 0x02,
  Linear = 0x03,
  Switching = 0x04,
  Battery = 0x05,
  Ups = 0x06,
  Converter = 0x07,
  Regulator = 0x08
}

/// <summary>DSP0134 §7.40.1 — Power Supply Status (Characteristics bits 8:6).</summary>
public enum PowerSupplyStatus : byte {
  Other = 0x01,
  Unknown = 0x02,
  OK = 0x03,
  NonCritical = 0x04,
  Critical = 0x05
}

/// <summary>DSP0134 §7.40.1 — DMTF Input Voltage Range Switching (Characteristics bits 12:9).</summary>
public enum PowerSupplyInputVoltageRangeSwitching : byte {
  Other = 0x01,
  Unknown = 0x02,
  Manual = 0x03,
  AutoSwitch = 0x04,
  WideRange = 0x05,
  NotApplicable = 0x06
}

/// <summary>
/// Type 39 — System Power Supply (DSP0134 §7.40)
/// </summary>
public sealed class T039_SystemPowerSupply : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>Power unit group this supply belongs to (redundancy grouping).</summary>
  public byte PowerUnitGroup { get; init; }

  public string? Location { get; init; }
  public string? DeviceName { get; init; }
  public string? Manufacturer { get; init; }
  public string? SerialNumber { get; init; }
  public string? AssetTagNumber { get; init; }
  public string? ModelPartNumber { get; init; }
  public string? RevisionLevel { get; init; }

  /// <summary>Maximum sustained power output in watts; 0x8000 = unknown.</summary>
  public ushort MaxPowerCapacityWatts { get; init; }

  /// <summary>Raw Power Supply Characteristics bitfield (WORD).</summary>
  public ushort CharacteristicsRaw { get; init; }

  public ushort InputVoltageProbeHandle { get; init; }
  public ushort CoolingDeviceHandle { get; init; }
  public ushort InputCurrentProbeHandle { get; init; }

  // Decoded Characteristics bitfield (DSP0134 §7.40.1)
  public bool IsHotReplaceable => (CharacteristicsRaw & 0x0001) != 0;
  public bool IsPresent => (CharacteristicsRaw & 0x0002) != 0;
  public bool IsUnplugged => (CharacteristicsRaw & 0x0004) != 0;
  public PowerSupplyType SupplyType => (PowerSupplyType)((CharacteristicsRaw >> 3) & 0x07);
  public PowerSupplyStatus Status => (PowerSupplyStatus)((CharacteristicsRaw >> 6) & 0x07);
  public PowerSupplyInputVoltageRangeSwitching InputVoltageRangeSwitching =>
    (PowerSupplyInputVoltageRangeSwitching)((CharacteristicsRaw >> 9) & 0x0F);

  public bool IsMaxPowerKnown => MaxPowerCapacityWatts != 0x8000;

  internal static T039_SystemPowerSupply Decode(SmbiosRawStructure s) {
    // DSP0134 §7.40: Power Unit Group @0x04; Location/Device Name/Manufacturer/
    // Serial Number/Asset Tag/Model Part Number/Revision Level strings @0x05-0x0B;
    // Max Power Capacity WORD @0x0C; Power Supply Characteristics WORD @0x0E;
    // Input Voltage Probe Handle @0x10; Cooling Device Handle @0x12;
    // Input Current Probe Handle @0x14 (v2.3.1).
    return new T039_SystemPowerSupply {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      PowerUnitGroup = s.Length > 0x04 ? s.ReadByte(0x04) : (byte)0,
      Location = s.Length > 0x05 ? s.GetString(s.ReadByte(0x05)) : null,
      DeviceName = s.Length > 0x06 ? s.GetString(s.ReadByte(0x06)) : null,
      Manufacturer = s.Length > 0x07 ? s.GetString(s.ReadByte(0x07)) : null,
      SerialNumber = s.Length > 0x08 ? s.GetString(s.ReadByte(0x08)) : null,
      AssetTagNumber = s.Length > 0x09 ? s.GetString(s.ReadByte(0x09)) : null,
      ModelPartNumber = s.Length > 0x0A ? s.GetString(s.ReadByte(0x0A)) : null,
      RevisionLevel = s.Length > 0x0B ? s.GetString(s.ReadByte(0x0B)) : null,
      MaxPowerCapacityWatts = s.Length > 0x0D ? s.ReadWord(0x0C) : (ushort)0x8000,
      CharacteristicsRaw = s.Length > 0x0F ? s.ReadWord(0x0E) : (ushort)0,
      InputVoltageProbeHandle = s.Length > 0x11 ? s.ReadWord(0x10) : (ushort)0xFFFF,
      CoolingDeviceHandle = s.Length > 0x13 ? s.ReadWord(0x12) : (ushort)0xFFFF,
      InputCurrentProbeHandle = s.Length > 0x15 ? s.ReadWord(0x14) : (ushort)0xFFFF,
    };
  }
}
