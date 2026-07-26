namespace Crystal.Smbios.Types;

/// <summary>
/// Type 35 — Management Device Component (DSP0134 §7.36).
/// Associates a cooling device or environmental probe (Type 26/27/28/29)
/// with the Management Device (Type 34) that controls it, and optionally
/// with a Management Device Threshold Data (Type 36) structure.
/// </summary>
public sealed class T035_ManagementDeviceComponent : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  public string? Description { get; init; }

  /// <summary>Handle of the owning Management Device (Type 34) structure.</summary>
  public ushort ManagementDeviceHandle { get; init; }
  /// <summary>Handle of the governed probe/cooling-device structure (Type 26/27/28/29).</summary>
  public ushort ComponentHandle { get; init; }
  /// <summary>Handle of the associated Management Device Threshold Data (Type 36) structure, or 0xFFFF if none.</summary>
  public ushort ThresholdHandle { get; init; }

  public bool HasThreshold => ThresholdHandle != 0xFFFF;

  internal static T035_ManagementDeviceComponent Decode(SmbiosRawStructure s) {
    return new T035_ManagementDeviceComponent {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      Description = s.GetString(s.ReadByte(0x04)),
      ManagementDeviceHandle = s.ReadWord(0x05),
      ComponentHandle = s.ReadWord(0x07),
      ThresholdHandle = s.ReadWord(0x09),
    };
  }
}
