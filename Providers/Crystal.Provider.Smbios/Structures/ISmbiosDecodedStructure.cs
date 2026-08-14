namespace Crystal.Provider.Smbios.Structures;

/// <summary>
/// Common metadata shared by every decoded SMBIOS structure (T000, T001, T004, …):
/// the raw structure type it was decoded from, the formatted-area length, and the
/// firmware-assigned handle. Lets callers work with heterogeneous decoded structures
/// generically (e.g. logging/dumping "every structure's Handle" across types) without
/// needing to know the concrete T0xx type up front.
///
/// Named <c>StructureType</c> rather than <c>Type</c> because several decoded types
/// (e.g. <c>T017_MemoryDevice.Type</c>) already expose a domain-specific "Type" enum
/// of their own (<c>MemoryType</c>, <c>ProcessorType</c>, etc.) — this avoids colliding
/// with those.
/// </summary>
public interface ISmbiosDecodedStructure {
  /// <summary>The raw SMBIOS structure type (DSP0134 §7) this instance was decoded from.</summary>
  SmbiosStructureType StructureType { get; }

  /// <summary>Length of the formatted area this instance was decoded from (excludes the string table).</summary>
  byte Length { get; }

  /// <summary>Firmware-assigned handle, unique within the table.</summary>
  ushort Handle { get; }
}
