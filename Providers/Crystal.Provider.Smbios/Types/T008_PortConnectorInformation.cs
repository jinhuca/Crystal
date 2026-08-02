namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 8 — describes one physical port connector (USB, HDMI, headphone jack,
/// PS/2, RJ-45, etc.), with separate internal (motherboard-side) and external
/// (chassis-side) reference designators and connector types. Like Types 9 and
/// 41, this structure is standalone — it does not reference or get referenced
/// by another structure's handle.
/// </summary>
public sealed class T008_PortConnectorInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>Internal (motherboard header) label, e.g. "J1", "Front Panel USB".</summary>
  public string? InternalReferenceDesignator { get; init; }
  /// <summary>Connector type on the internal (motherboard) side.</summary>
  public PortConnectorType InternalConnectorType { get; init; }

  /// <summary>External (chassis-facing) label, e.g. "USB 3.0 Port 1", "HDMI-OUT".</summary>
  public string? ExternalReferenceDesignator { get; init; }
  /// <summary>Connector type on the external (chassis) side.</summary>
  public PortConnectorType ExternalConnectorType { get; init; }

  /// <summary>Functional port type (e.g. USB, Serial, Video, Audio).</summary>
  public PortType PortType { get; init; }

  internal static T008_PortConnectorInformation Decode(SmbiosRawStructure s) {
    // DSP0134 §7.9 formatted-area layout:
    // 04 InternalReferenceDesignator   STRING
    // 05 InternalConnectorType         BYTE
    // 06 ExternalReferenceDesignator   STRING
    // 07 ExternalConnectorType         BYTE
    // 08 PortType                      BYTE
    return new T008_PortConnectorInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      InternalReferenceDesignator = s.GetString(s.ReadByte(0x04)),
      InternalConnectorType = (PortConnectorType)s.ReadByte(0x05),
      ExternalReferenceDesignator = s.GetString(s.ReadByte(0x06)),
      ExternalConnectorType = (PortConnectorType)s.ReadByte(0x07),
      PortType = s.Length > 0x08 ? (PortType)s.ReadByte(0x08) : PortType.None,
    };
  }
}

// ── Type 8 — Port Connector Information enums (DSP0134 §7.9) ────────────────

/// <summary>
/// DSP0134 §7.9.2 — Connector Type. Used for both the internal and external
/// connector fields; the same enum applies to both.
/// </summary>
public enum PortConnectorType : byte {
  None = 0x00,
  Centronics = 0x01,
  MiniCentronics = 0x02,
  Proprietary = 0x03,
  Db25PinMale = 0x04,
  Db25PinFemale = 0x05,
  Db15PinMale = 0x06,
  Db15PinFemale = 0x07,
  Db9PinMale = 0x08,
  Db9PinFemale = 0x09,
  Rj11 = 0x0A,
  Rj45 = 0x0B,
  MiniScsi50Pin = 0x0C,
  MiniDin = 0x0D,
  MicroDin = 0x0E,
  Ps2 = 0x0F,
  Infrared = 0x10,
  HpHil = 0x11,
  AccessBusUsb = 0x12,
  SsaScsi = 0x13,
  CircularDin8Male = 0x14,
  CircularDin8Female = 0x15,
  OnBoardIde = 0x16,
  OnBoardFloppy = 0x17,
  DualInline9Pin = 0x18,
  DualInline25Pin = 0x19,
  DualInline50Pin = 0x1A,
  DualInline68Pin = 0x1B,
  OnBoardSoundInputFromCdRom = 0x1C,
  MiniCentronicsType14 = 0x1D,
  MiniCentronicsType26 = 0x1E,
  MiniJackHeadphones = 0x1F,
  Bnc = 0x20,
  Ieee1394 = 0x21,
  SasSataPlugReceptacle = 0x22,
  UsbTypeCReceptacle = 0x23,
  Pc98 = 0xA0,
  Pc98Hireso = 0xA1,
  PcH98 = 0xA2,
  Pc98Note = 0xA3,
  Pc98Full = 0xA4,
  Other = 0xFF,
}

/// <summary>DSP0134 §7.9.3 — Port Type.</summary>
public enum PortType : byte {
  None = 0x00,
  ParallelPortXtAtCompatible = 0x01,
  ParallelPortPs2 = 0x02,
  ParallelPortEcp = 0x03,
  ParallelPortEpp = 0x04,
  ParallelPortEcpEpp = 0x05,
  SerialPortXtAtCompatible = 0x06,
  SerialPort16450Compatible = 0x07,
  SerialPort16550Compatible = 0x08,
  SerialPort16550ACompatible = 0x09,
  ScsiPort = 0x0A,
  MidiPort = 0x0B,
  JoystickPort = 0x0C,
  KeyboardPort = 0x0D,
  MousePort = 0x0E,
  SsaScsi = 0x0F,
  Usb = 0x10,
  FireWire = 0x11,
  PcmciaTypeI = 0x12,
  PcmciaTypeII = 0x13,
  PcmciaTypeIII = 0x14,
  Cardbus = 0x15,
  AccessBusPort = 0x16,
  ScsiII = 0x17,
  ScsiWide = 0x18,
  Pc98 = 0x19,
  Pc98Hireso = 0x1A,
  PcH98 = 0x1B,
  VideoPort = 0x1C,
  AudioPort = 0x1D,
  ModemPort = 0x1E,
  NetworkPort = 0x1F,
  Sata = 0x20,
  Sas = 0x21,
  Mfdp = 0x22,
  Thunderbolt = 0x23,
  Compatible8251 = 0xA0,
  Compatible8251Fifo = 0xA1,
  Other = 0xFF,
}
