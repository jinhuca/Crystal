namespace Crystal.Provider.Mmi.HardwareFeatures.Bios;

/// <summary>
/// Represents the BIOS metrics of a system, including various characteristics, version information, and other relevant details.
/// </summary>
/// <param name="BiosCharacteristics">The BIOS characteristics.</param>
/// <param name="BIOSVersion">The BIOS version.</param>
/// <param name="BuildNumber">The build number.</param>
/// <param name="Caption">The caption.</param>
/// <param name="CodeSet">The code set.</param>
/// <param name="CurrentLanguage">The current language.</param>
/// <param name="Description">The description.</param>
/// <param name="EmbeddedControllerMajorVersion">The major version of the embedded controller.</param>
/// <param name="EmbeddedControllerMinorVersion">The minor version of the embedded controller.</param>
/// <param name="IdentificationCode">The identification code.</param>
/// <param name="InstallableLanguages">The installable languages.</param>
/// <param name="InstallDate">The installation date.</param>
/// <param name="LanguageEdition">The language edition.</param>
/// <param name="ListOfLanguages">The list of languages.</param>
/// <param name="Manufacturer">The manufacturer.</param>
/// <param name="Name">The name.</param>
/// <param name="OtherTargetOS">The other target operating system.</param>
/// <param name="PartNumber">The part number.</param>
/// <param name="PrimaryBIOS">Indicates whether the BIOS is primary.</param>
/// <param name="ReleaseDate">The release date.</param>
/// <param name="SerialNumber">The serial number.</param>
/// <param name="SMBIOSBIOSVersion">The SMBIOS BIOS version.</param>
/// <param name="SMBIOSPresent">Indicates whether SMBIOS is present.</param>
/// <param name="SMBIOSMajorVersion">The major version of SMBIOS.</param>
/// <param name="SMBIOSMinorVersion">The minor version of SMBIOS.</param>
/// <param name="Status">The status.</param>
/// <param name="SystemBiosMajorVersion">The major version of the system BIOS.</param>
/// <param name="SystemBiosMinorVersion">The minor version of the system BIOS.</param>
/// <param name="TargetOperatingSystem">The target operating system.</param>
/// <param name="Version">The version.</param>
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
