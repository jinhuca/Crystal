using Crystal.NetworkModule.Models;
using Crystal.NetworkModule.ViewModels;
using Crystal.Provider.Etw;
using Crystal.Service.Network;
using Microsoft.Reactive.Testing;
using Prism.Events;
using Xunit;

namespace Crystal.NetworkModule.Tests;

// End-to-end tests over the real network pipeline: a fake load source + a fake ETW source ->
// real EtwRateBroadcaster -> real ProcessNetworkSource -> real NetworkMonitor (driven by a
// TestScheduler) -> real NetworkModel -> real NetworkViewModel, wired the way the module wires them.
// Unlike NetworkViewModelTests (which pushes pre-built snapshots into the VM through a fake model)
// and NetworkMonitorTests (which stops at the monitor), these exercise the whole service->module
// seam: the sensor poll cadence, the ETW broadcaster's ranking of per-process rates, and the VM's
// reconciliation/label formatting all run for real. The VM is driven synchronously (the test runs on
// the STA/UI thread, so UiThreadMarshaller executes inline) so no dispatcher pumping is needed.
public class NetworkPipelineIntegrationTests {
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  // A load source whose per-poll snapshot is supplied by a caller-controlled callback, so a test can
  // make successive polls return changing values. Records Read() calls for cold/cadence assertions.
  private sealed class ScriptedLoadSource(Func<NetworkSnapshot> read) : INetworkLoadSource {
    public int ReadCount { get; private set; }
    public NetworkSnapshot Read() {
      ReadCount++;
      return read();
    }
  }

  // A scripted ETW source: reports running and returns caller-supplied per-PID rates each snapshot.
  // Mirrors the fake in Crystal.Service.Network.Tests (internal to that assembly, so not visible
  // here).
  private sealed class ScriptedEtwSource(Func<IReadOnlyDictionary<uint, ProcessEtwMetrics>> rates)
      : IProcessEtwSource {
    public bool IsRunning => true;
    public string? StartError => null;
    public IReadOnlyDictionary<uint, ProcessEtwMetrics> SnapshotRates() => rates();
    public void Pause() { }
    public void Resume() { }
    public void Dispose() { }
  }

  private static ProcessEtwMetrics Net(double bytesPerSec) => new(0, bytesPerSec, 0);

  private static NetworkInterfaceReading Wired(string name = "Ethernet",
      double up = 2048, double down = 4096) => new(name, 10, up, down);

  private static NetworkInterfaceReading Wifi(string name, string ssid, int signal) =>
      new(name, 10, 2048, 4096, WifiSsid: ssid, WifiSignalPercent: signal,
          WifiRxRateKbps: 866_000, WifiTxRateKbps: 866_000, WifiBssid: "AA:BB:CC:DD:EE:FF",
          WifiSecurity: "WPA2-Personal / CCMP");

  private static NetworkViewModel CreateVm(
      INetworkLoadSource loads, IProcessEtwSource etw, TestScheduler scheduler,
      Func<IReadOnlyDictionary<uint, string>>? names = null) {
    var broadcaster = new EtwRateBroadcaster(etw, Interval, scheduler);
    var processNetwork = new ProcessNetworkSource(broadcaster, names ?? (() => new Dictionary<uint, string>()));
    var monitor = new NetworkMonitor(loads, processNetwork, Interval, scheduler);
    var model = new NetworkModel(monitor);
    return new NetworkViewModel(model, new EventAggregator());
  }

  [Fact]
  public void Interface_readings_flow_through_the_monitor_and_model_into_the_view_model() {
    var loads = new ScriptedLoadSource(() => new NetworkSnapshot(
        [Wired("Ethernet", up: 2048, down: 4096), Wifi("Wi-Fi", "HomeNet", 72)],
        WifiStatus.Connected));
    var scheduler = new TestScheduler();
    var vm = CreateVm(loads, new ScriptedEtwSource(() => new Dictionary<uint, ProcessEtwMetrics>()), scheduler);

    scheduler.AdvanceBy(Interval.Ticks);

    Assert.Equal(2, vm.Adapters.Count);
    // Totals roll up across interfaces: 8192 B/s = 8.00 KiB/s.
    Assert.Equal("8.00 KiB/s", vm.DownloadLabel);   // 4096 + 4096
    Assert.Equal("4.00 KiB/s", vm.UploadLabel);     // 2048 + 2048
    Assert.True(vm.HasWifi);
    Assert.Equal("HomeNet  72%", vm.WifiLabel);
  }

  [Fact]
  public void Interfaces_reconcile_across_polls_as_adapters_come_and_go() {
    int poll = 0;
    var loads = new ScriptedLoadSource(() => {
      poll++;
      return poll == 1
          ? new NetworkSnapshot([Wired("Ethernet"), Wired("Ethernet 2")])
          : new NetworkSnapshot([Wired("Ethernet"), Wifi("Wi-Fi", "HomeNet", 72)], WifiStatus.Connected);
    });
    var scheduler = new TestScheduler();
    var vm = CreateVm(loads, new ScriptedEtwSource(() => new Dictionary<uint, ProcessEtwMetrics>()), scheduler);

    scheduler.AdvanceBy(Interval.Ticks);
    Assert.Equal(2, vm.Adapters.Count);
    Assert.False(vm.HasWifi);

    scheduler.AdvanceBy(Interval.Ticks);   // one NIC drops, a Wi-Fi adapter appears
    Assert.Equal(2, vm.Adapters.Count);
    Assert.True(vm.HasWifi);
  }

  [Fact]
  public void Top_talkers_flow_from_etw_through_the_process_source_ranked_into_the_view_model() {
    var loads = new ScriptedLoadSource(() => new NetworkSnapshot([Wired()]));
    var etw = new ScriptedEtwSource(() => new Dictionary<uint, ProcessEtwMetrics> {
      [10] = Net(5_000_000),
      [20] = Net(2_048),
    });
    var names = new Dictionary<uint, string> { [10] = "chrome", [20] = "svchost" };
    var scheduler = new TestScheduler();
    var vm = CreateVm(loads, etw, scheduler, () => names);

    scheduler.AdvanceBy(Interval.Ticks);

    var rows = vm.TopTalkersView.Cast<ProcessNetworkRowViewModel>().ToList();
    Assert.False(vm.HasTopTalkersStatus);
    Assert.Equal(2, rows.Count);
    Assert.Equal("chrome", rows[0].Name);          // ranked by rate descending
    Assert.Equal("4.77 MiB/s", rows[0].RateLabel);
    Assert.Equal("svchost", rows[1].Name);
  }

  [Fact]
  public void Zero_rate_processes_are_dropped_and_names_fall_back_to_pid() {
    var loads = new ScriptedLoadSource(() => new NetworkSnapshot([Wired()]));
    var etw = new ScriptedEtwSource(() => new Dictionary<uint, ProcessEtwMetrics> {
      [10] = Net(1_000_000),
      [20] = Net(0),              // no activity -> excluded from the ranking
    });
    var scheduler = new TestScheduler();
    // No name resolved for PID 10 -> "PID 10" fallback.
    var vm = CreateVm(loads, etw, scheduler, () => new Dictionary<uint, string>());

    scheduler.AdvanceBy(Interval.Ticks);

    var rows = vm.TopTalkersView.Cast<ProcessNetworkRowViewModel>().ToList();
    Assert.Single(rows);
    Assert.Equal("PID 10", rows[0].Name);
  }

  [Fact]
  public void The_sensor_poll_is_cold_until_the_view_model_subscribes() {
    var loads = new ScriptedLoadSource(() => new NetworkSnapshot([Wired()]));
    var scheduler = new TestScheduler();

    // Build the whole chain but stop before the VM so nothing subscribes to Sensors.
    var broadcaster = new EtwRateBroadcaster(
        new ScriptedEtwSource(() => new Dictionary<uint, ProcessEtwMetrics>()), Interval, scheduler);
    var monitor = new NetworkMonitor(loads, new ProcessNetworkSource(broadcaster,
        () => new Dictionary<uint, string>()), Interval, scheduler);
    var model = new NetworkModel(monitor);

    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);   // no subscriber yet -> no polling
    Assert.Equal(0, loads.ReadCount);

    var vm = new NetworkViewModel(model, new EventAggregator());   // the VM subscribes in its ctor
    scheduler.AdvanceBy(Interval.Ticks);

    Assert.True(loads.ReadCount > 0);
    Assert.Single(vm.Adapters);
  }
}
