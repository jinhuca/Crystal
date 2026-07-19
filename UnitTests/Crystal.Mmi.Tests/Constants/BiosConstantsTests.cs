using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class BiosConstantsTests {
  [Fact]
  public void QueryString_SelectsAllFromWin32Bios() {
    Assert.Equal("SELECT * FROM Win32_BIOS", BiosConstants.QueryString);
  }

  [Theory]
  [InlineData(BiosConstants.BiosCharacteristicsKey, "BiosCharacteristics")]
  [InlineData(BiosConstants.BIOSVersionKey, "BIOSVersion")]
  [InlineData(BiosConstants.BuildNumberKey, "BuildNumber")]
  [InlineData(BiosConstants.CaptionKey, "Caption")]
  [InlineData(BiosConstants.CodeSetKey, "CodeSet")]
  [InlineData(BiosConstants.CurrentLanguageKey, "CurrentLanguage")]
  [InlineData(BiosConstants.DescriptionKey, "Description")]
  [InlineData(BiosConstants.IdentificationCodeKey, "IdentificationCode")]
  [InlineData(BiosConstants.InstallableLanguagesKey, "InstallableLanguages")]
  [InlineData(BiosConstants.InstallDateKey, "InstallDate")]
  [InlineData(BiosConstants.LanguageEditionKey, "LanguageEdition")]
  [InlineData(BiosConstants.ListOfLanguagesKey, "ListOfLanguages")]
  [InlineData(BiosConstants.ManufacturerKey, "Manufacturer")]
  [InlineData(BiosConstants.NameKey, "Name")]
  [InlineData(BiosConstants.OtherTargetOSKey, "OtherTargetOS")]
  [InlineData(BiosConstants.PrimaryBIOSKey, "PrimaryBIOS")]
  [InlineData(BiosConstants.ReleaseDateKey, "ReleaseDate")]
  [InlineData(BiosConstants.SerialNumberKey, "SerialNumber")]
  [InlineData(BiosConstants.SMBIOSBIOSVersionKey, "SMBIOSBIOSVersion")]
  [InlineData(BiosConstants.SMBIOSMajorVersionKey, "SMBIOSMajorVersion")]
  [InlineData(BiosConstants.SMBIOSMinorVersionKey, "SMBIOSMinorVersion")]
  [InlineData(BiosConstants.SMBIOSPresentKey, "SMBIOSPresent")]
  [InlineData(BiosConstants.SoftwareElementIDKey, "SoftwareElementID")]
  [InlineData(BiosConstants.SoftwareElementStateKey, "SoftwareElementState")]
  [InlineData(BiosConstants.StatusKey, "Status")]
  [InlineData(BiosConstants.SystemBiosMajorVersionKey, "SystemBiosMajorVersion")]
  [InlineData(BiosConstants.SystemBiosMinorVersionKey, "SystemBiosMinorVersion")]
  [InlineData(BiosConstants.TargetOperatingSystemKey, "TargetOperatingSystem")]
  [InlineData(BiosConstants.VersionKey, "Version")]
  public void PropertyKey_MatchesWin32BiosSchema(string key, string expected) {
    Assert.Equal(expected, key);
  }
}
