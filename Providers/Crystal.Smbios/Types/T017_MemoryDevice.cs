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
  Hbm = 0x20,
  Hbm2 = 0x21,
  Ddr5 = 0x22,
  Lpddr5 = 0x23,
  Hbm3 = 0x24,
  Mrdimm = 0x25
}

/// <summary>
/// Memory Device Type Detail Flags (DSP0134 §7.18.3)
/// </summary>
[Flags]
public enum MemoryTypeDetail : ushort {
  None = 0x0000,
  Other = 0x0002,          // bit 1
  Unknown = 0x0004,        // bit 2
  FastPaged = 0x0008,      // bit 3
  StaticColumn = 0x0010,   // bit 4
  PseudoStatic = 0x0020,   // bit 5
  Rambus = 0x0040,         // bit 6
  Synchronous = 0x0080,    // bit 7
  Cmos = 0x0100,           // bit 8
  Edo = 0x0200,            // bit 9
  WindowDram = 0x0400,     // bit 10
  CacheDram = 0x0800,      // bit 11
  NonVolatile = 0x1000,    // bit 12
  Registered = 0x2000,     // bit 13 (Registered / Buffered)
  Unbuffered = 0x4000,     // bit 14 (Unbuffered / Unregistered)
  Lrdimm = 0x8000          // bit 15
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

  // ── SMBIOS 3.2 additions ──────────────────────────────────────────────────
  public ushort MemoryOperatingModeCapability { get; init; }
  public string? FirmwareVersion { get; init; }
  public ushort ModuleManufacturerId { get; init; }
  public ushort ModuleProductId { get; init; }
  public ushort MemorySubsystemControllerManufacturerId { get; init; }
  public ushort MemorySubsystemControllerProductId { get; init; }
  public ulong NonVolatileSizeBytes { get; init; }
  public ulong VolatileSizeBytes { get; init; }
  public ulong CacheSizeBytes { get; init; }
  public ulong LogicalSizeBytes { get; init; }

  // ── SMBIOS 3.3 additions ──────────────────────────────────────────────────
  public uint ExtendedSpeedMts { get; init; }
  public uint ExtendedConfiguredMemorySpeedMts { get; init; }

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
      // DSP0134 §7.18: Type Detail WORD @0x13, Speed WORD @0x15, strings @0x17-0x1A,
      // Attributes @0x1B, Extended Size DWORD @0x1C, Configured Speed @0x20,
      // voltages @0x22/0x24/0x26, Technology @0x28.
      TypeDetail = s.Length > 0x14 ? (MemoryTypeDetail)s.ReadWord(0x13) : MemoryTypeDetail.None,
      SpeedMts = s.Length > 0x16 ? s.ReadWord(0x15) : (ushort)0,
      Manufacturer = s.Length > 0x17 ? s.GetString(s.ReadByte(0x17)) : null,
      SerialNumber = s.Length > 0x18 ? s.GetString(s.ReadByte(0x18)) : null,
      AssetTag = s.Length > 0x19 ? s.GetString(s.ReadByte(0x19)) : null,
      PartNumber = s.Length > 0x1A ? s.GetString(s.ReadByte(0x1A)) : null,
      Attributes = s.Length > 0x1B ? s.ReadByte(0x1B) : (byte)0,
      ExtendedSizeMegabytes = s.Length > 0x1F ? s.ReadDWord(0x1C) : 0,
      ConfiguredMemorySpeedMts = s.Length > 0x21 ? s.ReadWord(0x20) : (ushort)0,
      MinimumVoltageMillivolts = s.Length > 0x23 ? s.ReadWord(0x22) : (ushort)0,
      MaximumVoltageMillivolts = s.Length > 0x25 ? s.ReadWord(0x24) : (ushort)0,
      ConfiguredVoltageMillivolts = s.Length > 0x27 ? s.ReadWord(0x26) : (ushort)0,
      Technology = s.Length > 0x28 ? (MemoryTechnology)s.ReadByte(0x28) : MemoryTechnology.Unknown,
      // SMBIOS 3.2 fields
      MemoryOperatingModeCapability = s.Length > 0x2A ? s.ReadWord(0x29) : (ushort)0,
      FirmwareVersion = s.Length > 0x2B ? s.GetString(s.ReadByte(0x2B)) : null,
      ModuleManufacturerId = s.Length > 0x2D ? s.ReadWord(0x2C) : (ushort)0,
      ModuleProductId = s.Length > 0x2F ? s.ReadWord(0x2E) : (ushort)0,
      MemorySubsystemControllerManufacturerId = s.Length > 0x31 ? s.ReadWord(0x30) : (ushort)0,
      MemorySubsystemControllerProductId = s.Length > 0x33 ? s.ReadWord(0x32) : (ushort)0,
      NonVolatileSizeBytes = s.Length > 0x3B ? s.ReadQWord(0x34) : 0UL,
      VolatileSizeBytes = s.Length > 0x43 ? s.ReadQWord(0x3C) : 0UL,
      CacheSizeBytes = s.Length > 0x4B ? s.ReadQWord(0x44) : 0UL,
      LogicalSizeBytes = s.Length > 0x53 ? s.ReadQWord(0x4C) : 0UL,
      // SMBIOS 3.3 fields
      ExtendedSpeedMts = s.Length > 0x57 ? s.ReadDWord(0x54) : 0u,
      ExtendedConfiguredMemorySpeedMts = s.Length > 0x5B ? s.ReadDWord(0x58) : 0u
    };
  }
}
