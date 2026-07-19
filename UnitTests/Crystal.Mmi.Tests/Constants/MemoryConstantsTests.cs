using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class MemoryConstantsTests {
  [Fact]
  public void QueryString_SelectsAllFromWin32PhysicalMemory() {
    Assert.Equal("SELECT * FROM Win32_PhysicalMemory", MemoryConstants.QueryString);
  }

  [Theory]
  [InlineData(MemoryConstants.AttributesKey, "Attributes")]
  [InlineData(MemoryConstants.BankLabelKey, "BankLabel")]
  [InlineData(MemoryConstants.CapacityKey, "Capacity")]
  [InlineData(MemoryConstants.CaptionKey, "Caption")]
  [InlineData(MemoryConstants.ConfiguredClockSpeedKey, "ConfiguredClockSpeed")]
  [InlineData(MemoryConstants.ConfiguredVoltageKey, "ConfiguredVoltage")]
  [InlineData(MemoryConstants.CreationClassNameKey, "CreationClassName")]
  [InlineData(MemoryConstants.DataWidthKey, "DataWidth")]
  [InlineData(MemoryConstants.DescriptionKey, "Description")]
  [InlineData(MemoryConstants.DeviceLocatorKey, "DeviceLocator")]
  [InlineData(MemoryConstants.FormFactorKey, "FormFactor")]
  [InlineData(MemoryConstants.HotSwappableKey, "HotSwappable")]
  [InlineData(MemoryConstants.InstallDateKey, "InstallDate")]
  [InlineData(MemoryConstants.InterleaveDataDepthKey, "InterleaveDataDepth")]
  [InlineData(MemoryConstants.InterleavePositionKey, "InterleavePosition")]
  [InlineData(MemoryConstants.ManufacturerKey, "Manufacturer")]
  [InlineData(MemoryConstants.MaxVoltageKey, "MaxVoltage")]
  [InlineData(MemoryConstants.MemoryTypeKey, "MemoryType")]
  [InlineData(MemoryConstants.MinVoltageKey, "MinVoltage")]
  [InlineData(MemoryConstants.ModelKey, "Model")]
  [InlineData(MemoryConstants.NameKey, "Name")]
  [InlineData(MemoryConstants.OtherIdentifyingInfoKey, "OtherIdentifyingInfo")]
  [InlineData(MemoryConstants.PartNumberKey, "PartNumber")]
  [InlineData(MemoryConstants.PositionInRowKey, "PositionInRow")]
  [InlineData(MemoryConstants.PoweredOnKey, "PoweredOn")]
  [InlineData(MemoryConstants.RemovableKey, "Removable")]
  [InlineData(MemoryConstants.ReplaceableKey, "Replaceable")]
  [InlineData(MemoryConstants.SerialNumberKey, "SerialNumber")]
  [InlineData(MemoryConstants.SKUKey, "SKU")]
  [InlineData(MemoryConstants.SMBIOSMemoryTypeKey, "SMBIOSMemoryType")]
  [InlineData(MemoryConstants.SpeedKey, "Speed")]
  [InlineData(MemoryConstants.StatusKey, "Status")]
  [InlineData(MemoryConstants.TagKey, "Tag")]
  [InlineData(MemoryConstants.TotalWidthKey, "TotalWidth")]
  [InlineData(MemoryConstants.TypeDetailKey, "TypeDetail")]
  [InlineData(MemoryConstants.VersionKey, "Version")]
  public void PropertyKey_MatchesWin32PhysicalMemorySchema(string key, string expected) {
    Assert.Equal(expected, key);
  }
}
