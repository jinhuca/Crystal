using System;
using System.Collections.Generic;

namespace Crystal.Smbios.Types;

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

/// <summary>
/// Type 42 — Management Controller Host Interface (DSP0134 §7.43).
/// Describes a management controller host interface not discoverable via
/// Plug-and-Play. This decoder exposes the common header (interface type
/// and its type-specific data blob); the type-specific payload for
/// <see cref="ManagementControllerHostInterfaceType.NetworkHostInterface"/>
/// (DSP0270) and the trailing Protocol Records region are left as raw
/// bytes for callers who need to dig further — dmidecode itself only
/// partially decodes this structure for the same reason.
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

  /// <summary>Any bytes after the interface-type-specific data (the Protocol Records region), left undecoded.</summary>
  public IReadOnlyList<byte> TrailingBytes { get; init; } = Array.Empty<byte>();

  internal static T042_ManagementControllerHostInterface Decode(SmbiosRawStructure s) {
    byte interfaceType = s.ReadByte(0x04);
    byte specLength = s.Length > 0x05 ? s.ReadByte(0x05) : (byte)0;

    int specEnd = 0x06 + specLength;
    var specData = Array.Empty<byte>();
    if (specLength > 0 && s.Length >= specEnd) {
      specData = new byte[specLength];
      for (int i = 0; i < specLength; i++) specData[i] = s.ReadByte(0x06 + i);
    }

    var trailing = Array.Empty<byte>();
    if (s.Length > specEnd) {
      int trailingLength = s.Length - specEnd;
      trailing = new byte[trailingLength];
      for (int i = 0; i < trailingLength; i++) trailing[i] = s.ReadByte(specEnd + i);
    }

    return new T042_ManagementControllerHostInterface {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      InterfaceTypeRaw = interfaceType,
      InterfaceTypeSpecificDataLength = specLength,
      InterfaceTypeSpecificData = specData,
      TrailingBytes = trailing,
    };
  }
}
