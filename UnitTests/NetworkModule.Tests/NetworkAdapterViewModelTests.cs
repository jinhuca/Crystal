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
  public void Update_formats_utilization_link_speed_and_data_totals() {
    var vm = new NetworkAdapterViewModel();

    vm.Update(new NetworkInterfaceReading(
        "Ethernet", UtilizationPercent: 42.5, UploadBytesPerSecond: 0, DownloadBytesPerSecond: 0,
        DataUploadedGb: 0.5, DataDownloadedGb: 3.25, LinkSpeedBitsPerSecond: 1_000_000_000));

    Assert.Equal("42.5%", vm.UtilizationLabel);
    Assert.Equal("1.0 Gbps", vm.LinkSpeedLabel);
    Assert.Equal("3.25 GiB", vm.DataDownloadedLabel);
    Assert.Equal("512.0 MiB", vm.DataUploadedLabel);
  }

  [Fact]
  public void Unknown_link_speed_renders_as_placeholder() {
    var vm = new NetworkAdapterViewModel();

    vm.Update(new NetworkInterfaceReading(
        "Ethernet", UtilizationPercent: 0, UploadBytesPerSecond: 0, DownloadBytesPerSecond: 0,
        LinkSpeedBitsPerSecond: 0));

    Assert.Equal("—", vm.LinkSpeedLabel);
  }

  [Theory]
  [InlineData(0, "▁▁▁▁")]
  [InlineData(20, "▂▁▁▁")]
  [InlineData(50, "▂▄▁▁")]
  [InlineData(72, "▂▄▆▁")]
  [InlineData(100, "▂▄▆█")]
  public void Signal_bars_fill_proportionally_to_quality(int signal, string expected) {
    var vm = new NetworkAdapterViewModel();

    vm.Update(Wifi(signal: signal));

    Assert.Equal(expected, vm.WifiSignalBars);
  }

  [Fact]
  public void Signal_bars_are_empty_when_quality_is_unavailable() {
    var vm = new NetworkAdapterViewModel();

    vm.Update(Wifi(signal: null, rssi: null));

    Assert.Equal("▁▁▁▁", vm.WifiSignalBars);
  }

  [Fact]
  public void Switching_from_wifi_to_wired_clears_the_wifi_flag() {
    var vm = new NetworkAdapterViewModel();

    vm.Update(Wifi());
    vm.Update(Wired());

    Assert.False(vm.IsWifi);
  }
}
