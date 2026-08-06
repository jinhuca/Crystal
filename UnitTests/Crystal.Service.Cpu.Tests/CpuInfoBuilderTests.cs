using Crystal.Infrastructure.DataStructures.Cpu.Implementations.Cpus;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;
using Crystal.Provider.CpuId;
using Crystal.Provider.Smbios.HardwareFeatures.Processor;
using Xunit;

namespace Crystal.Service.Cpu.Tests;

public class CpuInfoBuilderTests {
  private static readonly CpuIdRawData Cpuid =
    new("Test CPU", "TestVendor", 6, 1, 2, 3600, 100, 8, 16, true, true, null, null);

  private static SmbiosProcessorInfo Socket(string designation, int logical = 8) =>
    new(designation, MaxSpeedMHz: 4200, ExternalClockMHz: 100, LogicalCoreCount: logical, CacheInfo: null);

  private static CpuInfoBuilder Build(
      IReadOnlyList<SmbiosProcessorInfo> smbios,
      IReadOnlyList<System.Collections.Frozen.FrozenDictionary<string, Crystal.Provider.Mmi.MmiEngine.WmiValue>> wmi,
      ICpuTelemetrySource? telemetry = null) =>
    new(new FakeCpuIdProvider(Cpuid),
        new FakeSmbiosProcessorProvider(smbios),
        new FakeWmiHardwareProvider(wmi),
        new CpuSpecsResolver(),
        telemetry);

  [Fact]
  public async Task BuildAsync_OneSocketPerSmbiosProcessor() {
    var builder = Build(
      [Socket("CPU0"), Socket("CPU1")],
      [FakeWmiHardwareProvider.ProcessorRow("CPU0", 32, 16),
       FakeWmiHardwareProvider.ProcessorRow("CPU1", 32, 16)]);

    var info = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal(2, info.Sockets.Count);
    Assert.Equal(0, info.Sockets[0].SocketIndex);
    Assert.Equal(1, info.Sockets[1].SocketIndex);
    Assert.Equal("CPU0", info.Sockets[0].SocketDesignation);
    Assert.Equal("CPU1", info.Sockets[1].SocketDesignation);
  }

  [Fact]
  public async Task BuildAsync_CorrelatesWmiBySocketDesignationNotListOrder() {
    // SMBIOS lists CPU0 then CPU1; WMI enumerates them in the reverse order. The builder must
    // match on SocketDesignation, so each socket still gets its own WMI core counts.
    var builder = Build(
      [Socket("CPU0"), Socket("CPU1")],
      [FakeWmiHardwareProvider.ProcessorRow("CPU1", 64, 32),
       FakeWmiHardwareProvider.ProcessorRow("CPU0", 16, 8)]);

    var info = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal(8, info.Sockets[0].Specs.PhysicalCoreNum);   // CPU0 → WMI cores 8
    Assert.Equal(32, info.Sockets[1].Specs.PhysicalCoreNum);  // CPU1 → WMI cores 32
  }

  [Fact]
  public async Task BuildAsync_NoWmiMatch_FallsBackToCpuidCoreCounts() {
    // No WMI row matches "CPU0", so the resolver falls back to CPUID's counts (physical 8 / logical 16).
    var builder = Build(
      [Socket("CPU0")],
      [FakeWmiHardwareProvider.ProcessorRow("SOCKET-X", 4, 2)]);

    var info = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal(8, info.Sockets[0].Specs.PhysicalCoreNum);
    Assert.Equal(16, info.Sockets[0].Specs.LogicalCoreNum);
  }

  [Fact]
  public async Task BuildAsync_NullTelemetry_UsesEmptySensorsAndCores() {
    var builder = Build([Socket("CPU0")], [FakeWmiHardwareProvider.ProcessorRow("CPU0", 32, 16)], telemetry: null);

    var info = await builder.BuildAsync(CancellationToken.None);

    Assert.NotNull(info.Sockets[0].Sensors);
    Assert.IsType<CpuSensors>(info.Sockets[0].Sensors);
    Assert.Empty(info.Sockets[0].Cores);
  }

  [Fact]
  public async Task BuildAsync_TelemetryPresent_RefreshesThenCorrelatesByIndex() {
    var sensors0 = new CpuSensors();
    var telemetry = new FakeCpuTelemetrySource(
      sensors: new() { [0] = sensors0 });

    var builder = Build([Socket("CPU0"), Socket("CPU1")],
      [FakeWmiHardwareProvider.ProcessorRow("CPU0", 32, 16),
       FakeWmiHardwareProvider.ProcessorRow("CPU1", 32, 16)],
      telemetry);

    var info = await builder.BuildAsync(CancellationToken.None);

    Assert.True(telemetry.Refreshed);
    Assert.Same(sensors0, info.Sockets[0].Sensors);
    // Socket index 1 had no telemetry entry → empty CpuSensors fallback, not the socket-0 instance.
    Assert.NotSame(sensors0, info.Sockets[1].Sensors);
    Assert.Equal([0, 1], telemetry.RequestedSensorIndices);
  }

  [Fact]
  public async Task BuildAsync_NoProcessors_ReturnsEmptySocketList() {
    var builder = Build([], []);

    var info = await builder.BuildAsync(CancellationToken.None);

    Assert.Empty(info.Sockets);
  }
}
