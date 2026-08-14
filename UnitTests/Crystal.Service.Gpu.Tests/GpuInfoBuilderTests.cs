using Crystal.Provider.Mmi.MmiEngine;
using System.Collections.Frozen;
using Xunit;

namespace Crystal.Service.Gpu.Tests;

public class GpuInfoBuilderTests {
  // 1 GiB in bytes — stays within int range so it round-trips through the WMI Int getter (WMI's
  // AdapterRAM is a uint but the extension reads it as a signed int).
  private const int OneGiB = 1024 * 1024 * 1024;

  private static GpuInfoBuilder Build(
      IReadOnlyList<FrozenDictionary<string, WmiValue>> controllers,
      IGpuLoadSource loads) =>
      new(new FakeWmiHardwareProvider(controllers), loads);

  [Fact]
  public async Task BuildAsync_MapsAdapterFieldsAndPairsLoads() {
    var loads = new FakeGpuLoadSource(
        new GpuLoadReading("NVIDIA GeForce RTX 4070", CoreLoadPercent: 42, TemperatureC: 55, ClockMhz: 2400, PowerW: 120));
    var builder = Build(
        [VideoRows.Controller(name: "NVIDIA GeForce RTX 4070", adapterRamBytes: OneGiB,
                              driverVersion: "551.23", videoProcessor: "GeForce RTX 4070",
                              refreshRate: 144, horizontalRes: 2560, verticalRes: 1440)],
        loads);

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    var adapter = Assert.Single(snapshot.Adapters);
    Assert.Equal("NVIDIA GeForce RTX 4070", adapter.Name);
    Assert.Equal(1.0, adapter.VideoRamGB);
    Assert.Equal("551.23", adapter.DriverVersion);
    Assert.Equal("GeForce RTX 4070", adapter.VideoProcessor);
    Assert.Equal(144u, adapter.RefreshRateHz);
    Assert.Equal("2560 x 1440 @ 144Hz (0-bit color)", adapter.DisplayMode);

    var load = Assert.Single(snapshot.Loads);
    Assert.Equal("NVIDIA GeForce RTX 4070", load.AdapterName);
    Assert.Equal(42, load.CoreLoadPercent);
  }

  [Theory]
  [InlineData("Intel(R) UHD Graphics 770", GpuKind.Integrated)]
  [InlineData("AMD Radeon(TM) Graphics", GpuKind.Integrated)]
  [InlineData("Intel(R) Iris(R) Xe Graphics", GpuKind.Integrated)]
  [InlineData("NVIDIA GeForce RTX 4070", GpuKind.Dedicated)]
  [InlineData("AMD Radeon RX 7900 XTX", GpuKind.Dedicated)]
  public async Task BuildAsync_ClassifiesIntegratedVsDedicatedByName(string name, GpuKind expected) {
    var builder = Build([VideoRows.Controller(name: name)], new FakeGpuLoadSource());

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal(expected, snapshot.Adapters[0].Kind);
  }

  [Fact]
  public async Task BuildAsync_DriverDateFallsBackToInfDateWhenNoPnpKey() {
    var infDate = new DateTime(2024, 3, 15);
    var builder = Build([VideoRows.Controller(name: "NVIDIA GeForce RTX 4070", infDate: infDate)],
        new FakeGpuLoadSource());

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal(infDate, snapshot.Adapters[0].DriverDate);
  }

  [Fact]
  public async Task BuildAsync_HeadlessAdapter_ReportsNoMonitorDisplayMode() {
    var builder = Build([VideoRows.Controller(name: "NVIDIA GeForce RTX 4070")], new FakeGpuLoadSource());

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal("No Monitor Active / Headless Display Mode", snapshot.Adapters[0].DisplayMode);
    Assert.Null(snapshot.Adapters[0].VideoRamGB);
  }

  [Fact]
  public async Task BuildAsync_SkipsControllersWithNoName() {
    var builder = Build(
        [VideoRows.Controller(adapterRamBytes: OneGiB),                 // no name → dropped
         VideoRows.Controller(name: "NVIDIA GeForce RTX 4070")],
        new FakeGpuLoadSource());

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Single(snapshot.Adapters);
    Assert.Equal("NVIDIA GeForce RTX 4070", snapshot.Adapters[0].Name);
  }

  [Fact]
  public async Task BuildAsync_ReadsLiveLoadOncePerBuild() {
    var loads = new FakeGpuLoadSource();
    var builder = Build([VideoRows.Controller(name: "NVIDIA GeForce RTX 4070")], loads);

    await builder.BuildAsync(CancellationToken.None);
    await builder.BuildAsync(CancellationToken.None);

    Assert.Equal(2, loads.ReadCount);
  }

  [Fact]
  public async Task BuildAsync_NoAdapters_ReturnsEmptySnapshot() {
    var builder = Build([], new FakeGpuLoadSource());

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Empty(snapshot.Adapters);
  }
}
