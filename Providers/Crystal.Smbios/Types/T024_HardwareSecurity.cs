namespace Crystal.Smbios.Types;

/// <summary>
/// Hardware Security — Password/Reset Status Enumeration (DSP0134 §7.25).
/// </summary>
public enum HardwareSecurityStatus : byte {
  Disabled = 0x00,
  Enabled = 0x01,
  NotImplemented = 0x02,
  Unknown = 0x03,
}

/// <summary>
/// Type 24 — Hardware Security (DSP0134 §7.25).
/// Describes system-wide hardware security settings.
/// </summary>
public sealed class T024_HardwareSecurity : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public byte HardwareSecuritySettingsRaw { get; init; }

  /// <summary>Bits 7:6.</summary>
  public HardwareSecurityStatus PowerOnPasswordStatus { get; init; }
  /// <summary>Bits 5:4.</summary>
  public HardwareSecurityStatus KeyboardPasswordStatus { get; init; }
  /// <summary>Bits 3:2.</summary>
  public HardwareSecurityStatus AdministratorPasswordStatus { get; init; }
  /// <summary>Bits 1:0.</summary>
  public HardwareSecurityStatus FrontPanelResetStatus { get; init; }

  internal static T024_HardwareSecurity Decode(SmbiosRawStructure s) {
    byte raw = s.ReadByte(0x04);
    return new T024_HardwareSecurity {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      HardwareSecuritySettingsRaw = raw,
      PowerOnPasswordStatus = (HardwareSecurityStatus)(raw >> 6),
      KeyboardPasswordStatus = (HardwareSecurityStatus)((raw >> 4) & 0x03),
      AdministratorPasswordStatus = (HardwareSecurityStatus)((raw >> 2) & 0x03),
      FrontPanelResetStatus = (HardwareSecurityStatus)(raw & 0x03),
    };
  }
}
