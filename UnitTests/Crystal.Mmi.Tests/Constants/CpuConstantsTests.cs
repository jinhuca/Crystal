using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class CpuConstantsTests {
  [Fact]
  public void QueryString_SelectsAllFromWin32Processor() {
    Assert.Equal("SELECT * FROM Win32_Processor", CpuConstants.QueryString);
  }

  [Theory]
  [InlineData(CpuConstants.AddressWidthKey, "AddressWidth")]
  [InlineData(CpuConstants.ArchitectureKey, "Architecture")]
  [InlineData(CpuConstants.AssetTagKey, "AssetTag")]
  [InlineData(CpuConstants.AvailabilityKey, "Availability")]
  [InlineData(CpuConstants.CaptionKey, "Caption")]
  [InlineData(CpuConstants.CharacteristicsKey, "Characteristics")]
  [InlineData(CpuConstants.ConfigManagerErrorCodeKey, "ConfigManagerErrorCode")]
  [InlineData(CpuConstants.ConfigManagerUserConfigKey, "ConfigManagerUserConfig")]
  [InlineData(CpuConstants.CpuStatusKey, "CpuStatus")]
  [InlineData(CpuConstants.CreationClassNameKey, "CreationClassName")]
  [InlineData(CpuConstants.CurrentClockSpeedKey, "CurrentClockSpeed")]
  [InlineData(CpuConstants.CurrentVoltageKey, "CurrentVoltage")]
  [InlineData(CpuConstants.DataWidthKey, "DataWidth")]
  [InlineData(CpuConstants.DescriptionKey, "Description")]
  [InlineData(CpuConstants.DeviceIDKey, "DeviceID")]
  [InlineData(CpuConstants.ErrorClearedKey, "ErrorCleared")]
  [InlineData(CpuConstants.ErrorDescriptionKey, "ErrorDescription")]
  [InlineData(CpuConstants.ExtClockKey, "ExtClock")]
  [InlineData(CpuConstants.FamilyKey, "Family")]
  [InlineData(CpuConstants.InstallDateKey, "InstallDate")]
  [InlineData(CpuConstants.L2CacheSizeKey, "L2CacheSize")]
  [InlineData(CpuConstants.L2CacheSpeedKey, "L2CacheSpeed")]
  [InlineData(CpuConstants.L3CacheSizeKey, "L3CacheSize")]
  [InlineData(CpuConstants.L3CacheSpeedKey, "L3CacheSpeed")]
  [InlineData(CpuConstants.LastErrorCodeKey, "LastErrorCode")]
  [InlineData(CpuConstants.LevelKey, "Level")]
  [InlineData(CpuConstants.LoadPercentageKey, "LoadPercentage")]
  [InlineData(CpuConstants.NameKey, "Name")]
  [InlineData(CpuConstants.ManufacturerKey, "Manufacturer")]
  [InlineData(CpuConstants.MaxClockSpeedKey, "MaxClockSpeed")]
  [InlineData(CpuConstants.NumberOfCoresKey, "NumberOfCores")]
  [InlineData(CpuConstants.NumberOfEnabledCoreKey, "NumberOfEnabledCore")]
  [InlineData(CpuConstants.NumberOfLogicalProcessorsKey, "NumberOfLogicalProcessors")]
  [InlineData(CpuConstants.OtherFamilyDescriptionKey, "OtherFamilyDescription")]
  [InlineData(CpuConstants.PartNumberKey, "PartNumber")]
  [InlineData(CpuConstants.PNPDeviceIDKey, "PNPDeviceID")]
  [InlineData(CpuConstants.PowerManagementCapabilitiesKey, "PowerManagementCapabilities")]
  [InlineData(CpuConstants.ProcessorIdKey, "ProcessorId")]
  [InlineData(CpuConstants.ProcessorTypeKey, "ProcessorType")]
  [InlineData(CpuConstants.RevisionKey, "Revision")]
  [InlineData(CpuConstants.RoleKey, "Role")]
  [InlineData(CpuConstants.SerialNumberKey, "SerialNumber")]
  [InlineData(CpuConstants.SocketDesignationKey, "SocketDesignation")]
  [InlineData(CpuConstants.StatusKey, "Status")]
  [InlineData(CpuConstants.StatusInfoKey, "StatusInfo")]
  [InlineData(CpuConstants.SteppingKey, "Stepping")]
  [InlineData(CpuConstants.SystemCreationClassNameKey, "SystemCreationClassName")]
  [InlineData(CpuConstants.SystemNameKey, "SystemName")]
  [InlineData(CpuConstants.ThreadCountKey, "ThreadCount")]
  [InlineData(CpuConstants.UniqueIdKey, "UniqueId")]
  [InlineData(CpuConstants.UpgradeMethodKey, "UpgradeMethod")]
  [InlineData(CpuConstants.VersionKey, "Version")]
  [InlineData(CpuConstants.VirtualizationFirmwareEnabledKey, "VirtualizationFirmwareEnabled")]
  [InlineData(CpuConstants.VMMonitorModeExtensionsKey, "VMMonitorModeExtensions")]
  [InlineData(CpuConstants.VoltageCapsKey, "VoltageCaps")]
  public void PropertyKey_MatchesWin32ProcessorSchema(string key, string expected) {
    Assert.Equal(expected, key);
  }

  [Fact]
  public void SpeedUnit_IsMHz() {
    Assert.Equal("MHz", CpuConstants.SpeedUnit);
  }

  [Fact]
  public void CacheSizeUnit_IsKB() {
    Assert.Equal("KB", CpuConstants.CacheSizeUnit);
  }
}
