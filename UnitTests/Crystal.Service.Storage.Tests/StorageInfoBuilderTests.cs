using Crystal.Provider.Mmi.MmiEngine;
using System.Collections.Frozen;
using Xunit;

namespace Crystal.Service.Storage.Tests;

public class StorageInfoBuilderTests {
  private const ulong GB = 1024UL * 1024 * 1024;

  private static StorageInfoBuilder Build(params FrozenDictionary<string, WmiValue>[] rows) =>
      new(new FakeWmiHardwareProvider(rows));

  [Fact]
  public async Task BuildAsync_MapsEachDriveAndRollsUpTotals() {
    var builder = Build(
        DiskRow.Drive(model: "Samsung SSD 980", sizeBytes: 256 * GB, index: 0,
                      interfaceType: "SCSI", mediaType: "Fixed hard disk media", partitions: 3),
        DiskRow.Drive(model: "WD Blue", sizeBytes: 1024 * GB, index: 1));

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal(2, snapshot.DriveCount);
    Assert.Equal(2, snapshot.Drives.Count);
    Assert.Equal(256.0 + 1024.0, snapshot.TotalCapacityGB);

    var first = snapshot.Drives[0];
    Assert.Equal("Samsung SSD 980", first.Model);
    Assert.Equal(256.0, first.CapacityGB);
    Assert.Equal("SCSI", first.InterfaceType);
    Assert.Equal("Fixed hard disk media", first.MediaType);
    Assert.Equal(3u, first.Partitions);
    Assert.Equal(0, first.DriveIndex);
  }

  [Fact]
  public async Task BuildAsync_RoundsCapacityToOneDecimal() {
    // 500 GB advertised (decimal) drive reports 500,107,862,016 bytes → 465.76… GiB → 465.8.
    var builder = Build(DiskRow.Drive(model: "Drive", sizeBytes: 500_107_862_016UL));

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal(465.8, snapshot.Drives[0].CapacityGB);
  }

  [Fact]
  public async Task BuildAsync_NullSize_LeavesCapacityNullAndCountsAsZeroInTotal() {
    var builder = Build(DiskRow.Drive(model: "Unsized drive"));

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Null(snapshot.Drives[0].CapacityGB);
    Assert.Equal(0.0, snapshot.TotalCapacityGB);
  }

  [Fact]
  public async Task BuildAsync_FallsBackToCaptionWhenModelMissing() {
    var builder = Build(DiskRow.Drive(caption: "Generic USB Device", sizeBytes: 8 * GB));

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Equal("Generic USB Device", snapshot.Drives[0].Model);
  }

  [Fact]
  public async Task BuildAsync_SkipsRowsWithNeitherModelNorCaption() {
    var builder = Build(
        DiskRow.Drive(sizeBytes: 8 * GB),                 // no name → dropped
        DiskRow.Drive(model: "Real drive", sizeBytes: 8 * GB));

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Single(snapshot.Drives);
    Assert.Equal("Real drive", snapshot.Drives[0].Model);
  }

  [Fact]
  public async Task BuildAsync_TrimsWhitespaceOnIdentifiers() {
    var builder = Build(DiskRow.Drive(
        model: "Drive", sizeBytes: 8 * GB,
        manufacturer: "  Seagate  ", serial: "  SN123  ", firmware: "  FW01  "));

    var d = (await builder.BuildAsync(CancellationToken.None)).Drives[0];

    Assert.Equal("Seagate", d.Manufacturer);
    Assert.Equal("SN123", d.SerialNumber);
    Assert.Equal("FW01", d.FirmwareRevision);
  }

  [Fact]
  public async Task BuildAsync_NoDrives_ReturnsEmptySnapshot() {
    var builder = Build();

    var snapshot = await builder.BuildAsync(CancellationToken.None);

    Assert.Empty(snapshot.Drives);
    Assert.Equal(0, snapshot.DriveCount);
    Assert.Equal(0.0, snapshot.TotalCapacityGB);
  }
}
