using System.Reactive.Subjects;
using NetworkModule.Models;
using NetworkModule.ViewModels;
using Prism.Events;
using Xunit;

namespace NetworkModule.Tests;

public class NetworkViewModelTests {
  private sealed class FakeNetworkModel : INetworkModel {
    public Subject<NetworkSnapshot> Subject { get; } = new();
    public IObservable<NetworkSnapshot> Sensors => Subject;
  }

  private static NetworkViewModel CreateVm(out FakeNetworkModel model) {
    model = new FakeNetworkModel();
    return new NetworkViewModel(model, new EventAggregator());
  }

  private static NetworkInterfaceReading Wired(string name = "Ethernet") =>
      new(name, 10, 2048, 4096);

  private static NetworkInterfaceReading Wifi(string name, string ssid, int signal) =>
      new(name, 10, 2048, 4096, WifiSsid: ssid, WifiSignalPercent: signal,
          WifiRssiDbm: -60, WifiPhyType: "Wi-Fi 6 (802.11ax)", WifiChannel: 36, WifiBand: "5 GHz",
          WifiRxRateKbps: 866_000, WifiTxRateKbps: 866_000, WifiBssid: "AA:BB:CC:DD:EE:FF",
          WifiSecurity: "WPA2-Personal / CCMP");

  [Fact]
  public void No_wifi_adapter_hides_the_wifi_row() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wired()]));

    Assert.False(vm.HasWifi);
    Assert.Equal("—", vm.WifiLabel);
  }

  [Fact]
  public void Single_wifi_adapter_shows_ssid_and_signal() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wired(), Wifi("Wi-Fi", "HomeNet", 72)]));

    Assert.True(vm.HasWifi);
    Assert.Equal("HomeNet  72%", vm.WifiLabel);
  }

  [Fact]
  public void Wifi_adapter_populates_link_rate_security_and_bssid() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wifi("Wi-Fi", "HomeNet", 72)]));

    Assert.Equal("866 Mbps", vm.WifiLinkRate);
    Assert.Equal("WPA2-Personal / CCMP", vm.WifiSecurity);
    Assert.Equal("AA:BB:CC:DD:EE:FF", vm.WifiBssid);
  }

  [Fact]
  public void Wifi_summary_fields_clear_when_adapter_disconnects() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wifi("Wi-Fi", "HomeNet", 72)]));
    model.Subject.OnNext(new NetworkSnapshot([Wired()]));

    Assert.Equal("—", vm.WifiLinkRate);
    Assert.Equal("—", vm.WifiSecurity);
    Assert.Equal("—", vm.WifiBssid);
  }

  [Fact]
  public void Strongest_wifi_adapter_wins_when_several_are_connected() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([
        Wifi("Wi-Fi", "Weak", 40),
        Wifi("Wi-Fi 2", "Strong", 90),
    ]));

    Assert.True(vm.HasWifi);
    Assert.Equal("Strong  90%", vm.WifiLabel);
  }

  [Fact]
  public void Wifi_row_clears_when_adapter_disconnects() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wifi("Wi-Fi", "HomeNet", 72)]));
    model.Subject.OnNext(new NetworkSnapshot([Wired()]));

    Assert.False(vm.HasWifi);
    Assert.Equal("—", vm.WifiLabel);
  }
}
