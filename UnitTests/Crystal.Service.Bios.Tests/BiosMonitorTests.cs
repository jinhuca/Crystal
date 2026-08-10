using System.Reactive.Linq;
using Crystal.Provider.Mmi.HardwareFeatures.FirmwareSecurity;
using Crystal.Provider.Mmi.MmiEngine;
using Xunit;

namespace Crystal.Service.Bios.Tests;

public class BiosMonitorTests {
  private static FirmwareInfoBuilder Builder() =>
      new(new FakeWmiProvider(bios: new Dictionary<string, WmiValue> {
            ["Manufacturer"] = new WmiValue("AMI"),
          }),
          new FakeSmbiosProvider(null),
          new FakeSecurityProvider(SecureBootState.Unknown));

  [Fact]
  public async Task Firmware_emits_the_built_snapshot() {
    using var monitor = new BiosMonitor(Builder());

    var snap = await monitor.Firmware.FirstAsync();

    Assert.Equal("AMI", snap.Manufacturer);
  }

  [Fact]
  public async Task Firmware_replays_same_snapshot_to_late_subscribers() {
    using var monitor = new BiosMonitor(Builder());

    var first = await monitor.Firmware.FirstAsync();
    var second = await monitor.Firmware.FirstAsync();

    // Replay(1) caches the single emission — the build runs once, both see the same instance.
    Assert.Same(first, second);
  }

  [Fact]
  public void Ctor_throws_on_null_builder() =>
      Assert.Throws<ArgumentNullException>(() => new BiosMonitor(null!));
}
