namespace Crystal.Smbios.Types;

/// <summary>
/// Type 4 — Processor Information (DSP0134 §7.5)
/// </summary>
public sealed class T004_ProcessorInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public string? SocketDesignation { get; init; }
  public ProcessorType ProcessorType { get; init; }
  public byte ProcessorFamily { get; init; }
  public ushort ProcessorFamily2 { get; init; }
  public string? ProcessorManufacturer { get; init; }
  public ulong ProcessorId { get; init; }
  public string? ProcessorVersion { get; init; }
  public byte Voltage { get; init; }
  public ushort ExternalClockMhz { get; init; }
  public ushort MaxSpeedMhz { get; init; }
  public ushort CurrentSpeedMhz { get; init; }
  public byte Status { get; init; }
  public ProcessorUpgrade ProcessorUpgrade { get; init; }
  public ushort L1CacheHandle { get; init; }
  public ushort L2CacheHandle { get; init; }
  public ushort L3CacheHandle { get; init; }
  public string? SerialNumber { get; init; }
  public string? AssetTag { get; init; }
  public string? PartNumber { get; init; }
  public byte CoreCount { get; init; }
  public ushort CoreCount2 { get; init; }
  public byte CoreEnabled { get; init; }
  public ushort CoreEnabled2 { get; init; }
  public byte ThreadCount { get; init; }
  public ushort ThreadCount2 { get; init; }
  public ushort ProcessorCharacteristics { get; init; }

  public int LogicalCoreCount => CoreCount2 != 0 ? CoreCount2 : CoreCount == 0xFF ? 0 : CoreCount;
  public int LogicalThreadCount => ThreadCount2 != 0 ? ThreadCount2 : ThreadCount == 0xFF ? 0 : ThreadCount;
  public bool IsPopulated => (Status & 0x40) != 0;

  internal static T004_ProcessorInformation Decode(SmbiosRawStructure s) {
    return new T004_ProcessorInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      SocketDesignation = s.GetString(s.ReadByte(0x04)),
      ProcessorType = (ProcessorType)s.ReadByte(0x05),
      ProcessorFamily = s.ReadByte(0x06),
      ProcessorManufacturer = s.GetString(s.ReadByte(0x07)),
      ProcessorId = s.ReadQWord(0x08),
      ProcessorVersion = s.GetString(s.ReadByte(0x10)),
      Voltage = s.ReadByte(0x11),
      ExternalClockMhz = s.ReadWord(0x12),
      MaxSpeedMhz = s.ReadWord(0x14),
      CurrentSpeedMhz = s.ReadWord(0x16),
      Status = s.ReadByte(0x18),
      ProcessorUpgrade = (ProcessorUpgrade)s.ReadByte(0x19),
      L1CacheHandle = s.Length > 0x1B ? s.ReadWord(0x1A) : (ushort)0xFFFF,
      L2CacheHandle = s.Length > 0x1D ? s.ReadWord(0x1C) : (ushort)0xFFFF,
      L3CacheHandle = s.Length > 0x1F ? s.ReadWord(0x1E) : (ushort)0xFFFF,
      SerialNumber = s.Length > 0x20 ? s.GetString(s.ReadByte(0x20)) : null,
      AssetTag = s.Length > 0x21 ? s.GetString(s.ReadByte(0x21)) : null,
      PartNumber = s.Length > 0x22 ? s.GetString(s.ReadByte(0x22)) : null,
      CoreCount = s.Length > 0x23 ? s.ReadByte(0x23) : (byte)0,
      CoreEnabled = s.Length > 0x24 ? s.ReadByte(0x24) : (byte)0,
      ThreadCount = s.Length > 0x25 ? s.ReadByte(0x25) : (byte)0,
      ProcessorCharacteristics = s.Length > 0x27 ? s.ReadWord(0x26) : (ushort)0,
      ProcessorFamily2 = s.Length > 0x29 ? s.ReadWord(0x28) : (ushort)0,
      CoreCount2 = s.Length > 0x2B ? s.ReadWord(0x2A) : (ushort)0,
      CoreEnabled2 = s.Length > 0x2D ? s.ReadWord(0x2C) : (ushort)0,
    };
  }
}

// DSP0134 §7.5.1 — Processor type
public enum ProcessorType : byte {
  Other = 0x01,
  Unknown = 0x02,
  CentralProcessor = 0x03,
  MathProcessor = 0x04,
  DspProcessor = 0x05,
  VideoProcessor = 0x06,
}

// DSP0134 §7.5.5 — Processor upgrade socket
public enum ProcessorUpgrade : byte {
  Other = 0x01,
  Unknown = 0x02,
  DaughterBoard = 0x03,
  ZifSocket = 0x04,
  ReplacePiggyBack = 0x05,
  None = 0x06,
  LiF = 0x07,
  Slot1 = 0x08,
  Slot2 = 0x09,
  Socket370 = 0x0A,
  SlotA = 0x0B,
  SlotM = 0x0C,
  Socket423 = 0x0D,
  SocketA = 0x0E,
  Socket478 = 0x0F,
  Socket754 = 0x10,
  Socket940 = 0x11,
  Socket939 = 0x12,
  SocketmPGA604 = 0x13,
  SocketLGA771 = 0x14,
  SocketLGA775 = 0x15,
  SocketS1 = 0x16,
  SocketAM2 = 0x17,
  SocketF = 0x18,
  SocketLGA1366 = 0x19,
  SocketG34 = 0x1A,
  SocketAM3 = 0x1B,
  SocketC32 = 0x1C,
  SocketLGA1156 = 0x1D,
  SocketLGA1567 = 0x1E,
  SocketPGA988A = 0x1F,
  SocketBGA1288 = 0x20,
  SocketRPGA988B = 0x21,
  SocketBGA1023 = 0x22,
  SocketBGA1224 = 0x23,
  SocketLGA1155 = 0x24,
  SocketLGA1356 = 0x25,
  SocketLGA2011 = 0x26,
  SocketFS1 = 0x27,
  SocketFS2 = 0x28,
  SocketFM1 = 0x29,
  SocketFM2 = 0x2A,
  SocketLGA2011_3 = 0x2B,
  SocketLGA1356_3 = 0x2C,
  SocketLGA1150 = 0x2D,
  SocketBGA1168 = 0x2E,
  SocketBGA1234 = 0x2F,
  SocketBGA1364 = 0x30,
  SocketAM4 = 0x31,
  SocketLGA1151 = 0x32,
  SocketBGA1356 = 0x33,
  SocketBGA1440 = 0x34,
  SocketBGA1515 = 0x35,
  SocketLGA3647_1 = 0x36,
  SocketSP3 = 0x37,
  SocketSP3r2 = 0x38,
  SocketLGA2066 = 0x39,
  SocketBGA1392 = 0x3A,
  SocketBGA1510 = 0x3B,
  SocketBGA1528 = 0x3C,
  SocketLGA4189 = 0x3D,
  SocketLGA1200 = 0x3E,
  SocketLGA4677 = 0x3F,
  SocketLGA1700 = 0x40,
  SocketBGA1744 = 0x41,
  SocketBGA1781 = 0x42,
  SocketBGA1211 = 0x43,
  SocketBGA2422 = 0x44,
  SocketLGA1211 = 0x45,
  SocketLGA2422 = 0x46,
  SocketLGA5773 = 0x47,
  SocketBGA5773 = 0x48,
  SocketAM5 = 0x49,
  SocketSP5 = 0x4A,
  SocketSP6 = 0x4B,
  SocketBGA883 = 0x4C,
  SocketBGA1190 = 0x4D,
  SocketBGA4129 = 0x4E,
  SocketLGA4710 = 0x4F,
  SocketLGA7529 = 0x50,
}
