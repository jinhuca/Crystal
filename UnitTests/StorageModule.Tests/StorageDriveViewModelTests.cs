using Crystal.Service.Storage;
using StorageModule.ViewModels;
using Xunit;

namespace StorageModule.Tests;

public class StorageDriveViewModelTests {
  private static StorageDriveInfo Info(int? index = 0, string model = "Samsung 990 Pro",
                                       double? capacityGB = 2000, string? media = "Fixed hard disk media",
                                       string? iface = "SCSI", uint? partitions = 3,
                                       string? manufacturer = "Samsung", string? serial = "SN123") =>
      new(Model: model, CapacityGB: capacityGB, InterfaceType: iface, MediaType: media,
          Manufacturer: manufacturer, SerialNumber: serial, FirmwareRevision: "1B2QEXM7",
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
  public void Manufacturer_and_serial_are_surfaced_from_the_drive_info() {
    var vm = new StorageDriveViewModel(Info(manufacturer: "Western Digital", serial: "WD-9XYZ"));

    Assert.Equal("Western Digital", vm.Manufacturer);
    Assert.Equal("WD-9XYZ", vm.SerialNumber);
  }

  [Fact]
  public void Blank_manufacturer_and_serial_show_placeholder() {
    var vm = new StorageDriveViewModel(Info(manufacturer: null, serial: "   "));

    Assert.Equal("—", vm.Manufacturer);
    Assert.Equal("—", vm.SerialNumber);
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
  public void Update_formats_temperature_and_health_labels() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 10, 5, 5, ResponseMs: null,
        TemperatureC: 41.6, HealthPercent: 98));

    Assert.Equal("41.6 °C", vm.TemperatureLabel);
    Assert.Equal("98%", vm.HealthLabel);
  }

  [Fact]
  public void Missing_temperature_and_health_show_placeholder() {
    var vm = new StorageDriveViewModel(Info());

    // SMART sensors are absent without elevation/PawnIO or on drives that don't report them.
    vm.Update(new StorageDiskLoad(0, 10, 5, 5, ResponseMs: null));

    Assert.Equal("—", vm.TemperatureLabel);
    Assert.Equal("—", vm.HealthLabel);
  }

  [Fact]
  public void Capacity_bar_splits_used_and_free_from_total_minus_free() {
    var vm = new StorageDriveViewModel(Info());

    // 931 total, 466 free -> 465 used, ~50%.
    vm.Update(new StorageDiskLoad(0, 10, 5, 5, ResponseMs: null,
        UsedSpacePercent: 49.9, FreeSpaceGB: 466, TotalSpaceGB: 931));

    Assert.Equal(0.5, vm.UsedSpaceFraction, precision: 2);
    Assert.Equal(0.5, vm.FreeSpaceFraction, precision: 2);
    Assert.Equal("465 / 931 GB", vm.CapacityUsageLabel);
    Assert.Equal("465 GB", vm.UsedSpaceLabel);
    Assert.Equal("466 GB", vm.FreeSpaceLabel);
    Assert.Equal("50%", vm.UsedSpacePercentLabel);
  }

  [Fact]
  public void Capacity_bar_falls_back_to_used_percent_without_totals() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 10, 5, 5, ResponseMs: null, UsedSpacePercent: 30));

    Assert.Equal(0.30, vm.UsedSpaceFraction, precision: 2);
    Assert.Equal("—", vm.CapacityUsageLabel); // no GB figures to show
  }

  [Fact]
  public void Capacity_bar_is_empty_and_placeholder_without_space_data() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 10, 5, 5, ResponseMs: null));

    Assert.Equal(0, vm.UsedSpaceFraction);
    Assert.Equal(1, vm.FreeSpaceFraction);
    Assert.Equal("—", vm.CapacityUsageLabel);
    Assert.Equal("—", vm.UsedSpacePercentLabel);
  }

  [Fact]
  public void Update_formats_read_and_write_activity_labels() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, ActivityPercent: 24, ReadRateMBps: 5, WriteRateMBps: 5,
        ResponseMs: null, ReadActivityPercent: 18.2, WriteActivityPercent: 6.4));

    Assert.Equal("18.2%", vm.ReadActivityLabel);
    Assert.Equal("6.4%", vm.WriteActivityLabel);
  }

  [Fact]
  public void Read_and_write_activity_default_to_zero_when_absent() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 10, 5, 5, ResponseMs: null));

    Assert.Equal("0.0%", vm.ReadActivityLabel);
    Assert.Equal("0.0%", vm.WriteActivityLabel);
  }

  [Fact]
  public void Endurance_formats_data_in_tb_above_a_terabyte_and_gb_below() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 10, 5, 5, ResponseMs: null,
        DataReadGB: 512, DataWrittenGB: 145408, PowerOnHours: 4210, PowerOnCount: 1832));

    Assert.Equal("142.0 TB", vm.DataWrittenLabel); // 145408 GB / 1024
    Assert.Equal("512 GB", vm.DataReadLabel);
    Assert.Equal("4210 h", vm.PowerOnHoursLabel);
    Assert.Equal("1832", vm.PowerOnCountLabel);
  }

  [Fact]
  public void Missing_endurance_sensors_show_placeholder() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 10, 5, 5, ResponseMs: null));

    Assert.Equal("—", vm.DataWrittenLabel);
    Assert.Equal("—", vm.DataReadLabel);
    Assert.Equal("—", vm.PowerOnHoursLabel);
    Assert.Equal("—", vm.PowerOnCountLabel);
  }

  [Fact]
  public void Transfer_axis_tracks_rolling_peak_with_nice_ceiling() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 90, ReadRateMBps: 1200, WriteRateMBps: 700, ResponseMs: null));

    Assert.Equal(2000, vm.TransferMaxMBps); // 1900 -> 2000
  }

  [Fact]
  public void Peak_transfer_holds_the_session_maximum_and_scales_to_gbps() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 90, ReadRateMBps: 1200, WriteRateMBps: 700, ResponseMs: null));
    Assert.Equal("1.9 GB/s", vm.PeakTransferLabel); // 1900 MB/s -> GB/s

    // A slower later sample must not lower the retained peak.
    vm.Update(new StorageDiskLoad(0, 10, ReadRateMBps: 50, WriteRateMBps: 20, ResponseMs: null));
    Assert.Equal("1.9 GB/s", vm.PeakTransferLabel);
  }

  [Fact]
  public void Peak_transfer_shows_mbps_below_a_gigabyte() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 40, ReadRateMBps: 300, WriteRateMBps: 120, ResponseMs: null));

    Assert.Equal("420.0 MB/s", vm.PeakTransferLabel);
  }

  [Fact]
  public void Transfer_axis_tracks_the_taller_trace_not_the_read_plus_write_sum() {
    var vm = new StorageDriveViewModel(Info());

    // Read and write are plotted as two independent lines, so the axis follows the taller single
    // trace (600 -> 1000), not their sum (1200, which would round up to 2000).
    vm.Update(new StorageDiskLoad(0, 80, ReadRateMBps: 600, WriteRateMBps: 600, ResponseMs: null));

    Assert.Equal(1000, vm.TransferMaxMBps);
  }

  [Fact]
  public void Transfer_axis_floors_at_100_when_idle() {
    var vm = new StorageDriveViewModel(Info());

    vm.Update(new StorageDiskLoad(0, 1, ReadRateMBps: 2, WriteRateMBps: 3, ResponseMs: null));

    Assert.Equal(100, vm.TransferMaxMBps);
  }
}
