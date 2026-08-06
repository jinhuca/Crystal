namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 18 — 32-Bit Memory Error Information (DSP0134 §7.19)
/// </summary>
public sealed class T018_MemoryErrorInformation32 : IMemoryErrorInformation, ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public byte ErrorType { get; init; }
  public byte ErrorGranularity { get; init; }
  public byte ErrorOperation { get; init; }
  public uint VendorSyndrome { get; init; }
  public uint MemoryArrayErrorAddress { get; init; }
  public uint DeviceErrorAddress { get; init; }
  public uint ErrorResolution { get; init; }

  // IMemoryErrorInformation
  byte IMemoryErrorInformation.ErrorType => ErrorType;
  byte IMemoryErrorInformation.ErrorGranularity => ErrorGranularity;
  byte IMemoryErrorInformation.ErrorOperation => ErrorOperation;
  ulong IMemoryErrorInformation.VendorSyndrome => VendorSyndrome;
  ulong IMemoryErrorInformation.MemoryArrayErrorAddress => MemoryArrayErrorAddress;
  ulong IMemoryErrorInformation.DeviceErrorAddress => DeviceErrorAddress;
  ulong IMemoryErrorInformation.ErrorResolution => ErrorResolution;
  bool IMemoryErrorInformation.Is64Bit => false;

  internal static T018_MemoryErrorInformation32 Decode(SmbiosRawStructure s) {
    // DSP0134 §7.19: Error Type @0x04, Granularity @0x05, Operation @0x06,
    // Vendor Syndrome DWORD @0x07, Memory Array Error Address DWORD @0x0B,
    // Device Error Address DWORD @0x0F, Error Resolution DWORD @0x13.
    return new T018_MemoryErrorInformation32 {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      ErrorType = s.Length > 0x04 ? s.ReadByte(0x04) : (byte)0,
      ErrorGranularity = s.Length > 0x05 ? s.ReadByte(0x05) : (byte)0,
      ErrorOperation = s.Length > 0x06 ? s.ReadByte(0x06) : (byte)0,
      VendorSyndrome = s.Length > 0x0A ? s.ReadDWord(0x07) : 0u,
      MemoryArrayErrorAddress = s.Length > 0x0E ? s.ReadDWord(0x0B) : 0u,
      DeviceErrorAddress = s.Length > 0x12 ? s.ReadDWord(0x0F) : 0u,
      ErrorResolution = s.Length > 0x16 ? s.ReadDWord(0x13) : 0u,
    };
  }
}
