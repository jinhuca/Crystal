using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.SoftwareFeatures.Directory;

internal static class WmiDirectory {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.Directory;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string Name = CommonWmiProperties.Name;
  public const string CreationClassName = CommonWmiProperties.CreationClassName;
  public const string InstallDate = CommonWmiProperties.InstallDate;

  // ---------------------------------------------------------------------
  // Directory Specific Properties
  // ---------------------------------------------------------------------
  public const string AccessMask = nameof(AccessMask);
  public const string Archive = nameof(Archive);
  public const string Compressed = nameof(Compressed);
  public const string CompressionMethod = nameof(CompressionMethod);
  public const string CreationDate = nameof(CreationDate);
  public const string CSCreationClassName = nameof(CSCreationClassName);
  public const string CSName = nameof(CSName);
  public const string Drive = nameof(Drive);
  public const string EightDotThreeFileName = nameof(EightDotThreeFileName);
  public const string Encrypted = nameof(Encrypted);
  public const string EncryptionMethod = nameof(EncryptionMethod);
  public const string Extension = nameof(Extension);
  public const string FileName = nameof(FileName);
  public const string FileSize = nameof(FileSize);
  public const string FileType = nameof(FileType);
  public const string FSCreationClassName = nameof(FSCreationClassName);
  public const string FSName = nameof(FSName);
  public const string Hidden = nameof(Hidden);
  public const string InUseCount = nameof(InUseCount);
  public const string LastAccessed = nameof(LastAccessed);
  public const string LastModified = nameof(LastModified);
  public const string Path = nameof(Path);
  public const string Readable = nameof(Readable);
  public const string System = nameof(System);
  public const string Writeable = nameof(Writeable);
}
