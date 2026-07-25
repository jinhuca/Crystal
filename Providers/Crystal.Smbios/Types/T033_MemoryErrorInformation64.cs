namespace Crystal.Smbios.Types;

/// <summary>
/// Type 33 — Memory Error Information (64-bit / extended) (DSP0134 §7.33)
/// </summary>
public sealed class T033_MemoryErrorInformation64 : IMemoryErrorInformation, ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public byte ErrorType { get; init; }
  public byte ErrorGranularity { get; init; }
  public ushort ErrorOperation { get; init; }
  public ulong PhysicalAddress { get; init; }
  public ulong PhysicalAddressMask { get; init; }
  public ulong VendorSyndrome { get; init; }
  public ushort MemoryArrayHandle { get; init; }
  public ushort DeviceHandle { get; init; }

  internal static T033_MemoryErrorInformation64 Decode(SmbiosRawStructure s) {
    return new T033_MemoryErrorInformation64 {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      ErrorType = s.Length > 0x04 ? s.ReadByte(0x04) : (byte)0,
      ErrorGranularity = s.Length > 0x05 ? s.ReadByte(0x05) : (byte)0,
      ErrorOperation = s.Length > 0x07 ? s.ReadWord(0x06) : (ushort)0,
      VendorSyndrome = s.Length > 0x10 ? s.ReadQWord(0x08) : 0UL,
      PhysicalAddress = s.Length > 0x17 ? s.ReadQWord(0x10) : 0UL,
      PhysicalAddressMask = s.Length > 0x1F ? s.ReadQWord(0x18) : 0UL,
      MemoryArrayHandle = s.Length > 0x21 ? s.ReadWord(0x20) : (ushort)0,
      DeviceHandle = s.Length > 0x23 ? s.ReadWord(0x22) : (ushort)0,
    };
  }

  // IMemoryErrorInformation implementation
  byte IMemoryErrorInformation.ErrorType => ErrorType;
  byte IMemoryErrorInformation.ErrorGranularity => ErrorGranularity;
  ulong IMemoryErrorInformation.VendorSyndrome => VendorSyndrome;
  ushort IMemoryErrorInformation.MemoryArrayHandle => MemoryArrayHandle;
  ushort IMemoryErrorInformation.DeviceHandle => DeviceHandle;
  ulong IMemoryErrorInformation.PhysicalAddress => PhysicalAddress;
  ulong IMemoryErrorInformation.AddressResolution => PhysicalAddressMask;
  bool IMemoryErrorInformation.Is64Bit => true;
}
