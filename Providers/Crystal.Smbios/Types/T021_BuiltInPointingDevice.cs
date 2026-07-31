namespace Crystal.Smbios.Types;

using System;

/// <summary>
/// Type 21 ─ Built-in Pointing Device (DSP0134 §7.11)
/// Standalone structure describing an integrated pointing device (touchpad, trackpoint, etc.).
/// </summary>
public sealed class T021_BuiltInPointingDevice : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>Pointing device type (mouse, trackball, touchpad, etc.).</summary>
  public PointingDeviceType DeviceType { get; init; }

  /// <summary>Interface used by the pointing device (PS/2, USB, I2C, etc.).</summary>
  public PointingDeviceInterface Interface { get; init; }

  /// <summary>Number of physical buttons (0 = unknown/not reported).</summary>
  public byte NumberOfButtons { get; init; }

  internal static T021_BuiltInPointingDevice Decode(SmbiosRawStructure s) {
    // DSP0134 §7.22: Type @0x04, Interface @0x05, Number of Buttons @0x06.
    return new T021_BuiltInPointingDevice {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      DeviceType = s.Length > 0x04 ? (PointingDeviceType)s.ReadByte(0x04) : PointingDeviceType.Other,
      Interface = s.Length > 0x05 ? (PointingDeviceInterface)s.ReadByte(0x05) : PointingDeviceInterface.Unknown,
      NumberOfButtons = s.Length > 0x06 ? s.ReadByte(0x06) : (byte)0,
    };
  }
}

/// <summary>DSP0134 §7.23.1 ─ Device Chemistry.</summary>
public enum BatteryChemistry : byte {
  Other = 0x01,
  Unknown = 0x02,
  LeadAcid = 0x03,
  NickelCadmium = 0x04,
  NickelMetalHydride = 0x05,
  LithiumIon = 0x06,
  ZincAir = 0x07,
  LithiumPolymer = 0x08,
}

// ── Type 21 ─ Built-in Pointing Device enums (DSP0134 §7.11) ──────────────────────────

/// <summary>
/// DSP0134 §7.11.1 ─ Pointing Device Type.
/// </summary>
public enum PointingDeviceType : byte {
  Other = 0x01,
  Unknown = 0x02,
  Mouse = 0x03,
  TrackBall = 0x04,
  TrackPoint = 0x05,
  GlidePoint = 0x06,
  TouchPad = 0x07,
  TouchScreen = 0x08,
  OpticalSensor = 0x09,
}

/// <summary>
/// DSP0134 §7.11.2 ─ Pointing Device Interface.
/// </summary>
public enum PointingDeviceInterface : byte {
  Other = 0x01,
  Unknown = 0x02,
  Serial = 0x03,
  Ps2 = 0x04,
  Infrared = 0x05,
  HpHil = 0x06,
  BusMouse = 0x07,
  Adb = 0x08,
  BusMouseDb9 = 0xA0,
  BusMouseMicroDin = 0xA1,
  Usb = 0xA2,
  I2c = 0xA3,
  Spi = 0xA4,
}