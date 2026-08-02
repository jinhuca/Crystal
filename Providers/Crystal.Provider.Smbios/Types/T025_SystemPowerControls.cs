namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 25 — System Power Controls (DSP0134 §7.26).
/// Describes the next scheduled power-on time for systems with a timed
/// power-on facility. Fields are packed BCD; a value of 0x00 in Day means
/// "every day", and similarly for Hour/Minute/Second meaning "every N".
/// </summary>
public sealed class T025_SystemPowerControls : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public byte NextScheduledPowerOnMonthBcd { get; init; }
  public byte NextScheduledPowerOnDayOfMonthBcd { get; init; }
  public byte NextScheduledPowerOnHourBcd { get; init; }
  public byte NextScheduledPowerOnMinuteBcd { get; init; }
  public byte NextScheduledPowerOnSecondBcd { get; init; }

  /// <summary>Decodes a packed-BCD byte into its decimal value, or null when it isn't valid BCD.</summary>
  public static int? DecodeBcd(byte value) {
    int hi = value >> 4, lo = value & 0x0F;
    if (hi > 9 || lo > 9) return null;
    return hi * 10 + lo;
  }

  public int? Month => DecodeBcd(NextScheduledPowerOnMonthBcd);
  public int? DayOfMonth => DecodeBcd(NextScheduledPowerOnDayOfMonthBcd);
  public int? Hour => DecodeBcd(NextScheduledPowerOnHourBcd);
  public int? Minute => DecodeBcd(NextScheduledPowerOnMinuteBcd);
  public int? Second => DecodeBcd(NextScheduledPowerOnSecondBcd);

  internal static T025_SystemPowerControls Decode(SmbiosRawStructure s) {
    return new T025_SystemPowerControls {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      NextScheduledPowerOnMonthBcd = s.ReadByte(0x04),
      NextScheduledPowerOnDayOfMonthBcd = s.ReadByte(0x05),
      NextScheduledPowerOnHourBcd = s.ReadByte(0x06),
      NextScheduledPowerOnMinuteBcd = s.ReadByte(0x07),
      NextScheduledPowerOnSecondBcd = s.ReadByte(0x08),
    };
  }
}
