using Xunit;

namespace Crystal.Service.Memory.Tests;

public class MemoryInfoBuilderTests {
  private const ulong GB = 1024UL * 1024 * 1024;

  [Fact]
  public async Task BuildAsync_MapsModulesAndRollsUpTotals() {
    var builder = new MemoryInfoBuilder(new FakeWmiHardwareProvider(
        sticks: [
          MemoryRows.Stick(deviceLocator: "DIMM A", capacityBytes: 16 * GB, speed: 5600,
                           configuredSpeed: 4800, formFactor: 8, smbiosType: 34, manufacturer: "Crucial"),
          MemoryRows.Stick(deviceLocator: "DIMM B", capacityBytes: 16 * GB, speed: 5200, formFactor: 8),
        ],
        arrays: [MemoryRows.Array(memoryDevices: 4)]));

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal(2, snapshot.PopulatedSlots);
    Assert.Equal(2, snapshot.Modules.Count);
    Assert.Equal(32.0, snapshot.TotalCapacityGB);
    Assert.Equal(5600u, snapshot.MaxSpeedMHz);   // max advertised speed across sticks
    Assert.Equal(4, snapshot.TotalSlots);
    Assert.Equal("DDR5", snapshot.MemoryType);
    Assert.Equal("DIMM (Desktop)", snapshot.FormFactor);

    var first = snapshot.Modules[0];
    Assert.Equal("DIMM A", first.SlotLabel);
    Assert.Equal(16.0, first.CapacityGB);
    Assert.Equal(5600u, first.SpeedMHz);
    Assert.Equal(4800u, first.ConfiguredSpeedMHz);
    Assert.Equal("Crucial", first.Manufacturer);
  }

  [Fact]
  public async Task BuildAsync_SumsSlotCountAcrossMultipleArrays() {
    var builder = new MemoryInfoBuilder(new FakeWmiHardwareProvider(
        sticks: [MemoryRows.Stick(deviceLocator: "DIMM A", capacityBytes: 8 * GB, formFactor: 8)],
        arrays: [MemoryRows.Array(2), MemoryRows.Array(2)]));

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal(4, snapshot.TotalSlots);
  }

  [Fact]
  public async Task BuildAsync_NoArrayRows_LeavesTotalSlotsNull() {
    var builder = new MemoryInfoBuilder(new FakeWmiHardwareProvider(
        sticks: [MemoryRows.Stick(deviceLocator: "DIMM A", capacityBytes: 8 * GB, formFactor: 8)]));

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Null(snapshot.TotalSlots);
  }

  [Fact]
  public async Task BuildAsync_ZeroReportedSlots_CollapseToNull() {
    var builder = new MemoryInfoBuilder(new FakeWmiHardwareProvider(
        sticks: [MemoryRows.Stick(deviceLocator: "DIMM A", capacityBytes: 8 * GB, formFactor: 8)],
        arrays: [MemoryRows.Array(0)]));

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Null(snapshot.TotalSlots);
  }

  [Fact]
  public async Task BuildAsync_FallsBackToBankLabelWhenDeviceLocatorMissing() {
    var builder = new MemoryInfoBuilder(new FakeWmiHardwareProvider(
        sticks: [MemoryRows.Stick(bankLabel: "BANK 0", capacityBytes: 8 * GB, formFactor: 8)]));

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal("BANK 0", snapshot.Modules[0].SlotLabel);
  }

  [Fact]
  public async Task BuildAsync_UnknownSmbiosType_LeavesMemoryTypeNull() {
    var builder = new MemoryInfoBuilder(new FakeWmiHardwareProvider(
        sticks: [MemoryRows.Stick(deviceLocator: "DIMM A", capacityBytes: 8 * GB, formFactor: 8)]));

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Null(snapshot.MemoryType);
  }

  [Fact]
  public async Task BuildAsync_NoModules_ReturnsEmptyRollups() {
    var builder = new MemoryInfoBuilder(new FakeWmiHardwareProvider());

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Empty(snapshot.Modules);
    Assert.Equal(0, snapshot.PopulatedSlots);
    Assert.Equal(0.0, snapshot.TotalCapacityGB);
    Assert.Null(snapshot.MaxSpeedMHz);
  }
}
