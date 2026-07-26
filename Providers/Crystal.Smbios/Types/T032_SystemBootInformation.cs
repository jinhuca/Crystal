namespace Crystal.Smbios.Types;

/// <summary>
/// System Boot Status Enumeration (DSP0134 §7.33.1).
/// Values 0x09-0x7F are reserved, 0x80-0xBF are BIOS vendor-specific, and
/// 0xC0-0xFF are OEM/product-specific — those ranges surface only via
/// <see cref="T032_SystemBootInformation.BootStatusRaw"/>.
/// </summary>
public enum SystemBootStatus : byte {
  NoError = 0x00,
  NoBootableMedia = 0x01,
  NormalOSFailedLoading = 0x02,
  FirmwareDetectedFailure = 0x03,
  OSDetectedFailure = 0x04,
  UserRequestedBoot = 0x05,
  SystemSecurityViolation = 0x06,
  PreviousRequestedImage = 0x07,
  WatchdogTimerExpired = 0x08,
}

/// <summary>
/// Type 32 — System Boot Information (DSP0134 §7.33).
/// Communicates the reason the current boot occurred, e.g. to a PXE image
/// or an OS-present management application.
/// </summary>
public sealed class T032_SystemBootInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public byte BootStatusRaw { get; init; }

  /// <summary>Decoded status when it falls within the standard enumeration (&lt;= 0x08); null for the reserved/OEM/product-specific ranges — use <see cref="BootStatusRaw"/> in that case.</summary>
  public SystemBootStatus? Status => BootStatusRaw <= 0x08 ? (SystemBootStatus)BootStatusRaw : null;

  internal static T032_SystemBootInformation Decode(SmbiosRawStructure s) {
    return new T032_SystemBootInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      BootStatusRaw = s.Length > 0x0A ? s.ReadByte(0x0A) : (byte)0,
    };
  }
}
