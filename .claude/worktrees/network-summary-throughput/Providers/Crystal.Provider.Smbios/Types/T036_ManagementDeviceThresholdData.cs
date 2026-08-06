namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 36 — Management Device Threshold Data (DSP0134 §7.37).
/// Threshold values for a component governed by a Management Device.
/// Each field is 0x8000 when the corresponding threshold is unknown.
/// </summary>
public sealed class T036_ManagementDeviceThresholdData : ISmbiosDecodedStructure {
  private const ushort Unknown = 0x8000;

  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public ushort LowerThresholdNonCritical { get; init; }
  public ushort UpperThresholdNonCritical { get; init; }
  public ushort LowerThresholdCritical { get; init; }
  public ushort UpperThresholdCritical { get; init; }
  public ushort LowerThresholdNonRecoverable { get; init; }
  public ushort UpperThresholdNonRecoverable { get; init; }

  public bool HasNonCriticalThresholds => LowerThresholdNonCritical != Unknown || UpperThresholdNonCritical != Unknown;
  public bool HasCriticalThresholds => LowerThresholdCritical != Unknown || UpperThresholdCritical != Unknown;
  public bool HasNonRecoverableThresholds => LowerThresholdNonRecoverable != Unknown || UpperThresholdNonRecoverable != Unknown;

  internal static T036_ManagementDeviceThresholdData Decode(SmbiosRawStructure s) {
    return new T036_ManagementDeviceThresholdData {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      LowerThresholdNonCritical = s.Length > 0x05 ? s.ReadWord(0x04) : Unknown,
      UpperThresholdNonCritical = s.Length > 0x07 ? s.ReadWord(0x06) : Unknown,
      LowerThresholdCritical = s.Length > 0x09 ? s.ReadWord(0x08) : Unknown,
      UpperThresholdCritical = s.Length > 0x0B ? s.ReadWord(0x0A) : Unknown,
      LowerThresholdNonRecoverable = s.Length > 0x0D ? s.ReadWord(0x0C) : Unknown,
      UpperThresholdNonRecoverable = s.Length > 0x0F ? s.ReadWord(0x0E) : Unknown,
    };
  }
}
