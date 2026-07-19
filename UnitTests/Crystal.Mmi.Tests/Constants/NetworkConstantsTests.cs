using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class NetworkConstantsTests {
  [Fact]
  public void QueryString_SelectsAllFromWin32NetworkAdapter() {
    Assert.Equal("SELECT * FROM Win32_NetworkAdapter", NetworkConstants.QueryString);
  }

  [Theory]
  [InlineData(NetworkConstants.AdapterTypeKey, "AdapterType")]
  [InlineData(NetworkConstants.AdapterTypeIdKey, "AdapterTypeId")]
  [InlineData(NetworkConstants.AutoSenseKey, "AutoSense")]
  [InlineData(NetworkConstants.AvailabilityKey, "Availability")]
  [InlineData(NetworkConstants.CaptionKey, "Caption")]
  [InlineData(NetworkConstants.ConfigManagerErrorCodeKey, "ConfigManagerErrorCode")]
  [InlineData(NetworkConstants.CreationClassNameKey, "CreationClassName")]
  [InlineData(NetworkConstants.DescriptionKey, "Description")]
  [InlineData(NetworkConstants.DeviceIDKey, "DeviceID")]
  [InlineData(NetworkConstants.GUIDKey, "GUID")]
  [InlineData(NetworkConstants.IndexKey, "Index")]
  [InlineData(NetworkConstants.InstallDateKey, "InstallDate")]
  [InlineData(NetworkConstants.InstalledKey, "Installed")]
  [InlineData(NetworkConstants.InterfaceIndexKey, "InterfaceIndex")]
  [InlineData(NetworkConstants.MACAddressKey, "MACAddress")]
  [InlineData(NetworkConstants.ManufacturerKey, "Manufacturer")]
  [InlineData(NetworkConstants.MaxSpeedKey, "MaxSpeed")]
  [InlineData(NetworkConstants.NameKey, "Name")]
  [InlineData(NetworkConstants.NetConnectionIDKey, "NetConnectionID")]
  [InlineData(NetworkConstants.NetConnectionStatusKey, "NetConnectionStatus")]
  [InlineData(NetworkConstants.NetEnabledKey, "NetEnabled")]
  [InlineData(NetworkConstants.PhysicalAdapterKey, "PhysicalAdapter")]
  [InlineData(NetworkConstants.PNPDeviceIDKey, "PNPDeviceID")]
  [InlineData(NetworkConstants.ProductNameKey, "ProductName")]
  [InlineData(NetworkConstants.ServiceNameKey, "ServiceName")]
  [InlineData(NetworkConstants.SpeedKey, "Speed")]
  [InlineData(NetworkConstants.StatusKey, "Status")]
  [InlineData(NetworkConstants.StatusInfoKey, "StatusInfo")]
  [InlineData(NetworkConstants.SystemCreationClassNameKey, "SystemCreationClassName")]
  [InlineData(NetworkConstants.SystemNameKey, "SystemName")]
  [InlineData(NetworkConstants.TimeOfLastResetKey, "TimeOfLastReset")]
  public void PropertyKey_MatchesWin32NetworkAdapterSchema(string key, string expected) {
    Assert.Equal(expected, key);
  }
}
