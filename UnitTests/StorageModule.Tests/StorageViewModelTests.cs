using System.Reactive.Subjects;
using Prism.Events;
using StorageModule.Models;
using StorageModule.ViewModels;
using Xunit;

namespace StorageModule.Tests;

public class StorageViewModelTests {
  private sealed class FakeStorageModel : IStorageModel {
    public Subject<StorageSnapshot> SpecsSubject { get; } = new();
    public Subject<StorageLoadReading> LoadSubject { get; } = new();
    public IObservable<StorageSnapshot> Specs => SpecsSubject;
    public IObservable<StorageLoadReading> Load => LoadSubject;
  }

  private static StorageViewModel CreateVm(out FakeStorageModel model) {
    model = new FakeStorageModel();
    return new StorageViewModel(model, new EventAggregator());
  }

  private static StorageDriveInfo Drive(int index, string model = "Samsung 990 Pro",
                                        double? capacityGB = 2000, string? media = "Fixed hard disk media") =>
      new(Model: model, CapacityGB: capacityGB, InterfaceType: "SCSI", MediaType: media,
          Manufacturer: "Samsung", SerialNumber: "SN123", FirmwareRevision: "1B2QEXM7",
          Partitions: 3, DriveIndex: index);

  private static StorageSnapshot Snapshot(params StorageDriveInfo[] drives) =>
      new(drives, TotalCapacityGB: drives.Sum(d => d.CapacityGB ?? 0), DriveCount: drives.Length);

  private static StorageDiskLoad DiskLoad(int index, double activity, double read, double write,
                                          double? responseMs = null) =>
      new(index, activity, read, write, responseMs);

  [Fact]
  public void Specs_populate_drives_and_totals() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Snapshot(Drive(0), Drive(1, "WD Black", 1000)));

    Assert.Equal(2, vm.Drives.Count);
    Assert.Equal("3000 GB", vm.TotalCapacityLabel);
    Assert.Equal("2 drives", vm.DriveCountLabel);
  }

  [Fact]
  public void Single_drive_count_is_singular() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Snapshot(Drive(0)));

    Assert.Equal("1 drive", vm.DriveCountLabel);
  }

  [Fact]
  public void First_disk_is_selected_by_default() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Snapshot(Drive(0), Drive(1)));

    Assert.NotNull(vm.SelectedDisk);
    Assert.Equal(0, vm.SelectedDisk!.DriveIndex);
  }

  [Fact]
  public void Selection_is_preserved_across_spec_refreshes() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(Snapshot(Drive(0), Drive(1)));
    vm.SelectedDisk = vm.Drives[1];
    // A later inventory refresh must not reset the selection back to disk 0 (the ??= default only
    // applies when nothing is selected yet).
    model.SpecsSubject.OnNext(Snapshot(Drive(0), Drive(1)));

    Assert.Equal(1, vm.SelectedDisk!.DriveIndex);
  }

  [Fact]
  public void Load_routes_each_reading_to_the_matching_disk_by_index() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Snapshot(Drive(0), Drive(1)));

    model.LoadSubject.OnNext(new StorageLoadReading([
        DiskLoad(0, activity: 12, read: 200, write: 100),
        DiskLoad(1, activity: 40, read: 10, write: 5),
    ]));

    var disk0 = vm.Drives.Single(d => d.DriveIndex == 0);
    var disk1 = vm.Drives.Single(d => d.DriveIndex == 1);
    Assert.Equal("12.0%", disk0.ActivityLabel);
    Assert.Equal("200.0 MB/s", disk0.ReadSpeedLabel);
    Assert.Equal("40.0%", disk1.ActivityLabel);
  }

  [Fact]
  public void Aggregate_load_is_busiest_activity_and_summed_transfer() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Snapshot(Drive(0), Drive(1)));

    model.LoadSubject.OnNext(new StorageLoadReading([
        DiskLoad(0, activity: 12, read: 200, write: 100),
        DiskLoad(1, activity: 40, read: 10, write: 5),
    ]));

    Assert.Equal(40, vm.Load);            // busiest disk
    Assert.Equal(315, vm.TransferRateMBps); // 200+100 + 10+5
  }

  [Fact]
  public void Load_for_unknown_index_is_ignored_without_throwing() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Snapshot(Drive(0)));

    // Disk 7 has no matching inventory entry; it must still count toward the aggregate.
    model.LoadSubject.OnNext(new StorageLoadReading([DiskLoad(7, activity: 55, read: 50, write: 50)]));

    Assert.Equal(55, vm.Load);
    Assert.Equal(100, vm.TransferRateMBps);
  }

  [Fact]
  public void Transfer_axis_floors_at_100_when_idle() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Snapshot(Drive(0)));

    model.LoadSubject.OnNext(new StorageLoadReading([DiskLoad(0, activity: 1, read: 2, write: 3)]));

    Assert.Equal(100, vm.TransferMaxMBps);
  }

  [Fact]
  public void Transfer_axis_rounds_peak_up_to_a_nice_ceiling() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(Snapshot(Drive(0)));

    // 1200 + 700 = 1900 MB/s -> nice ceiling 2000.
    model.LoadSubject.OnNext(new StorageLoadReading([DiskLoad(0, activity: 90, read: 1200, write: 700)]));

    Assert.Equal(2000, vm.TransferMaxMBps);
  }
}
