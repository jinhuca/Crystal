namespace Crystal.Smbios.Types;

/// <summary>
/// Type 18 — Memory Error Information (32-bit) (DSP0134 §7.18)
/// </summary>
public sealed class T018_MemoryErrorInformation32 : IMemoryErrorInformation, ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public byte ErrorType { get; init; }
  public byte ErrorGranularity { get; init; }
  public uint VendorSyndrome { get; init; }
  public ushort MemoryArrayHandle { get; init; }
  public ushort DeviceHandle { get; init; }
  public uint PhysicalAddress { get; init; }
  public uint AddressResolution { get; init; }

  // IMemoryErrorInformation
  byte IMemoryErrorInformation.ErrorType => ErrorType;
  byte IMemoryErrorInformation.ErrorGranularity => ErrorGranularity;
  ulong IMemoryErrorInformation.VendorSyndrome => VendorSyndrome;
  ushort IMemoryErrorInformation.MemoryArrayHandle => MemoryArrayHandle;
  ushort IMemoryErrorInformation.DeviceHandle => DeviceHandle;
  ulong IMemoryErrorInformation.PhysicalAddress => PhysicalAddress;
  ulong IMemoryErrorInformation.AddressResolution => AddressResolution;
  bool IMemoryErrorInformation.Is64Bit => false;

  internal static T018_MemoryErrorInformation32 Decode(SmbiosRawStructure s) {
    return new T018_MemoryErrorInformation32 {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      ErrorType = s.Length > 0x04 ? s.ReadByte(0x04) : (byte)0,
      ErrorGranularity = s.Length > 0x05 ? s.ReadByte(0x05) : (byte)0,
      VendorSyndrome = s.Length > 0x09 ? s.ReadDWord(0x06) : 0u,
      MemoryArrayHandle = s.Length > 0x0B ? s.ReadWord(0x0A) : (ushort)0,
      DeviceHandle = s.Length > 0x0D ? s.ReadWord(0x0C) : (ushort)0,
      PhysicalAddress = s.Length > 0x11 ? s.ReadDWord(0x0E) : 0u,
      AddressResolution = s.Length > 0x15 ? s.ReadDWord(0x12) : 0u,
    };
  }
}
