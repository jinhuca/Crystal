using System.Reactive.Linq;
using Crystal.Provider.Etw;
using Microsoft.Reactive.Testing;
using Xunit;

namespace Crystal.Service.Network.Tests;

public class NetworkMonitorTests {
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  private static NetworkMonitor Build(FakeNetworkLoadSource loads, TestScheduler scheduler) {
    var broadcaster = new EtwRateBroadcaster(new FakeEtwSource(), Interval, scheduler);
    var processNetwork = new ProcessNetworkSource(broadcaster);
    return new NetworkMonitor(loads, processNetwork, Interval, scheduler);
  }

  [Fact]
  public void Sensors_emits_one_snapshot_per_interval() {
    var scheduler = new TestScheduler();
    var reading = new NetworkInterfaceReading("Ethernet", UtilizationPercent: 12,
        UploadBytesPerSecond: 1_000, DownloadBytesPerSecond: 2_000);
    var loads = new FakeNetworkLoadSource(new NetworkSnapshot([reading], WifiStatus.Connected));
    var monitor = Build(loads, scheduler);

    var received = new List<NetworkSnapshot>();
    using var _ = monitor.Sensors.Subscribe(received.Add);
    scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);

    Assert.Equal(3, received.Count);
    Assert.Equal("Ethernet", received[0].Interfaces[0].Name);
    Assert.Equal(WifiStatus.Connected, received[0].WifiStatus);
    Assert.Equal(3, loads.ReadCount);
  }

  [Fact]
  public void Sensors_is_cold_until_subscribed() {
    var scheduler = new TestScheduler();
    var loads = new FakeNetworkLoadSource();
    var monitor = Build(loads, scheduler);

    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

    Assert.Equal(0, loads.ReadCount);
  }

  [Fact]
  public void Sensors_stops_polling_after_unsubscribe() {
    var scheduler = new TestScheduler();
    var loads = new FakeNetworkLoadSource();
    var monitor = Build(loads, scheduler);

    var subscription = monitor.Sensors.Subscribe();
    scheduler.AdvanceBy(TimeSpan.FromSeconds(2).Ticks);
    subscription.Dispose();
    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

    Assert.Equal(2, loads.ReadCount);
  }

  [Fact]
  public void Sensors_shares_one_poll_across_concurrent_subscribers() {
    var scheduler = new TestScheduler();
    var loads = new FakeNetworkLoadSource();
    var monitor = Build(loads, scheduler);

    using var a = monitor.Sensors.Subscribe();
    using var b = monitor.Sensors.Subscribe();
    scheduler.AdvanceBy(TimeSpan.FromSeconds(2).Ticks);

    // Publish().RefCount() means both subscribers share a single poll per interval, not one each.
    Assert.Equal(2, loads.ReadCount);
  }

  [Fact]
  public void TopTalkers_is_forwarded_from_the_process_source() {
    var scheduler = new TestScheduler();
    var loads = new FakeNetworkLoadSource();
    var monitor = Build(loads, scheduler);

    ProcessNetworkSnapshot? result = null;
    using var _ = monitor.TopTalkers.Subscribe(s => result = s);
    scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

    Assert.NotNull(result);
    Assert.True(result!.IsRunning);
    Assert.Empty(result.TopTalkers);
  }
}
