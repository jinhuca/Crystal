using Crystal.NetworkModule.Models;
using Crystal.NetworkModule.ViewModels;
using Crystal.Service.Network;
using Crystal.Infrastructure.Core.Events;
using System.ComponentModel;
using System.Reactive.Subjects;
using Xunit;

namespace Crystal.NetworkModule.Tests;

public class NetworkViewModelTests {
  private sealed class FakeNetworkModel : INetworkModel {
    public Subject<NetworkSnapshot> Subject { get; } = new();
    public Subject<ProcessNetworkSnapshot> TopTalkersSubject { get; } = new();
    public IObservable<NetworkSnapshot> Sensors => Subject;
    public IObservable<ProcessNetworkSnapshot> TopTalkers => TopTalkersSubject;
  }

  private static NetworkViewModel CreateVm(out FakeNetworkModel model) {
    model = new FakeNetworkModel();
    return new NetworkViewModel(model, new EventAggregator());
  }

  private static NetworkInterfaceReading Wired(string name = "Ethernet") =>
      new(name, 10, 2048, 4096, DnsSuffix: "corp.example", IPv4Address: "10.0.0.5",
          IPv6Address: "fe80::1%3");

  private static NetworkInterfaceReading Wifi(string name, string ssid, int signal) =>
      new(name, 10, 2048, 4096, WifiSsid: ssid, WifiSignalPercent: signal,
          WifiRssiDbm: -60, WifiPhyType: "802.11ac", WifiChannel: 36, WifiBand: "5 GHz",
          WifiRxRateKbps: 866_000, WifiTxRateKbps: 866_000, WifiBssid: "AA:BB:CC:DD:EE:FF",
          WifiSecurity: "WPA2-Personal / CCMP", DnsSuffix: "Home", IPv4Address: "192.168.1.15",
          IPv6Address: "fe80::e3ec:b8e4:107c:8e44%8");

  [Fact]
  public void Adapters_reflect_the_active_interface_count() {
    var vm = CreateVm(out var model);

    // The summary tile's header binds Adapters.Count as "N active interface(s)".
    model.Subject.OnNext(new NetworkSnapshot([Wired("Ethernet"), Wifi("Wi-Fi", "HomeNet", 72)]));

    Assert.Equal(2, vm.Adapters.Count);
  }

  [Fact]
  public void Adapters_reconcile_by_name_across_polls() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wired("Ethernet"), Wired("Ethernet 2")]));
    // A later poll drops one interface and adds another; the count follows and no stale row lingers.
    model.Subject.OnNext(new NetworkSnapshot([Wired("Ethernet"), Wifi("Wi-Fi", "HomeNet", 72)]));

    Assert.Equal(2, vm.Adapters.Count);
  }

  [Fact]
  public void Wired_only_connection_hides_the_wifi_rows() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wired()], WifiStatus.None));

    Assert.False(vm.HasWifi);
    Assert.True(vm.HasConnection);
    Assert.Equal("Ethernet", vm.ConnectionAdapterName);
    Assert.Equal("—", vm.ConnectionSsid);
    Assert.False(vm.HasWifiStatus);
  }

  [Fact]
  public void Disabled_radio_with_no_connection_shows_muted_status_row() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([], WifiStatus.Disabled));

    Assert.False(vm.HasConnection);
    Assert.True(vm.HasWifiStatus);
    Assert.Equal("Wi-Fi disabled", vm.WifiStatusLabel);
  }

  [Fact]
  public void Disconnected_radio_with_no_connection_shows_muted_status_row() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([], WifiStatus.Disconnected));

    Assert.False(vm.HasConnection);
    Assert.True(vm.HasWifiStatus);
    Assert.Equal("Wi-Fi disconnected", vm.WifiStatusLabel);
  }

  [Fact]
  public void Connected_wifi_suppresses_the_status_row() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wifi("Wi-Fi", "HomeNet", 72)], WifiStatus.Connected));

    Assert.True(vm.HasWifi);
    Assert.True(vm.HasConnection);
    Assert.False(vm.HasWifiStatus);
  }

  [Fact]
  public void Status_row_clears_when_radio_reconnects() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([], WifiStatus.Disconnected));
    model.Subject.OnNext(new NetworkSnapshot([Wifi("Wi-Fi", "HomeNet", 72)], WifiStatus.Connected));

    Assert.False(vm.HasWifiStatus);
    Assert.True(vm.HasWifi);
  }

  [Fact]
  public void Connected_wifi_shows_ssid_and_signal_bars() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wired(), Wifi("Wi-Fi", "HomeNet", 72)]));

    Assert.True(vm.HasWifi);
    Assert.Equal("Wi-Fi", vm.ConnectionAdapterName);
    Assert.Equal("HomeNet", vm.ConnectionSsid);
    // 72% quality lights three of four segments.
    Assert.Equal("▂▄▆▁", vm.SignalBars);
  }

  [Fact]
  public void Connected_wifi_populates_the_taskmanager_detail_rows() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wifi("Wi-Fi", "HomeNet", 72)]));

    Assert.Equal("Home", vm.ConnectionDnsName);
    Assert.Equal("802.11ac", vm.ConnectionType);
    Assert.Equal("192.168.1.15", vm.ConnectionIPv4);
    Assert.Equal("fe80::e3ec:b8e4:107c:8e44%8", vm.ConnectionIPv6);
  }

  [Fact]
  public void Wifi_detail_rows_clear_when_adapter_disconnects() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wifi("Wi-Fi", "HomeNet", 72)]));
    model.Subject.OnNext(new NetworkSnapshot([Wired()]));

    Assert.False(vm.HasWifi);
    Assert.Equal("Ethernet", vm.ConnectionAdapterName);
    Assert.Equal("—", vm.ConnectionSsid);
    Assert.Equal("—", vm.ConnectionType);
    Assert.Equal("—", vm.SignalBars);
  }

  [Fact]
  public void Wifi_disabled_details_the_active_wired_connection() {
    var vm = CreateVm(out var model);

    // Wi-Fi off but a wired interface is moving traffic: the tile details it (adapter/DNS/addresses)
    // so the throughput has an obvious owner, with the Wi-Fi-only rows hidden.
    model.Subject.OnNext(new NetworkSnapshot([Wired("Ethernet")], WifiStatus.Disabled));

    Assert.False(vm.HasWifi);
    Assert.True(vm.HasConnection);
    Assert.Equal("Ethernet", vm.ConnectionAdapterName);
    Assert.Equal("10.0.0.5", vm.ConnectionIPv4);
    Assert.False(vm.HasWifiStatus);
  }

  [Fact]
  public void Connected_wifi_is_the_primary_connection_over_a_busier_wire() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wifi("Wi-Fi", "HomeNet", 72)], WifiStatus.Connected));

    Assert.True(vm.HasWifi);
    Assert.Equal("Wi-Fi", vm.ConnectionAdapterName);
  }

  [Fact]
  public void Non_wifi_primary_connection_is_sticky_when_the_busiest_nic_flips() {
    var vm = CreateVm(out var model);

    // Two addressed interfaces; the detail block seeds on the busiest with an IPv4.
    var eth = new NetworkInterfaceReading("Ethernet", 10, 100, 9000, IPv4Address: "10.0.0.5");
    var virt = new NetworkInterfaceReading("vEthernet", 10, 100, 100, IPv4Address: "172.16.0.9");
    model.Subject.OnNext(new NetworkSnapshot([eth, virt], WifiStatus.Disconnected));
    Assert.Equal("Ethernet", vm.ConnectionAdapterName);

    // Next poll the other NIC is busier, but the primary must not thrash — it stays put.
    model.Subject.OnNext(new NetworkSnapshot([
        eth with { DownloadBytesPerSecond = 100 },
        virt with { DownloadBytesPerSecond = 9000 },
    ], WifiStatus.Disconnected));

    Assert.Equal("Ethernet", vm.ConnectionAdapterName);
    Assert.Equal("10.0.0.5", vm.ConnectionIPv4);
  }

  [Fact]
  public void Primary_connection_reseeds_when_the_held_interface_drops() {
    var vm = CreateVm(out var model);

    var eth = new NetworkInterfaceReading("Ethernet", 10, 100, 9000, IPv4Address: "10.0.0.5");
    var wlanBridge = new NetworkInterfaceReading("Bridge", 10, 100, 100, IPv4Address: "172.16.0.9");
    model.Subject.OnNext(new NetworkSnapshot([eth, wlanBridge], WifiStatus.Disconnected));
    Assert.Equal("Ethernet", vm.ConnectionAdapterName);

    // Ethernet disappears: the held choice is gone, so the block reseeds onto what remains.
    model.Subject.OnNext(new NetworkSnapshot([wlanBridge], WifiStatus.Disconnected));

    Assert.Equal("Bridge", vm.ConnectionAdapterName);
    Assert.Equal("172.16.0.9", vm.ConnectionIPv4);
  }

  [Fact]
  public void Strongest_wifi_adapter_wins_when_several_are_connected() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([
        Wifi("Wi-Fi", "Weak", 40),
        Wifi("Wi-Fi 2", "Strong", 90),
    ]));

    Assert.True(vm.HasWifi);
    Assert.Equal("Wi-Fi 2", vm.ConnectionAdapterName);
    Assert.Equal("Strong", vm.ConnectionSsid);
  }

  [Fact]
  public void Wifi_rows_hide_when_adapter_disconnects() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wifi("Wi-Fi", "HomeNet", 72)]));
    model.Subject.OnNext(new NetworkSnapshot([Wired()]));

    Assert.False(vm.HasWifi);
    Assert.Equal("—", vm.ConnectionSsid);
  }

  // The bound, sorted view over the backing collection — the order the table actually shows.
  private static List<ProcessNetworkRowViewModel> SortedRows(NetworkViewModel vm) =>
      vm.TopTalkersView.Cast<ProcessNetworkRowViewModel>().ToList();

  [Fact]
  public void Top_talkers_default_sort_is_rate_descending() {
    var vm = CreateVm(out var model);

    model.TopTalkersSubject.OnNext(new ProcessNetworkSnapshot([
        new ProcessNetworkReading(10, "chrome", 5_000_000),
        new ProcessNetworkReading(20, "svchost", 2_048),
    ], IsRunning: true, StatusError: null));

    var rows = SortedRows(vm);
    Assert.False(vm.HasTopTalkersStatus);
    Assert.Equal(2, rows.Count);
    Assert.Equal("chrome", rows[0].Name);
    Assert.Equal("4.77 MiB/s", rows[0].RateLabel);
    Assert.Equal("2.00 KiB/s", rows[1].RateLabel);
  }

  [Fact]
  public void Top_talkers_reconcile_and_resort_across_polls() {
    var vm = CreateVm(out var model);

    model.TopTalkersSubject.OnNext(new ProcessNetworkSnapshot([
        new ProcessNetworkReading(10, "chrome", 5_000_000),
        new ProcessNetworkReading(20, "svchost", 2_048),
    ], IsRunning: true, StatusError: null));

    // svchost surges past chrome and a new PID appears; chrome drops out of the ranking.
    model.TopTalkersSubject.OnNext(new ProcessNetworkSnapshot([
        new ProcessNetworkReading(20, "svchost", 9_000_000),
        new ProcessNetworkReading(30, "steam", 1_000_000),
    ], IsRunning: true, StatusError: null));

    var rows = SortedRows(vm);
    Assert.Equal(2, rows.Count);
    Assert.Equal(20u, rows[0].ProcessId);
    Assert.Equal(30u, rows[1].ProcessId);
    Assert.DoesNotContain(rows, r => r.ProcessId == 10u);
  }

  [Fact]
  public void Sorting_by_name_toggles_direction_on_repeat_click() {
    var vm = CreateVm(out var model);

    model.TopTalkersSubject.OnNext(new ProcessNetworkSnapshot([
        new ProcessNetworkReading(10, "chrome", 5_000_000),
        new ProcessNetworkReading(20, "svchost", 2_048),
        new ProcessNetworkReading(30, "alpha", 1_000),
    ], IsRunning: true, StatusError: null));

    vm.SortTopTalkersBy(nameof(ProcessNetworkRowViewModel.Name));
    Assert.Equal(ListSortDirection.Ascending, vm.TopTalkersSortDirection);
    Assert.Equal("alpha", SortedRows(vm)[0].Name);

    vm.SortTopTalkersBy(nameof(ProcessNetworkRowViewModel.Name));
    Assert.Equal(ListSortDirection.Descending, vm.TopTalkersSortDirection);
    Assert.Equal("svchost", SortedRows(vm)[0].Name);
  }

  [Fact]
  public void Sorting_by_a_new_column_starts_descending_for_rate() {
    var vm = CreateVm(out var model);

    model.TopTalkersSubject.OnNext(new ProcessNetworkSnapshot([
        new ProcessNetworkReading(10, "chrome", 2_048),
        new ProcessNetworkReading(20, "svchost", 9_000_000),
    ], IsRunning: true, StatusError: null));

    // Switch to Name, then back to rate: a new column resets to descending.
    vm.SortTopTalkersBy(nameof(ProcessNetworkRowViewModel.Name));
    vm.SortTopTalkersBy(nameof(ProcessNetworkRowViewModel.RateBytesPerSecond));

    Assert.Equal(ListSortDirection.Descending, vm.TopTalkersSortDirection);
    Assert.Equal(20u, SortedRows(vm)[0].ProcessId);
  }

  [Fact]
  public void Summary_top_talkers_are_capped_at_three_in_rank_order() {
    var vm = CreateVm(out var model);

    model.TopTalkersSubject.OnNext(new ProcessNetworkSnapshot([
        new ProcessNetworkReading(10, "a", 8_000_000),
        new ProcessNetworkReading(20, "b", 7_000_000),
        new ProcessNetworkReading(30, "c", 6_000_000),
        new ProcessNetworkReading(40, "d", 5_000_000),
        new ProcessNetworkReading(50, "e", 4_000_000),
        new ProcessNetworkReading(60, "f", 3_000_000),
        new ProcessNetworkReading(70, "g", 2_000_000),
    ], IsRunning: true, StatusError: null));

    Assert.Equal(3, vm.SummaryTopTalkers.Count);
    Assert.Equal(new[] { 10u, 20u, 30u },
        vm.SummaryTopTalkers.Select(r => r.ProcessId).ToArray());
    // Shares the same row instances as the full ranking.
    Assert.Same(vm.TopTalkers.Single(r => r.ProcessId == 10u), vm.SummaryTopTalkers[0]);
  }

  [Fact]
  public void Summary_top_talkers_reconcile_across_polls() {
    var vm = CreateVm(out var model);

    model.TopTalkersSubject.OnNext(new ProcessNetworkSnapshot([
        new ProcessNetworkReading(10, "chrome", 5_000_000),
        new ProcessNetworkReading(20, "svchost", 2_048),
    ], IsRunning: true, StatusError: null));

    model.TopTalkersSubject.OnNext(new ProcessNetworkSnapshot([
        new ProcessNetworkReading(30, "steam", 9_000_000),
    ], IsRunning: true, StatusError: null));

    Assert.Equal(new[] { 30u }, vm.SummaryTopTalkers.Select(r => r.ProcessId).ToArray());
  }

  [Fact]
  public void Top_talkers_show_status_when_etw_is_not_running() {
    var vm = CreateVm(out var model);

    model.TopTalkersSubject.OnNext(new ProcessNetworkSnapshot([], IsRunning: false,
        StatusError: "not elevated"));

    Assert.True(vm.HasTopTalkersStatus);
    Assert.Contains("not elevated", vm.TopTalkersStatusLabel);
    Assert.Empty(vm.TopTalkers);
  }
}
