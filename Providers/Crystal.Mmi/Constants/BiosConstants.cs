using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Mmi.Constants; 
internal static class BiosConstants {
  public const string QueryString = "SELECT * FROM Win32_BIOS";

  public const string BiosCharacteristicsKey = "BiosCharacteristics";
  public const string BiosCharacteristicsDesc = "Array of BIOS characteristics supported by the system, as defined by the System Management BIOS Reference Specification";

  public const string BIOSVersionKey = "BIOSVersion";
  public const string BIOSVersionDesc = "Array of names for the BIOS";

  public const string BuildNumberKey = "BuildNumber";
  public const string BuildNumberDesc = "Build number of the BIOS";

  public const string CaptionKey = "Caption";
  public const string CaptionDesc = "Short description of the BIOS";

  public const string CodeSetKey = "CodeSet";
  public const string CodeSetDesc = "Code page value the BIOS uses";

  public const string CurrentLanguageKey = "CurrentLanguage";
  public const string CurrentLanguageDesc = "Current language identifier used by the BIOS";

  public const string DescriptionKey = "Description";
  public const string DescriptionDesc = "Description of the BIOS";

  public const string IdentificationCodeKey = "IdentificationCode";
  public const string IdentificationCodeDesc = "Identifier for this BIOS";

  public const string InstallableLanguagesKey = "InstallableLanguages";
  public const string InstallableLanguagesDesc = "Number of languages available for installation on this BIOS";

  public const string InstallDateKey = "InstallDate";
  public const string InstallDateDesc = "Date and time the BIOS was installed";

  public const string LanguageEditionKey = "LanguageEdition";
  public const string LanguageEditionDesc = "Language edition of the BIOS";

  public const string ListOfLanguagesKey = "ListOfLanguages";
  public const string ListOfLanguagesDesc = "List of languages available for installation on this BIOS";

  public const string ManufacturerKey = "Manufacturer";
  public const string ManufacturerDesc = "Name of the BIOS manufacturer";

  public const string NameKey = "Name";
  public const string NameDesc = "Name of the BIOS";

  public const string OtherTargetOSKey = "OtherTargetOS";
  public const string OtherTargetOSDesc = "Additional target operating system description, used when TargetOperatingSystem is set to 'Other'";

  public const string PrimaryBIOSKey = "PrimaryBIOS";
  public const string PrimaryBIOSDesc = "If True, this is the primary BIOS of the computer system";

  public const string ReleaseDateKey = "ReleaseDate";
  public const string ReleaseDateDesc = "Release date of the Windows BIOS";

  public const string SerialNumberKey = "SerialNumber";
  public const string SerialNumberDesc = "Assigned serial number of the BIOS";

  public const string SMBIOSBIOSVersionKey = "SMBIOSBIOSVersion";
  public const string SMBIOSBIOSVersionDesc = "BIOS version as reported by SMBIOS";

  public const string SMBIOSMajorVersionKey = "SMBIOSMajorVersion";
  public const string SMBIOSMajorVersionDesc = "Major SMBIOS version number";

  public const string SMBIOSMinorVersionKey = "SMBIOSMinorVersion";
  public const string SMBIOSMinorVersionDesc = "Minor SMBIOS version number";

  public const string SMBIOSPresentKey = "SMBIOSPresent";
  public const string SMBIOSPresentDesc = "If True, the SMBIOS is available on this computer system";

  public const string SoftwareElementIDKey = "SoftwareElementID";
  public const string SoftwareElementIDDesc = "Identifier for this software element";

  public const string SoftwareElementStateKey = "SoftwareElementState";
  public const string SoftwareElementStateDesc = "State of a software element";

  public const string StatusKey = "Status";
  public const string StatusDesc = "Current status of the BIOS";

  public const string SystemBiosMajorVersionKey = "SystemBiosMajorVersion";
  public const string SystemBiosMajorVersionDesc = "Major version of the system BIOS";

  public const string SystemBiosMinorVersionKey = "SystemBiosMinorVersion";
  public const string SystemBiosMinorVersionDesc = "Minor version of the system BIOS";

  public const string TargetOperatingSystemKey = "TargetOperatingSystem";
  public const string TargetOperatingSystemDesc = "Target operating system for the BIOS";

  public const string VersionKey = "Version";
  public const string VersionDesc = "BIOS version as reported by the manufacturer";
}
