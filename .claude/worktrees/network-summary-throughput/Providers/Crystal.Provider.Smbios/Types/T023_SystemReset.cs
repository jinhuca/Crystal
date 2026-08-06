namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// System Reset — Boot Option Enumeration (DSP0134 §7.24, bits 2:1 / 4:3 of Capabilities).
/// </summary>
public enum SystemResetBootOption : byte {
  Reserved = 0x00,
  OperatingSystem = 0x01,
  SystemUtilities = 0x02,
  DoNotReboot = 0x03,
}

/// <summary>
/// Type 23 — System Reset (DSP0134 §7.24).
/// Describes the system's automatic-reset (watchdog) capabilities.
/// </summary>
public sealed class T023_SystemReset : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public byte CapabilitiesRaw { get; init; }

  /// <summary>Whether automatic system-reset functionality is enabled (bit 0).</summary>
  public bool IsEnabled { get; init; }
  /// <summary>Whether the system has a watchdog timer (bit 5).</summary>
  public bool HasWatchdogTimer { get; init; }
  /// <summary>Action taken when the reset limit has not yet been reached (bits 2:1).</summary>
  public SystemResetBootOption BootOption { get; init; }
  /// <summary>Action taken once <see cref="ResetLimit"/> has been reached (bits 4:3).</summary>
  public SystemResetBootOption BootOptionOnLimit { get; init; }

  /// <summary>Number of automatic resets since the last manual reset; 0xFFFF = unknown.</summary>
  public ushort ResetCount { get; init; }
  /// <summary>Number of consecutive automatic resets allowed before <see cref="BootOptionOnLimit"/> applies; 0xFFFF = unknown.</summary>
  public ushort ResetLimit { get; init; }
  /// <summary>Watchdog interval in minutes; 0xFFFF = unknown.</summary>
  public ushort TimerIntervalMinutes { get; init; }
  /// <summary>Number of minutes before the watchdog times out; 0xFFFF = unknown.</summary>
  public ushort TimeoutMinutes { get; init; }

  internal static T023_SystemReset Decode(SmbiosRawStructure s) {
    byte capabilities = s.ReadByte(0x04);
    return new T023_SystemReset {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      CapabilitiesRaw = capabilities,
      IsEnabled = (capabilities & 0x01) != 0,
      HasWatchdogTimer = (capabilities & 0x20) != 0,
      BootOption = (SystemResetBootOption)((capabilities >> 1) & 0x03),
      BootOptionOnLimit = (SystemResetBootOption)((capabilities >> 3) & 0x03),
      ResetCount = s.ReadWord(0x05),
      ResetLimit = s.ReadWord(0x07),
      TimerIntervalMinutes = s.ReadWord(0x09),
      TimeoutMinutes = s.ReadWord(0x0B),
    };
  }
}
