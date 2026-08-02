using System;
using System.Collections.Generic;

namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Management Controller Host Interface Type Enumeration (DSP0134 §7.43.1).
/// 0x02-0x0F are MCTP-defined host interfaces (DSP0239); 0x40 is the
/// Network Host Interface (DSP0270); 0xF0 is OEM-defined.
/// </summary>
public enum ManagementControllerHostInterfaceType : byte {
  Kcs = 0x02,
  Uart8250 = 0x03,
  Uart16450 = 0x04,
  Uart16550_16550A = 0x05,
  Uart16650_16650A = 0x06,
  Uart16750_16750A = 0x07,
  Uart16850_16850A = 0x08,
  I2CSmbus = 0x09,
  I3C = 0x0A,
  PCIeVdm = 0x0B,
  Mmbi = 0x0C,
  Pcc = 0x0D,
  UCIe = 0x0E,
  Usb = 0x0F,
  NetworkHostInterface = 0x40,
  OemDefined = 0xF0,
}

/// <summary>DSP0134 §7.43.2 — Protocol Record Type.</summary>
public enum ManagementControllerProtocolType : byte {
  Ipmi = 0x02,
  Mctp = 0x03,
  RedfishOverIp = 0x04,
  OemDefined = 0xF0,
}

/// <summary>
/// A single Protocol Record within a Type 42 structure (DSP0134 §7.43.2):
/// a Protocol Type byte followed by a length-prefixed type-specific data blob.
/// </summary>
public sealed class ManagementControllerProtocolRecord {
  public byte ProtocolTypeRaw { get; init; }
  public ManagementControllerProtocolType? ProtocolType =>
      Enum.IsDefined(typeof(ManagementControllerProtocolType), ProtocolTypeRaw)
          ? (ManagementControllerProtocolType)ProtocolTypeRaw
          : null;

  public byte ProtocolTypeSpecificDataLength { get; init; }
  public IReadOnlyList<byte> ProtocolTypeSpecificData { get; init; } = Array.Empty<byte>();
}

/// <summary>
/// Type 42 — Management Controller Host Interface (DSP0134 §7.43).
/// Describes a management controller host interface not discoverable via
/// Plug-and-Play. This decoder exposes the common header (interface type and
/// its type-specific data blob) plus the Protocol Records region. The
/// interface-type-specific payload (e.g. DSP0270 for the Network Host
/// Interface) is left as raw bytes for callers who need to dig further.
/// </summary>
public sealed class T042_ManagementControllerHostInterface : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public byte InterfaceTypeRaw { get; init; }
  public ManagementControllerHostInterfaceType? InterfaceType =>
      Enum.IsDefined(typeof(ManagementControllerHostInterfaceType), InterfaceTypeRaw)
          ? (ManagementControllerHostInterfaceType)InterfaceTypeRaw
          : null;

  /// <summary>Length in bytes of <see cref="InterfaceTypeSpecificData"/>.</summary>
  public byte InterfaceTypeSpecificDataLength { get; init; }

  /// <summary>Raw interface-type-specific data (DSP0270 for Network Host Interface, etc).</summary>
  public IReadOnlyList<byte> InterfaceTypeSpecificData { get; init; } = Array.Empty<byte>();

  /// <summary>Number of protocol records reported by the structure.</summary>
  public byte ProtocolRecordCount { get; init; }

  /// <summary>Decoded protocol records (each: type + length-prefixed data).</summary>
  public IReadOnlyList<ManagementControllerProtocolRecord> ProtocolRecords { get; init; }
      = Array.Empty<ManagementControllerProtocolRecord>();

  internal static T042_ManagementControllerHostInterface Decode(SmbiosRawStructure s) {
    // DSP0134 §7.43: Interface Type @0x04; Interface-Type-Specific Data Length
    // BYTE @0x05; Interface-Type-Specific Data @0x06; then Number of Protocol
    // Records BYTE, followed by that many Protocol Records (each: Protocol Type
    // BYTE, Protocol-Type-Specific Data Length BYTE, Data) (v3.2).
    byte interfaceType = s.ReadByte(0x04);
    byte specLength = s.Length > 0x05 ? s.ReadByte(0x05) : (byte)0;

    int specEnd = 0x06 + specLength;
    var specData = Array.Empty<byte>();
    if (specLength > 0 && s.Length >= specEnd) {
      specData = new byte[specLength];
      for (int i = 0; i < specLength; i++) specData[i] = s.ReadByte(0x06 + i);
    }

    byte recordCount = 0;
    var records = new List<ManagementControllerProtocolRecord>();
    int cursor = specEnd;
    if (s.Length > cursor) {
      recordCount = s.ReadByte(cursor);
      cursor += 1;
      for (int r = 0; r < recordCount && cursor + 1 < s.Length + 1 && cursor < s.Length; r++) {
        byte protocolType = s.ReadByte(cursor);
        byte dataLength = (cursor + 1) < s.Length ? s.ReadByte(cursor + 1) : (byte)0;
        int dataStart = cursor + 2;

        var data = Array.Empty<byte>();
        if (dataLength > 0 && s.Length >= dataStart + dataLength) {
          data = new byte[dataLength];
          for (int i = 0; i < dataLength; i++) data[i] = s.ReadByte(dataStart + i);
        }

        records.Add(new ManagementControllerProtocolRecord {
          ProtocolTypeRaw = protocolType,
          ProtocolTypeSpecificDataLength = dataLength,
          ProtocolTypeSpecificData = data,
        });

        cursor = dataStart + dataLength;
      }
    }

    return new T042_ManagementControllerHostInterface {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      InterfaceTypeRaw = interfaceType,
      InterfaceTypeSpecificDataLength = specLength,
      InterfaceTypeSpecificData = specData,
      ProtocolRecordCount = recordCount,
      ProtocolRecords = records,
    };
  }
}
