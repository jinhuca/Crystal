using System;

namespace Crystal.Smbios.Types;

/// <summary>
/// Memory Device Form Factor Enumeration (DSP0134 §7.18.1)
/// </summary>
public enum MemoryFormFactor : byte {
  Other = 0x01,
  Unknown = 0x02,
  Simm = 0x03,
  Sipp = 0x04,
  ChIPs = 0x05,
  Dip = 0x06,
  Zip = 0x07,
  ProprietaryCard = 0x08,
  Dimm = 0x09,
  Tsop = 0x0A,
  RowOfChips = 0x0B,
  Rimm = 0x0C,
  Sodimm = 0x0D,
  Srimm = 0x0E,
  Fbqdimm = 0x0F, // Index 0x0F: FB-DIMM / 144-pin form factor configuration
  FbDimm = 0x10,
  Lrdimm = 0x11,
  MiniDimm = 0x12,
  MicroDimm = 0x13,
  Camm = 0x14
}

/// <summary>
/// Memory Device Type Enumeration (DSP0134 §7.18.2)
/// </summary>
public enum MemoryType : byte {
  Other = 0x01,
  Unknown = 0x02,
  Dram = 0x03,
  Edram = 0x04,
  Vram = 0x05,
  Sram = 0x06,
  Ram = 0x07,
  Rom = 0x08,
  Flash = 0x09,
  Eeprom = 0x0A,
  Feprom = 0x0B,
  Eprom = 0x0C,
  Cdram = 0x0D,
  ThreeDram = 0x0E, // Index 0x0E: 3D RAM configuration
  Sdram = 0x0F,
  Sgram = 0x10,
  Rdram = 0x11,
  Ddr = 0x12,
  Ddr2 = 0x13,
  Ddr2FbDimm = 0x14,
  Ddr3 = 0x18,
  Fbd2 = 0x19,
  Ddr4 = 0x1A,
  Lpddr = 0x1B,
  Lpddr2 = 0x1C,
  Lpddr3 = 0x1D,
  Lpddr4 = 0x1E,
  LogicalNonVolatileDevice = 0x1F,
  Ddr5 = 0x22,
  Lpddr5 = 0x23,
  Hbm = 0x24,
  Hbm2 = 0x25,
  Hbm3 = 0x26
}

/// <summary>
/// Memory Device Type Detail Flags (DSP0134 §7.18.3)
/// </summary>
[Flags]
public enum MemoryTypeDetail : ushort {
  None = 0x0000,
  Other = 0x0002,
  Unknown = 0x0004,
  FastPageMode = 0x0008,
  NibbleMode = 0x0010,
  StaticColumn = 0x0020,
  PseudoStatic = 0x0040,
  PipelinedNibble = 0x0080,
  Synchronous = 0x0100,
  Cmos = 0x0200,
  Edo = 0x0400,
  WindowDram = 0x0800,
  CacheDram = 0x1000,
  NonVolatile = 0x2000,
  Registered = 0x4000,
  Unbuffered = 0x8000
}

/// <summary>
/// Memory Device Technology Enumeration (DSP0134 §7.18.6)
/// </summary>
public enum MemoryTechnology : byte {
  Other = 0x01,
  Unknown = 0x02,
  Dram = 0x03,
  NvdimmN = 0x04,
  NvdimmF = 0x05,
  NvdimmP = 0x06,
  IntelOptaneDC = 0x07
}

/// <summary>
/// Type 17 — Memory Device Information (DSP0134 §7.18)
/// </summary>
public sealed class T017_MemoryDevice : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public ushort PhysicalMemoryArrayHandle { get; init; }
  public ushort MemoryErrorInformationHandle { get; init; }
  public ushort TotalWidthBits { get; init; }
  public ushort DataWidthBits { get; init; }
  public ushort SizeRaw { get; init; }
  public MemoryFormFactor FormFactor { get; init; }
  public byte DeviceSet { get; init; }
  public string? DeviceLocator { get; init; }
  public string? BankLocator { get; init; }
  public MemoryType Type { get; init; }
  public MemoryTypeDetail TypeDetail { get; init; }
  public ushort SpeedMts { get; init; }
  public string? Manufacturer { get; init; }
  public string? SerialNumber { get; init; }
  public string? AssetTag { get; init; }
  public string? PartNumber { get; init; }
  public byte Attributes { get; init; }
  public uint ExtendedSizeMegabytes { get; init; }
  public ushort ConfiguredMemorySpeedMts { get; init; }
  public ushort MinimumVoltageMillivolts { get; init; }
  public ushort MaximumVoltageMillivolts { get; init; }
  public ushort ConfiguredVoltageMillivolts { get; init; }
  public MemoryTechnology Technology { get; init; }

  // ── High Utility Computation Properties ───────────────────────────────────

  /// <summary>Checks if a physical RAM module is actively populated in this slot.</summary>
  public bool IsPopulated {
    get {
      // A slot is unpopulated if it is 0, or if it explicitly reports 0xFFFF (Unknown)
      if (SizeRaw == 0 || SizeRaw == 0xFFFF) return false;

      // If it overflows to the Extended Size field, verify that field isn't an unknown marker
      if (SizeRaw == 0x7FFF && ExtendedSizeMegabytes == 0x7FFFFFFF) return false;

      return true;
    }
  }


  /// <summary>Gets the memory capacity in bytes, properly handling DDR4/DDR5 high-capacity expansion sizes.</summary>
  public long CapacityBytes {
    get {
      if (!IsPopulated) return 0L;

      // Handle SMBIOS 2.7+ Extended Size Field (Triggers when SizeRaw == 0x7FFF)
      if (SizeRaw == 0x7FFF) {
        return (long)ExtendedSizeMegabytes * 1024L * 1024L;
      }

      // Bit 15 indicates Kilobyte units (0 = Megabytes, 1 = Kilobytes)
      bool isKb = (SizeRaw & 0x8000) != 0;
      long value = SizeRaw & 0x7FFF;

      return isKb ? value * 1024L : value * 1024L * 1024L;
    }
  }

  /// <summary>Gets the memory module rank configuration count extracted from structural attributes bitfields.</summary>
  public int RankCount {
    get {
      if (!IsPopulated) return 0;
      int ranks = Attributes & 0x0F;
      return ranks == 0 ? 1 : ranks; // Default to single rank if unspecified
    }
  }

  internal static T017_MemoryDevice Decode(SmbiosRawStructure s) {
    ushort sizeRaw = s.ReadWord(0x0C);

    return new T017_MemoryDevice {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      PhysicalMemoryArrayHandle = s.ReadWord(0x04),
      MemoryErrorInformationHandle = s.ReadWord(0x06),
      TotalWidthBits = s.ReadWord(0x08),
      DataWidthBits = s.ReadWord(0x0A),
      SizeRaw = sizeRaw,
      FormFactor = s.Length > 0x0E ? (MemoryFormFactor)s.ReadByte(0x0E) : MemoryFormFactor.Unknown,
      DeviceSet = s.Length > 0x0F ? s.ReadByte(0x0F) : (byte)0,
      DeviceLocator = s.Length > 0x10 ? s.GetString(s.ReadByte(0x10)) : null,
      BankLocator = s.Length > 0x11 ? s.GetString(s.ReadByte(0x11)) : null,
      Type = s.Length > 0x12 ? (MemoryType)s.ReadByte(0x12) : MemoryType.Unknown,
      TypeDetail = s.Length > 0x15 ? (MemoryTypeDetail)s.ReadWord(0x14) : MemoryTypeDetail.None,
      SpeedMts = s.Length > 0x17 ? s.ReadWord(0x16) : (ushort)0,
      Manufacturer = s.Length > 0x18 ? s.GetString(s.ReadByte(0x18)) : null,
      SerialNumber = s.Length > 0x19 ? s.GetString(s.ReadByte(0x19)) : null,
      AssetTag = s.Length > 0x1A ? s.GetString(s.ReadByte(0x1A)) : null,
      PartNumber = s.Length > 0x1B ? s.GetString(s.ReadByte(0x1B)) : null,
      Attributes = s.Length > 0x1C ? s.ReadByte(0x1C) : (byte)0,
      ExtendedSizeMegabytes = s.Length > 0x20 ? s.ReadDWord(0x1D) : 0,
      ConfiguredMemorySpeedMts = s.Length > 0x22 ? s.ReadWord(0x21) : (ushort)0,
      MinimumVoltageMillivolts = s.Length > 0x24 ? s.ReadWord(0x23) : (ushort)0,
      MaximumVoltageMillivolts = s.Length > 0x26 ? s.ReadWord(0x25) : (ushort)0,
      ConfiguredVoltageMillivolts = s.Length > 0x28 ? s.ReadWord(0x27) : (ushort)0,
      Technology = s.Length > 0x29 ? (MemoryTechnology)s.ReadByte(0x29) : MemoryTechnology.Unknown
    };
  }
}
