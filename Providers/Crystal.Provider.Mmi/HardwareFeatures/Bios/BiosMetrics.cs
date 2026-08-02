namespace Crystal.Provider.Mmi.HardwareFeatures.Bios;

public record BiosMetrics(
  ushort? BiosCharacteristics, // Maps from ushort[] array types
  string? BIOSVersion,         // Maps from string[] array types
  string? BuildNumber,
  string? Caption,
  string? CodeSet,
  string? CurrentLanguage,
  string? Description,
  string? EmbeddedControllerMajorVersion,
  string? EmbeddedControllerMinorVersion,
  string? IdentificationCode,
  ushort? InstallableLanguages,
  DateTime? InstallDate,
  string? LanguageEdition,
  string? ListOfLanguages,     // Maps from string[] array types
  string? Manufacturer,
  string? Name,
  string? OtherTargetOS,
  string? PartNumber,
  bool? PrimaryBIOS,
  string? ReleaseDate,
  string? SerialNumber,
  string? SMBIOSBIOSVersion,
  bool? SMBIOSPresent,
  ushort? SMBIOSMajorVersion,
  ushort? SMBIOSMinorVersion,
  string? Status,
  string? SystemBiosMajorVersion,
  string? SystemBiosMinorVersion,
  ushort? TargetOperatingSystem,
  string? Version);
