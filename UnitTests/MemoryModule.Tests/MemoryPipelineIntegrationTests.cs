using System.Collections.Frozen;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Service.Memory;
using MemoryModule.Models;
using MemoryModule.ViewModels;
using Microsoft.Reactive.Testing;
using Prism.Events;
using Xunit;

namespace MemoryModule.Tests;

// End-to-end tests over the real memory pipeline: a fake WMI provider -> real MemoryInfoBuilder ->
// real MemoryMonitor (driven by a TestScheduler) -> real MemoryModel -> real MemoryViewModel, wired
// the way the module wires them. Unlike MemoryViewModelTests (which pushes pre-built snapshots into
// the VM through a fake model) and MemoryInfoBuilderTests (which stops at the snapshot), these
// exercise the whole service->module seam: the builder mapping WMI rows, the monitor's replay/
// refcount cadence, and the VM's label formatting all run for real. The VM is driven synchronously
// (the test runs on the STA/UI thread, so UiThreadMarshaller executes inline) so no dispatcher
// pumping is needed.
public class MemoryPipelineIntegrationTests {
  private const ulong GB = 1024UL * 1024 * 1024;
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  // MemoryLoadSource whose reading is supplied by a caller-controlled callback, so a test can make
  // successive polls return changing values. Also records Read() calls for cold/cadence assertions.
  private sealed class ScriptedLoadSource(Func<MemoryLoadReading> read) : IMemoryLoadSource {
    public int ReadCount { get; private set; }
    public MemoryLoadReading Read() {
      ReadCount++;
      return read();
    }
  }

  // MemoryInfoBuilder queries two WMI classes via the ToSafe*MetricsAsync extensions, both of which
  // call GetMultiMetricsForClassAsync(className); route by class name so the two result sets are
  // scripted independently. Mirrors the fake in Crystal.Service.Memory.Tests (internal to that
  // assembly, so not visible here).
  private sealed class FakeWmiHardwareProvider(
      IReadOnlyList<FrozenDictionary<string, WmiValue>>? sticks = null,
      IReadOnlyList<FrozenDictionary<string, WmiValue>>? arrays = null) : IWmiHardwareProvider {
    private readonly IReadOnlyList<FrozenDictionary<string, WmiValue>> _sticks = sticks ?? [];
    private readonly IReadOnlyList<FrozenDictionary<string, WmiValue>> _arrays = arrays ?? [];

    public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
        string wmiClassName, CancellationToken cancellationToken, bool bypassCache = false,
        IReadOnlyList<string>? projection = null)
      => Task.FromResult(wmiClassName == "Win32_PhysicalMemoryArray" ? _arrays : _sticks);

    public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
        string namespaceName, string wmiClassName, CancellationToken cancellationToken)
      => Task.FromResult(wmiClassName == "Win32_PhysicalMemoryArray" ? _arrays : _sticks);

    public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> QueryAsync(
        string namespaceName, string wqlQuery, CancellationToken cancellationToken)
      => throw new NotSupportedException();

    public Task<WmiMethodResult> InvokeStaticMethodAsync(
        string namespaceName, string wmiClassName, string methodName,
        IReadOnlyDictionary<string, WmiValue> inParameters, CancellationToken cancellationToken)
      => throw new NotSupportedException();
  }

  // A populated Win32_PhysicalMemory stick. SMBIOSMemoryType 34 = DDR5, FormFactor 8 = DIMM.
  private static FrozenDictionary<string, WmiValue> Stick(
      string deviceLocator, ulong capacityBytes, int speed, int smbiosType = 34, int formFactor = 8) {
    var v = new Dictionary<string, WmiValue> {
      ["DeviceLocator"] = new WmiValue(deviceLocator),
      ["Capacity"] = new WmiValue(capacityBytes),
      ["Speed"] = new WmiValue(speed),
      ["FormFactor"] = new WmiValue(formFactor),
      ["SMBIOSMemoryType"] = new WmiValue(smbiosType),
    };
    return v.ToFrozenDictionary();
  }

  private static FrozenDictionary<string, WmiValue> Array(int memoryDevices) =>
      new Dictionary<string, WmiValue> { ["MemoryDevices"] = new WmiValue(memoryDevices) }
          .ToFrozenDictionary();

  private static FakeWmiHardwareProvider TwoSticks() =>
      new(sticks: [Stick("DIMM A1", 16 * GB, 6000), Stick("DIMM B1", 16 * GB, 5600)],
          arrays: [Array(4)]);

  private static MemoryViewModel CreateVm(MemoryModel model) =>
      new(model, new EventAggregator());

  [Fact]
  public void Specs_flow_from_wmi_through_the_builder_and_model_into_the_view_model() {
    using var monitor = new MemoryMonitor(
        new MemoryInfoBuilder(TwoSticks()), new ScriptedLoadSource(() => new(0, null, null)),
        Interval, new TestScheduler());
    using var model = new MemoryModel(monitor);
    var vm = CreateVm(model);

    // Specs are eager + Replay(1); the VM's subscription (in its ctor) receives the built snapshot.
    Assert.Equal("32 GB DDR5", vm.HeaderSpecLabel);
    Assert.Equal("2 of 4", vm.SlotsUsedLabel);
    Assert.Equal("6000 MT/s", vm.SpeedLabel);
    Assert.Equal(2, vm.Modules.Count);
    Assert.Equal("DIMM A1", vm.Modules[0].SlotLabel);
  }

  [Fact]
  public void Live_load_readings_reach_the_view_model_on_each_poll() {
    int poll = 0;
    var loads = new ScriptedLoadSource(() => {
      poll++;
      return poll == 1
          ? new MemoryLoadReading(LoadPercent: 25, UsedGB: 8, AvailableGB: 24)
          : new MemoryLoadReading(LoadPercent: 75, UsedGB: 24, AvailableGB: 8);
    });
    var scheduler = new TestScheduler();
    using var monitor = new MemoryMonitor(new MemoryInfoBuilder(TwoSticks()), loads, Interval, scheduler);
    using var model = new MemoryModel(monitor);
    var vm = CreateVm(model);

    scheduler.AdvanceBy(Interval.Ticks);   // first poll
    Assert.Equal(25, vm.Load);
    Assert.Equal("8 GB", vm.InUseLabel);

    scheduler.AdvanceBy(Interval.Ticks);   // second poll
    Assert.Equal(75, vm.Load);
    Assert.Equal("24 GB", vm.InUseLabel);
    Assert.Equal("8 GB", vm.AvailableLabel);
  }

  [Fact]
  public void Composition_fraction_uses_the_capacity_built_from_specs() {
    // The used/capacity ratio depends on specs (32 GB) flowing before the load reading applies it.
    var loads = new ScriptedLoadSource(() => new MemoryLoadReading(LoadPercent: 50, UsedGB: 16, AvailableGB: 16));
    var scheduler = new TestScheduler();
    using var monitor = new MemoryMonitor(new MemoryInfoBuilder(TwoSticks()), loads, Interval, scheduler);
    using var model = new MemoryModel(monitor);
    var vm = CreateVm(model);

    scheduler.AdvanceBy(Interval.Ticks);

    Assert.Equal(0.5, vm.CompositionInUseFraction, precision: 3);
  }

  [Fact]
  public void The_load_poll_is_cold_until_the_view_model_subscribes() {
    var loads = new ScriptedLoadSource(() => new MemoryLoadReading(LoadPercent: 40, UsedGB: 12, AvailableGB: 20));
    var scheduler = new TestScheduler();
    using var monitor = new MemoryMonitor(new MemoryInfoBuilder(TwoSticks()), loads, Interval, scheduler);
    using var model = new MemoryModel(monitor);

    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);  // no load subscriber yet -> no polling
    Assert.Equal(0, loads.ReadCount);

    var vm = CreateVm(model);           // the VM subscribes to Load in its ctor
    scheduler.AdvanceBy(Interval.Ticks);

    Assert.True(loads.ReadCount > 0);
    Assert.Equal(40, vm.Load);
  }
}
