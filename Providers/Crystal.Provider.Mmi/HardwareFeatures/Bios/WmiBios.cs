using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.Bios;

internal static class WmiBios {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.Bios;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Name = CommonWmiProperties.Name;
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // BIOS Specific Properties
  // ---------------------------------------------------------------------
  public const string BiosCharacteristics = nameof(BiosCharacteristics);
  public const string BIOSVersion = nameof(BIOSVersion);
  public const string BuildNumber = nameof(BuildNumber);
  public const string CodeSet = nameof(CodeSet);
  public const string CurrentLanguage = nameof(CurrentLanguage);
  public const string EmbeddedControllerMajorVersion = nameof(EmbeddedControllerMajorVersion);
  public const string EmbeddedControllerMinorVersion = nameof(EmbeddedControllerMinorVersion);
  public const string IdentificationCode = nameof(IdentificationCode);
  public const string InstallableLanguages = nameof(InstallableLanguages);
  public const string InstallationDate = nameof(InstallationDate);
  public const string LanguageEdition = nameof(LanguageEdition);
  public const string ListOfLanguages = nameof(ListOfLanguages);
  public const string OtherTargetOS = nameof(OtherTargetOS);
  public const string PrimaryBIOS = nameof(PrimaryBIOS);
  public const string ReleaseDate = nameof(ReleaseDate);
  public const string SerialNumber = nameof(SerialNumber);
  public const string SMBIOSBIOSVersion = nameof(SMBIOSBIOSVersion);
  public const string SMBIOSMajorVersion = nameof(SMBIOSMajorVersion);
  public const string SMBIOSMinorVersion = nameof(SMBIOSMinorVersion);
  public const string SMBIOSPresent = nameof(SMBIOSPresent);
  public const string SoftwareElementID = nameof(SoftwareElementID);
  public const string SoftwareElementState = nameof(SoftwareElementState);
  public const string TargetOperatingSystem = nameof(TargetOperatingSystem);
  public const string Version = nameof(Version);
}