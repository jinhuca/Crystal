namespace Crystal.Provider.Smbios.Types;

/// <summary>
/// Type 33 — 64-Bit Memory Error Information (DSP0134 §7.34)
/// </summary>
public sealed class T033_MemoryErrorInformation64 : IMemoryErrorInformation, ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }
  public byte ErrorType { get; init; }
  public byte ErrorGranularity { get; init; }
  public byte ErrorOperation { get; init; }
  public uint VendorSyndrome { get; init; }
  public ulong MemoryArrayErrorAddress { get; init; }
  public ulong DeviceErrorAddress { get; init; }
  public uint ErrorResolution { get; init; }

  internal static T033_MemoryErrorInformation64 Decode(SmbiosRawStructure s) {
    // DSP0134 §7.34: Error Type @0x04, Granularity @0x05, Operation @0x06,
    // Vendor Syndrome DWORD @0x07, Memory Array Error Address QWORD @0x0B,
    // Device Error Address QWORD @0x13, Error Resolution DWORD @0x1B.
    return new T033_MemoryErrorInformation64 {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      ErrorType = s.Length > 0x04 ? s.ReadByte(0x04) : (byte)0,
      ErrorGranularity = s.Length > 0x05 ? s.ReadByte(0x05) : (byte)0,
      ErrorOperation = s.Length > 0x06 ? s.ReadByte(0x06) : (byte)0,
      VendorSyndrome = s.Length > 0x0A ? s.ReadDWord(0x07) : 0u,
      MemoryArrayErrorAddress = s.Length > 0x12 ? s.ReadQWord(0x0B) : 0UL,
      DeviceErrorAddress = s.Length > 0x1A ? s.ReadQWord(0x13) : 0UL,
      ErrorResolution = s.Length > 0x1E ? s.ReadDWord(0x1B) : 0u,
    };
  }

  // IMemoryErrorInformation implementation
  byte IMemoryErrorInformation.ErrorType => ErrorType;
  byte IMemoryErrorInformation.ErrorGranularity => ErrorGranularity;
  byte IMemoryErrorInformation.ErrorOperation => ErrorOperation;
  ulong IMemoryErrorInformation.VendorSyndrome => VendorSyndrome;
  ulong IMemoryErrorInformation.MemoryArrayErrorAddress => MemoryArrayErrorAddress;
  ulong IMemoryErrorInformation.DeviceErrorAddress => DeviceErrorAddress;
  ulong IMemoryErrorInformation.ErrorResolution => ErrorResolution;
  bool IMemoryErrorInformation.Is64Bit => true;
}
