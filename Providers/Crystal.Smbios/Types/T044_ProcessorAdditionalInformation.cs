using System;
using System.Collections.Generic;

namespace Crystal.Smbios.Types;

/// <summary>
/// Processor-Specific Block Architecture Type Enumeration (DSP0134 §7.45.1)
/// </summary>
public enum ProcessorSpecificBlockArchitectureType : byte {
  Reserved = 0x00,
  Ia32 = 0x01,
  X64 = 0x02,
  Itanium = 0x03,
  Aarch32 = 0x04,
  Aarch64 = 0x05,
  RiscVRv32 = 0x06,
  RiscVRv64 = 0x07,
  RiscVRv128 = 0x08,
  LoongArch32 = 0x09,
  LoongArch64 = 0x0A,
}

/// <summary>
/// Type 44 — Processor Additional Information (DSP0134 §7.45).
/// Supplements a Type 4 Processor Information structure when its fixed
/// enumerations aren't sufficient to describe the processor (e.g. RISC-V,
/// LoongArch, or information that differs per core); may appear multiple
/// times per referenced processor. The processor-specific payload itself
/// is architecture-defined (maintained outside DSP0134) and is exposed
/// here as raw bytes rather than being decoded field-by-field.
/// </summary>
public sealed class T044_ProcessorAdditionalInformation : ISmbiosDecodedStructure {
  public SmbiosStructureType StructureType { get; init; }
  public byte Length { get; init; }
  public ushort Handle { get; init; }

  /// <summary>Handle of the associated Type 4 Processor Information structure.</summary>
  public ushort ReferencedHandle { get; init; }

  /// <summary>Length of the Processor-Specific Block, including its own 2-byte header.</summary>
  public byte ProcessorSpecificBlockLength { get; init; }
  public ProcessorSpecificBlockArchitectureType ProcessorArchitectureType { get; init; }

  /// <summary>Raw architecture-specific payload following the block header (may be empty).</summary>
  public IReadOnlyList<byte> ProcessorSpecificData { get; init; } = Array.Empty<byte>();

  internal static T044_ProcessorAdditionalInformation Decode(SmbiosRawStructure s) {
    byte blockLength = s.Length > 0x06 ? s.ReadByte(0x06) : (byte)0;
    var archType = s.Length > 0x07
        ? (ProcessorSpecificBlockArchitectureType)s.ReadByte(0x07)
        : ProcessorSpecificBlockArchitectureType.Reserved;

    var data = Array.Empty<byte>();
    int dataLength = blockLength - 2; // block length includes its own Length+ArchType header
    if (dataLength > 0 && s.Length >= 0x08 + dataLength) {
      data = new byte[dataLength];
      for (int i = 0; i < dataLength; i++) data[i] = s.ReadByte(0x08 + i);
    }

    return new T044_ProcessorAdditionalInformation {
      StructureType = s.Type,
      Length = s.Length,
      Handle = s.Handle,
      ReferencedHandle = s.ReadWord(0x04),
      ProcessorSpecificBlockLength = blockLength,
      ProcessorArchitectureType = archType,
      ProcessorSpecificData = data,
    };
  }
}
