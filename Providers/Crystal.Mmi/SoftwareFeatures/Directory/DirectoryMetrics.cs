namespace Crystal.Mmi.SoftwareFeatures.Directory;

// Win32_Directory represents a single folder entry (e.g. "C:\Temp"). Querying every folder on
// a system is expensive and typically scoped with a WHERE clause (e.g. WHERE Drive='C:') rather
// than enumerated in bulk — see the high-volume handling in the CLI.
public record DirectoryMetrics(
  uint? AccessMask,
  bool? Archive,
  string? Caption,
  bool? Compressed,
  string? CompressionMethod,
  string? CreationClassName,
  DateTime? CreationDate,
  string? CSCreationClassName,
  string? CSName,
  string? Description,
  string? Drive,                 // e.g. "c:"
  string? EightDotThreeFileName, // e.g. "c:\\progra~1"
  bool? Encrypted,
  string? EncryptionMethod,
  string? Extension,
  string? FileName,
  ulong? FileSize,                // always 0 for folders per WMI docs
  string? FileType,
  string? FSCreationClassName,
  string? FSName,
  bool? Hidden,
  DateTime? InstallDate,
  ulong? InUseCount,
  DateTime? LastAccessed,
  DateTime? LastModified,
  string? Name,                   // full path, e.g. "C:\\Windows\\system32\\win.ini"
  string? Path,                   // e.g. "\\windows\\system32\\" (no drive letter or folder name)
  bool? Readable,
  string? Status,
  bool? System,
  bool? Writeable
);
