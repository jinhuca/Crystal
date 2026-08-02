namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 41 — describes one onboard device built into the system board
/// (integrated NIC, audio codec, onboard NVMe/SATA controller, etc.).
/// Like Type 9 (System Slots), these structures are standalone — they do
/// not reference or get referenced by another structure's handle.
///
/// Supersedes the obsolete Type 10 (On Board Devices), which only recorded
/// device type and description without PCI bus addressing.
/// </summary>
public sealed class T041_OnboardDeviceExtendedInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>Human-readable label, e.g. "Onboard LAN", "Onboard Audio".</summary>
  public string? ReferenceDesignation { get; init; }

  /// <summary>Device category (lower 7 bits of the raw Device Type byte).</summary>
  public OnboardDeviceType DeviceType { get; init; }

  /// <summary>True when the device is enabled; false when disabled (e.g. in BIOS setup).</summary>
  public bool IsEnabled { get; init; }

  /// <summary>1-based instance number distinguishing multiple devices of the same type.</summary>
  public byte DeviceTypeInstance { get; init; }

  /// <summary>PCI segment group number; 0 on single-segment systems.</summary>
  public ushort SegmentGroupNumber { get; init; }
  /// <summary>PCI bus number of this device.</summary>
  public byte BusNumber { get; init; }
  /// <summary>Bits 7-3: device number; bits 2-0: function number.</summary>
  public byte DeviceFunctionNumber { get; init; }

  /// <summary>PCI device number (upper 5 bits of <see cref="DeviceFunctionNumber"/>).</summary>
  public int DeviceNumber => DeviceFunctionNumber >> 3;
  /// <summary>PCI function number (lower 3 bits of <see cref="DeviceFunctionNumber"/>).</summary>
  public int FunctionNumber => DeviceFunctionNumber & 0b111;

  internal static T041_OnboardDeviceExtendedInformation Decode(SmbiosRawStructure s) {
    // DSP0134 §7.42 formatted-area layout:
    // 04 ReferenceDesignation      STRING
    // 05 DeviceType                BYTE   (bit7 = enabled, bits6-0 = OnboardDeviceType)
    // 06 DeviceTypeInstance        BYTE
    // 07 SegmentGroupNumber        WORD
    // 09 BusNumber                 BYTE
    // 0A DeviceFunctionNumber      BYTE
    byte deviceTypeRaw = s.ReadByte(0x05);

    return new T041_OnboardDeviceExtendedInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      ReferenceDesignation = s.GetString(s.ReadByte(0x04)),
      DeviceType = (OnboardDeviceType)(deviceTypeRaw & 0x7F),
      IsEnabled = (deviceTypeRaw & 0x80) != 0,
      DeviceTypeInstance = s.ReadByte(0x06),
      SegmentGroupNumber = s.Length > 0x08 ? s.ReadWord(0x07) : (ushort)0,
      BusNumber = s.Length > 0x09 ? s.ReadByte(0x09) : (byte)0,
      DeviceFunctionNumber = s.Length > 0x0A ? s.ReadByte(0x0A) : (byte)0,
    };
  }
}

// ── Type 41 — Onboard Devices Extended Information enums (DSP0134 §7.42) ────

/// <summary>
/// DSP0134 §7.42.2 — Device Type (lower 7 bits of the Device Type byte;
/// bit 7 is the separate "device enabled" flag, see
/// <see cref="T041_OnboardDeviceExtendedInformation.IsEnabled"/>).
/// </summary>
public enum OnboardDeviceType : byte {
  Other = 0x01,
  Unknown = 0x02,
  Video = 0x03,
  ScsiController = 0x04,
  Ethernet = 0x05,
  TokenRing = 0x06,
  Sound = 0x07,
  PataController = 0x08,
  SataController = 0x09,
  SasController = 0x0A,
  WirelessLan = 0x0B,
  Bluetooth = 0x0C,
  Wwan = 0x0D,
  Emmc = 0x0E,
  NvmeController = 0x0F,
  UfsController = 0x10,
}