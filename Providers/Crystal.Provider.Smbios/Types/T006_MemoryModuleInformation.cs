using System;

namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Memory Module Installed/Enabled Size special sentinel values (DSP0134 §7.7.4).
/// Bits 6:0 otherwise encode 2^n MB.
/// </summary>
public static class MemoryModuleSizeSentinel {
  public const byte NotDeterminable = 0x7D;
  public const byte ModuleInstalledNoMemoryEnabled = 0x7E;
  public const byte NotInstalled = 0x7F;
}

/// <summary>
/// Memory Module Error Status bit-field (DSP0134 §7.7.5)
/// </summary>
[Flags]
public enum MemoryModuleErrorStatus : byte {
  UncorrectableErrorsReceived = 0x01,
  CorrectableErrorsReceived = 0x02,
  ErrorStatusUnknown = 0x04,
}

/// <summary>
/// Type 6 — Memory Module Information (Obsolete) (DSP0134 §7.7).
/// One instance per memory-module socket; superseded by Memory Device
/// (Type 17) since SMBIOS 2.1.
/// </summary>
public sealed class T006_MemoryModuleInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public string? SocketDesignation { get; init; }

  /// <summary>Bits 0-3: bank connector 0, bits 4-7: bank connector 1 (0xF = unused).</summary>
  public byte BankConnections { get; init; }

  /// <summary>Speed in ns; 0 means unknown.</summary>
  public byte CurrentSpeedNs { get; init; }

  public MemoryModuleTypeFlags CurrentMemoryType { get; init; }

  public byte InstalledSizeRaw { get; init; }
  public byte EnabledSizeRaw { get; init; }
  public MemoryModuleErrorStatus ErrorStatus { get; init; }

  /// <summary>Decodes an Installed/Enabled Size byte into MiB, or null for the special sentinel values.</summary>
  public static long? DecodeSizeMiB(byte raw) {
    byte n = (byte)(raw & 0x7F);
    if (n == MemoryModuleSizeSentinel.NotDeterminable ||
        n == MemoryModuleSizeSentinel.ModuleInstalledNoMemoryEnabled ||
        n == MemoryModuleSizeSentinel.NotInstalled ||
        n >= 63)
      return null;
    return 1L << n;
  }

  public long? InstalledSizeMiB => DecodeSizeMiB(InstalledSizeRaw);
  public long? EnabledSizeMiB => DecodeSizeMiB(EnabledSizeRaw);

  /// <summary>Whether the installed module uses double-bank (bit 7 of the raw Installed Size byte) addressing.</summary>
  public bool IsDoubleBank => (InstalledSizeRaw & 0x80) != 0;

  internal static T006_MemoryModuleInformation Decode(SmbiosRawStructure s) {
    return new T006_MemoryModuleInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      SocketDesignation = s.GetString(s.ReadByte(0x04)),
      BankConnections = s.ReadByte(0x05),
      CurrentSpeedNs = s.ReadByte(0x06),
      CurrentMemoryType = (MemoryModuleTypeFlags)s.ReadWord(0x07),
      InstalledSizeRaw = s.ReadByte(0x09),
      EnabledSizeRaw = s.ReadByte(0x0A),
      ErrorStatus = s.Length > 0x0B ? (MemoryModuleErrorStatus)s.ReadByte(0x0B) : 0,
    };
  }
}
