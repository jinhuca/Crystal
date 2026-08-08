using NetworkModule.Models;
using NetworkModule.ViewModels;
using Xunit;

namespace NetworkModule.Tests;

public class NetworkAdapterViewModelTests {
  private static NetworkInterfaceReading Wired(string name = "Ethernet") =>
      new(name, UtilizationPercent: 10, UploadBytesPerSecond: 2048, DownloadBytesPerSecond: 4096);

  private static NetworkInterfaceReading Wifi(
      string name = "Wi-Fi", string? ssid = "HomeNet", int? signal = 80, int? rssi = -60,
      string? phy = "Wi-Fi 6 (802.11ax)", int? channel = 36, string? band = "5 GHz") =>
      new(name, UtilizationPercent: 10, UploadBytesPerSecond: 2048, DownloadBytesPerSecond: 4096,
          WifiSsid: ssid, WifiSignalPercent: signal, WifiRssiDbm: rssi,
          WifiPhyType: phy, WifiChannel: channel, WifiBand: band);

  [Fact]
  public void Wired_reading_is_not_flagged_as_wifi() {
    var vm = new NetworkAdapterViewModel();

    vm.Update(Wired());

    Assert.False(vm.IsWifi);
    Assert.Equal("—", vm.WifiSsid);
    Assert.Equal("—", vm.WifiSignal);
    Assert.Equal("—", vm.WifiBand);
    Assert.Equal("—", vm.WifiChannel);
    Assert.Equal("—", vm.WifiPhyType);
  }

  [Fact]
  public void Wifi_reading_populates_all_fields() {
    var vm = new NetworkAdapterViewModel();

    vm.Update(Wifi());

    Assert.True(vm.IsWifi);
    Assert.Equal("HomeNet", vm.WifiSsid);
    Assert.Equal("80%  (-60 dBm)", vm.WifiSignal);
    Assert.Equal("Wi-Fi 6 (802.11ax)", vm.WifiPhyType);
    Assert.Equal("5 GHz", vm.WifiBand);
    Assert.Equal("36", vm.WifiChannel);
  }

  [Fact]
  public void Wifi_signal_without_rssi_shows_percent_only() {
    var vm = new NetworkAdapterViewModel();

    vm.Update(Wifi(rssi: null));

    Assert.Equal("80%", vm.WifiSignal);
  }

  [Fact]
  public void Wifi_with_only_ssid_is_still_flagged_as_wifi() {
    var vm = new NetworkAdapterViewModel();

    vm.Update(Wifi(signal: null, rssi: null, phy: null, channel: null, band: null));

    Assert.True(vm.IsWifi);
    Assert.Equal("HomeNet", vm.WifiSsid);
    Assert.Equal("—", vm.WifiSignal);
    Assert.Equal("—", vm.WifiBand);
    Assert.Equal("—", vm.WifiChannel);
  }

  [Fact]
  public void Switching_from_wifi_to_wired_clears_the_wifi_flag() {
    var vm = new NetworkAdapterViewModel();

    vm.Update(Wifi());
    vm.Update(Wired());

    Assert.False(vm.IsWifi);
  }
}
