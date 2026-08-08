using System.ComponentModel;
using System.Reactive.Subjects;
using NetworkModule.Models;
using NetworkModule.ViewModels;
using Prism.Events;
using Xunit;

namespace NetworkModule.Tests;

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
      new(name, 10, 2048, 4096);

  private static NetworkInterfaceReading Wifi(string name, string ssid, int signal) =>
      new(name, 10, 2048, 4096, WifiSsid: ssid, WifiSignalPercent: signal,
          WifiRssiDbm: -60, WifiPhyType: "Wi-Fi 6 (802.11ax)", WifiChannel: 36, WifiBand: "5 GHz",
          WifiRxRateKbps: 866_000, WifiTxRateKbps: 866_000, WifiBssid: "AA:BB:CC:DD:EE:FF",
          WifiSecurity: "WPA2-Personal / CCMP");

  [Fact]
  public void No_wifi_adapter_hides_the_wifi_row() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wired()], WifiStatus.None));

    Assert.False(vm.HasWifi);
    Assert.Equal("—", vm.WifiLabel);
    Assert.False(vm.HasWifiStatus);
  }

  [Fact]
  public void Disabled_radio_shows_muted_status_row() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wired()], WifiStatus.Disabled));

    Assert.False(vm.HasWifi);
    Assert.True(vm.HasWifiStatus);
    Assert.Equal("Wi-Fi disabled", vm.WifiStatusLabel);
  }

  [Fact]
  public void Disconnected_radio_shows_muted_status_row() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wired()], WifiStatus.Disconnected));

    Assert.False(vm.HasWifi);
    Assert.True(vm.HasWifiStatus);
    Assert.Equal("Wi-Fi disconnected", vm.WifiStatusLabel);
  }

  [Fact]
  public void Connected_radio_suppresses_the_status_row() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wifi("Wi-Fi", "HomeNet", 72)], WifiStatus.Connected));

    Assert.True(vm.HasWifi);
    Assert.False(vm.HasWifiStatus);
  }

  [Fact]
  public void Status_row_clears_when_radio_reconnects() {
    var vm = CreateVm(out var model);

    model.Subject.OnNext(new NetworkSnapshot([Wired()], WifiStatus.Disconnected));
    model.Subject.OnNext(new NetworkSnapshot([Wifi("Wi-Fi", "HomeNet", 72)], WifiStatus.Connected));

    Assert.False(vm.HasWifiStatus);
    Assert.True(vm.HasWifi);
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
  public void Summary_top_talkers_are_capped_at_five_in_rank_order() {
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

    Assert.Equal(5, vm.SummaryTopTalkers.Count);
    Assert.Equal(new[] { 10u, 20u, 30u, 40u, 50u },
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
