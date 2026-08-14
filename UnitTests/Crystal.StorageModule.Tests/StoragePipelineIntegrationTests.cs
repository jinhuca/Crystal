using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Service.Storage;
using Crystal.StorageModule.Models;
using Crystal.StorageModule.ViewModels;
using Microsoft.Reactive.Testing;
using Prism.Events;
using System.Collections.Frozen;
using Xunit;

namespace Crystal.StorageModule.Tests;

// End-to-end tests over the real storage pipeline: a fake WMI provider -> real StorageInfoBuilder ->
// real StorageMonitor (driven by a TestScheduler) -> real StorageModel -> real StorageViewModel,
// wired the way the module wires them. Unlike StorageViewModelTests (which pushes pre-built snapshots
// into the VM through a fake model) and StorageInfoBuilderTests (which stops at the snapshot), these
// exercise the whole service->module seam: the builder mapping WMI rows, the monitor's replay/
// refcount cadence, and the VM's per-disk load join (by physical-disk index) all run for real. The
// VM is driven synchronously (the test runs on the STA/UI thread, so UiThreadMarshaller executes
// inline) so no dispatcher pumping is needed.
public class StoragePipelineIntegrationTests {
  private const ulong GB = 1024UL * 1024 * 1024;
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  // A load source whose per-poll reading is supplied by a caller-controlled callback, so a test can
  // make successive polls return changing values. Records Read() calls for cold/cadence assertions.
  private sealed class ScriptedLoadSource(Func<StorageLoadReading> read) : IStorageLoadSource {
    public int ReadCount { get; private set; }
    public StorageLoadReading Read() {
      ReadCount++;
      return read();
    }
  }

  // StorageInfoBuilder calls ToSafeDiskDriveMetricsAsync, which invokes
  // GetMultiMetricsForClassAsync(Win32_DiskDrive); the fake speaks that raw property-bag contract.
  // Mirrors the fake in Crystal.Service.Storage.Tests (internal to that assembly, so not visible
  // here).
  private sealed class FakeWmiHardwareProvider(IReadOnlyList<FrozenDictionary<string, WmiValue>> instances)
      : IWmiHardwareProvider {
    public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
        string wmiClassName, CancellationToken cancellationToken, bool bypassCache = false,
        IReadOnlyList<string>? projection = null)
      => Task.FromResult(instances);

    public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
        string namespaceName, string wmiClassName, CancellationToken cancellationToken)
      => Task.FromResult(instances);

    public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> QueryAsync(
        string namespaceName, string wqlQuery, CancellationToken cancellationToken)
      => throw new NotSupportedException();

    public Task<WmiMethodResult> InvokeStaticMethodAsync(
        string namespaceName, string wmiClassName, string methodName,
        IReadOnlyDictionary<string, WmiValue> inParameters, CancellationToken cancellationToken)
      => throw new NotSupportedException();
  }

  private static FrozenDictionary<string, WmiValue> Drive(
      string model, ulong sizeBytes, int index, string interfaceType = "NVMe",
      string mediaType = "Fixed hard disk media") =>
      new Dictionary<string, WmiValue> {
        ["Model"] = new WmiValue(model),
        ["Size"] = new WmiValue(sizeBytes),
        ["Index"] = new WmiValue(index),
        ["InterfaceType"] = new WmiValue(interfaceType),
        ["MediaType"] = new WmiValue(mediaType),
      }.ToFrozenDictionary();

  private static FakeWmiHardwareProvider TwoDrives() =>
      new([Drive("Samsung 990 Pro", 1024 * GB, 0), Drive("WD Blue", 2048 * GB, 1)]);

  private static StorageViewModel CreateVm(StorageModel model) => new(model, new EventAggregator());

  [Fact]
  public void Specs_flow_from_wmi_through_the_builder_and_model_into_the_view_model() {
    using var monitor = new StorageMonitor(
        new StorageInfoBuilder(TwoDrives()), new ScriptedLoadSource(() => new StorageLoadReading([])),
        Interval, new TestScheduler());
    var model = new StorageModel(monitor);
    var vm = CreateVm(model);

    // Specs are eager + Replay(1); the VM's subscription (in its ctor) receives the built snapshot.
    Assert.Equal("2 drives", vm.DriveCountLabel);
    Assert.Equal(2, vm.Drives.Count);
    Assert.Equal("Samsung 990 Pro", vm.Drives[0].Model);
    // 1024 GiB + 2048 GiB = 3072 GB total.
    Assert.Equal("3072 GB", vm.TotalCapacityLabel);
    // First drive auto-selected for the detail view.
    Assert.Same(vm.Drives[0], vm.SelectedDisk);
  }

  [Fact]
  public void Live_disk_loads_join_to_the_matching_drive_by_physical_index() {
    var loads = new ScriptedLoadSource(() => new StorageLoadReading([
      new StorageDiskLoad(DriveIndex: 0, ActivityPercent: 30, ReadRateMBps: 100, WriteRateMBps: 50, ResponseMs: 0.4),
      new StorageDiskLoad(DriveIndex: 1, ActivityPercent: 80, ReadRateMBps: 20, WriteRateMBps: 10, ResponseMs: 5.0),
    ]));
    var scheduler = new TestScheduler();
    using var monitor = new StorageMonitor(new StorageInfoBuilder(TwoDrives()), loads, Interval, scheduler);
    var model = new StorageModel(monitor);
    var vm = CreateVm(model);

    scheduler.AdvanceBy(Interval.Ticks);

    // Each disk's sample routed to its matching per-disk VM by DriveIndex.
    Assert.Equal(30, vm.Drives[0].ActivityPercent);
    Assert.Equal("100.0 MB/s", vm.Drives[0].ReadSpeedLabel);
    Assert.Equal(80, vm.Drives[1].ActivityPercent);

    // Tile aggregates: busiest disk's activity + system-wide transfer rate (100+50+20+10 = 180).
    Assert.Equal(80, vm.Load);
    Assert.Equal(180, vm.TransferRateMBps);
  }

  [Fact]
  public void Successive_polls_update_the_per_disk_view_models_in_place() {
    int poll = 0;
    var loads = new ScriptedLoadSource(() => {
      poll++;
      double activity = poll == 1 ? 10 : 90;
      return new StorageLoadReading([
        new StorageDiskLoad(DriveIndex: 0, ActivityPercent: activity, ReadRateMBps: activity, WriteRateMBps: 0, ResponseMs: null),
      ]);
    });
    var scheduler = new TestScheduler();
    using var monitor = new StorageMonitor(new StorageInfoBuilder(TwoDrives()), loads, Interval, scheduler);
    var model = new StorageModel(monitor);
    var vm = CreateVm(model);

    scheduler.AdvanceBy(Interval.Ticks);
    var disk0 = vm.Drives[0];
    Assert.Equal(10, disk0.ActivityPercent);

    scheduler.AdvanceBy(Interval.Ticks);
    // Same drive VM instance, updated values (drives aren't rebuilt per poll).
    Assert.Same(disk0, vm.Drives[0]);
    Assert.Equal(90, disk0.ActivityPercent);
  }

  [Fact]
  public void The_load_poll_is_cold_until_the_view_model_subscribes() {
    var loads = new ScriptedLoadSource(() => new StorageLoadReading([
      new StorageDiskLoad(DriveIndex: 0, ActivityPercent: 25, ReadRateMBps: 5, WriteRateMBps: 5, ResponseMs: null),
    ]));
    var scheduler = new TestScheduler();
    using var monitor = new StorageMonitor(new StorageInfoBuilder(TwoDrives()), loads, Interval, scheduler);
    var model = new StorageModel(monitor);

    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);   // no load subscriber yet -> no polling
    Assert.Equal(0, loads.ReadCount);

    var vm = CreateVm(model);           // the VM subscribes to Load in its ctor
    scheduler.AdvanceBy(Interval.Ticks);

    Assert.True(loads.ReadCount > 0);
    Assert.Equal(25, vm.Load);
  }
}
