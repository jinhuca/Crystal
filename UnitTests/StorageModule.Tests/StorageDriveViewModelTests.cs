using StorageModule.Models;
using StorageModule.ViewModels;
using Xunit;

namespace StorageModule.Tests;

public class StorageDriveViewModelTests {
  private static StorageDriveInfo Info(int? index = 0, string model = "Samsung 990 Pro",
                                       double? capacityGB = 2000, string? media = "Fixed hard disk media",
                                       string? iface = "SCSI", uint? partitions = 3) =>
      new(Model: model, CapacityGB: capacityGB, InterfaceType: iface, MediaType: media,
          Manufacturer: "Samsung", SerialNumber: "SN123", FirmwareRevision: "1B2QEXM7",
          Partitions: partitions, DriveIndex: index);

  [Fact]
  public void Disk_and_header_labels_use_the_physical_index() {
    var vm = new StorageDriveViewModel(Info(index: 2));

    Assert.Equal("Disk 2", vm.DiskLabel);
    Assert.Equal("Disk 2 (Disk)", vm.HeaderLabel); // "Fixed" collapses to the neutral "Disk" kind
  }

  [Fact]
  public void Missing_index_falls_back_to_model_for_header() {
    var vm = new StorageDriveViewModel(Info(index: null, model: "Generic Drive"));

    Assert.Equal("Disk", vm.DiskLabel);
    Assert.Equal("Generic Drive", vm.HeaderLabel);
  }

  [Theory]
  [InlineData("Removable Media", "Removable", "Disk 0 (Removable)")]
  [InlineData("External hard disk media", "External", "Disk 0 (External)")]
  [InlineData(null, "Fixed", "Disk 0 (Disk)")]
  [InlineData("", "Fixed", "Disk 0 (Disk)")]
  public void Media_type_collapses_to_a_short_kind(string? media, string shortType, string header) {
    var vm = new StorageDriveViewModel(Info(media: media));

    Assert.Equal(shortType, vm.ShortMediaType);
    Assert.Equal(header, vm.HeaderLabel);
  }

  [Fact]
  public void Missing_capacity_and_partitions_show_placeholder() {
    var vm = new StorageDriveViewModel(Info(capacityGB: null, partitions: null));

    Assert.Equal("—", vm.CapacityLabel);
    Assert.Equal("—", vm.PartitionsLabel);
  }

  [Fact]
  public void Blank_identity_fields_show_placeholder() {
    var vm = new StorageDriveViewModel(Info(iface: "   "));

    Assert.Equal("—", vm.InterfaceType);
  }

  [Fact]
  public void Update_formats_live_metric_labels() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(DriveIndex: 0, ActivityPercent: 12.34,
        ReadRateMBps: 210.48, WriteRateMBps: 130.72, ResponseMs: 0.37));

    Assert.Equal("12.3%", vm.ActivityLabel);
    Assert.Equal("210.5 MB/s", vm.ReadSpeedLabel);
    Assert.Equal("130.7 MB/s", vm.WriteSpeedLabel);
    Assert.Equal("0.4 ms", vm.ResponseLabel);
  }

  [Fact]
  public void Null_response_time_shows_placeholder() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 10, 5, 5, ResponseMs: null));

    Assert.Equal("—", vm.ResponseLabel);
  }

  [Fact]
  public void Transfer_axis_tracks_rolling_peak_with_nice_ceiling() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 90, ReadRateMBps: 1200, WriteRateMBps: 700, ResponseMs: null));

    Assert.Equal(2000, vm.TransferMaxMBps); // 1900 -> 2000
  }

  [Fact]
  public void Transfer_axis_floors_at_100_when_idle() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 1, ReadRateMBps: 2, WriteRateMBps: 3, ResponseMs: null));

    Assert.Equal(100, vm.TransferMaxMBps);
  }
}
