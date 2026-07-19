using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class OsConstantsTests {
  [Fact]
  public void QueryString_SelectsAllFromWin32OperatingSystem() {
    Assert.Equal("SELECT * FROM Win32_OperatingSystem", OsConstants.QueryString);
  }

  // Each key constant must match the real Win32_OperatingSystem WMI/MI property
  // name exactly -- these are used to look up values in the property bag returned
  // by CimInstanceProperties, so a typo here silently returns nothing at runtime
  // instead of failing to compile.
  [Theory]
  [InlineData(OsConstants.BootDeviceKey, "BootDevice")]
  [InlineData(OsConstants.BuildNumberKey, "BuildNumber")]
  [InlineData(OsConstants.BuildTypeKey, "BuildType")]
  [InlineData(OsConstants.CaptionKey, "Caption")]
  [InlineData(OsConstants.CodeSetKey, "CodeSet")]
  [InlineData(OsConstants.CountryCodeKey, "CountryCode")]
  [InlineData(OsConstants.CreationClassNameKey, "CreationClassName")]
  [InlineData(OsConstants.CSCreationClassNameKey, "CSCreationClassName")]
  [InlineData(OsConstants.CSDVersionKey, "CSDVersion")]
  [InlineData(OsConstants.CSNameKey, "CSName")]
  [InlineData(OsConstants.VersionKey, "Version")]
  [InlineData(OsConstants.CurrentTimeZoneKey, "CurrentTimeZone")]
  [InlineData(OsConstants.DataExecutionPrevention_32BitApplicationsKey, "DataExecutionPrevention_32BitApplications")]
  [InlineData(OsConstants.DataExecutionPrevention_AvailableKey, "DataExecutionPrevention_Available")]
  [InlineData(OsConstants.DataExecutionPrevention_DriversKey, "DataExecutionPrevention_Drivers")]
  [InlineData(OsConstants.LastBootUpTimeKey, "LastBootUpTime")]
  [InlineData(OsConstants.LocalDateTimeKey, "LocalDateTime")]
  [InlineData(OsConstants.LocaleKey, "Locale")]
  [InlineData(OsConstants.ManufacturerKey, "Manufacturer")]
  [InlineData(OsConstants.MaxNumberOfProcessesKey, "MaxNumberOfProcesses")]
  [InlineData(OsConstants.MaxProcessMemorySizeKey, "MaxProcessMemorySize")]
  [InlineData(OsConstants.MUILanguagesKey, "MUILanguages")]
  [InlineData(OsConstants.NameKey, "Name")]
  [InlineData(OsConstants.NumberOfLicensedUsersKey, "NumberOfLicensedUsers")]
  [InlineData(OsConstants.NumberOfProcessesKey, "NumberOfProcesses")]
  [InlineData(OsConstants.NumberOfUsersKey, "NumberOfUsers")]
  [InlineData(OsConstants.OperatingSystemSKUKey, "OperatingSystemSKU")]
  [InlineData(OsConstants.OrganizationKey, "Organization")]
  [InlineData(OsConstants.OSArchitectureKey, "OSArchitecture")]
  [InlineData(OsConstants.OSLanguageKey, "OSLanguage")]
  [InlineData(OsConstants.OSProductSuiteKey, "OSProductSuite")]
  [InlineData(OsConstants.OSTypeKey, "OSType")]
  [InlineData(OsConstants.OtherTypeDescriptionKey, "OtherTypeDescription")]
  [InlineData(OsConstants.PAEEnabledKey, "PAEEnabled")]
  [InlineData(OsConstants.PortableOperatingSystemKey, "PortableOperatingSystem")]
  [InlineData(OsConstants.PrimaryKey, "Primary")]
  [InlineData(OsConstants.ProductTypeKey, "ProductType")]
  [InlineData(OsConstants.RegisteredUserKey, "RegisteredUser")]
  [InlineData(OsConstants.SerialNumberKey, "SerialNumber")]
  public void PropertyKey_MatchesWin32OperatingSystemSchema(string key, string expected) {
    Assert.Equal(expected, key);
  }
}
