using System.Collections.Frozen;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Service.Gpu;
using Crystal.GpuModule.Models;
using Crystal.GpuModule.ViewModels;
using Microsoft.Reactive.Testing;
using Prism.Events;
using Xunit;

namespace Crystal.GpuModule.Tests;

// End-to-end tests over the real GPU pipeline: a fake WMI provider + a fake load source -> real
// GpuInfoBuilder -> real GpuMonitor (driven by a TestScheduler) -> real GpuModel -> real
// GpuViewModel, wired the way the module wires them. Unlike GpuViewModelTests (which pushes
// pre-built snapshots into the VM through a fake model), these exercise the whole service->module
// seam: the builder mapping Win32_VideoController rows and classifying integrated vs dedicated, the
// monitor's replay/refcount cadence, and the VM's per-adapter load join all run for real. The VM is
// driven synchronously (the test runs on the STA/UI thread, so UiThreadMarshaller executes inline)
// so no dispatcher pumping is needed.
public class GpuPipelineIntegrationTests {
  private const int GiB = 1024 * 1024 * 1024;
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

  // A load source whose per-poll readings are supplied by a caller-controlled callback, so a test
  // can make successive polls return changing values. Records Read() calls for cold/cadence
  // assertions. Mirrors GpuLoadSource (which opens a LibreHardwareMonitor Computer, so not usable
  // here).
  private sealed class ScriptedLoadSource(Func<IReadOnlyList<GpuLoadReading>> read) : IGpuLoadSource {
    public int ReadCount { get; private set; }
    public IReadOnlyList<GpuLoadReading> Read() {
      ReadCount++;
      return read();
    }
  }

  // GpuInfoBuilder calls ToSafeVideoControllerMetricsAsync, which invokes
  // GetMultiMetricsForClassAsync(Win32_VideoController); the fake speaks that raw property-bag
  // contract. Keys match Win32_VideoController property names.
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

  // A populated Win32_VideoController row. PNPDeviceID is left unset so the builder's registry
  // driver-date/location probes short-circuit to null (no HKLM access from a unit test).
  private static FrozenDictionary<string, WmiValue> Controller(
      string name, int ramBytes, int hRes = 1920, int vRes = 1080, int refreshHz = 144,
      int bitsPerPixel = 32, string driverVersion = "31.0.15", string videoProcessor = "GPU Chip") =>
      new Dictionary<string, WmiValue> {
        ["Name"] = new WmiValue(name),
        ["AdapterRAM"] = new WmiValue(ramBytes),
        ["CurrentHorizontalResolution"] = new WmiValue(hRes),
        ["CurrentVerticalResolution"] = new WmiValue(vRes),
        ["CurrentRefreshRate"] = new WmiValue(refreshHz),
        ["CurrentBitsPerPixel"] = new WmiValue(bitsPerPixel),
        ["DriverVersion"] = new WmiValue(driverVersion),
        ["VideoProcessor"] = new WmiValue(videoProcessor),
      }.ToFrozenDictionary();

  private static GpuLoadReading Load(string name, double core = 0, double? temp = null,
                                     double? clock = null, double? power = null) =>
      new(AdapterName: name, CoreLoadPercent: core, TemperatureC: temp, ClockMhz: clock, PowerW: power);

  // A dedicated NVIDIA card plus an Intel integrated GPU (matches the "UHD Graphics" marker).
  // Win32_VideoController.AdapterRAM is a uint32 the pipeline carries as a signed int, so RAM values
  // stay under 2 GB (the classic AdapterRAM cap means this field is unreliable for big cards anyway).
  private static FakeWmiHardwareProvider MixedAdapters() =>
      new([Controller("NVIDIA GeForce RTX 4070", GiB), Controller("Intel(R) UHD Graphics 770", GiB / 2)]);

  private static GpuViewModel CreateVm(GpuModel model) => new(model, new EventAggregator());

  [Fact]
  public void Specs_flow_from_wmi_through_the_builder_and_model_into_the_view_model() {
    using var monitor = new GpuMonitor(
        new GpuInfoBuilder(MixedAdapters(), new ScriptedLoadSource(() => [])),
        Interval, new TestScheduler());
    using var model = new GpuModel(monitor);
    var vm = CreateVm(model);

    // Specs are eager + Replay(1); the VM's subscription (in its ctor) receives the built snapshot.
    // The VM orders integrated first so it lands in the left column.
    Assert.Equal(2, vm.Adapters.Count);
    Assert.Equal("Intel(R) UHD Graphics 770", vm.Adapters[0].Name);
    Assert.Equal("Integrated GPU", vm.Adapters[0].KindLabel);
    Assert.Equal("NVIDIA GeForce RTX 4070", vm.Adapters[1].Name);
    Assert.Equal("Dedicated GPU", vm.Adapters[1].KindLabel);

    // WMI AdapterRAM (bytes) -> GB, and the composed display-mode string.
    Assert.Equal(1, vm.Adapters[1].VideoRamGB);
    Assert.Equal("1920 x 1080 @ 144Hz (32-bit color)", vm.Adapters[1].DisplayMode);
    Assert.Equal("31.0.15", vm.Adapters[1].DriverVersion);
  }

  [Theory]
  [InlineData("Intel(R) UHD Graphics 770", "Integrated GPU")]
  [InlineData("Intel(R) Iris(R) Xe Graphics", "Integrated GPU")]
  [InlineData("AMD Radeon(TM) Graphics", "Integrated GPU")]
  [InlineData("AMD Radeon RX 7900 XTX", "Dedicated GPU")]
  [InlineData("NVIDIA GeForce RTX 4070", "Dedicated GPU")]
  public void The_integrated_marker_heuristic_classifies_each_adapter(string name, string expectedKind) {
    using var monitor = new GpuMonitor(
        new GpuInfoBuilder(new FakeWmiHardwareProvider([Controller(name, GiB)]),
                           new ScriptedLoadSource(() => [])),
        Interval, new TestScheduler());
    using var model = new GpuModel(monitor);
    var vm = CreateVm(model);

    Assert.Equal(expectedKind, Assert.Single(vm.Adapters).KindLabel);
  }

  [Fact]
  public void Live_load_readings_join_to_the_matching_adapter_by_name_across_polls() {
    int poll = 0;
    var loads = new ScriptedLoadSource(() => {
      poll++;
      double core = poll <= 1 ? 20 : 88;   // the eager specs build consumes poll #1
      return [Load("NVIDIA GeForce RTX 4070", core: core, temp: 65, clock: 2600, power: 180)];
    });
    var scheduler = new TestScheduler();
    using var monitor = new GpuMonitor(new GpuInfoBuilder(MixedAdapters(), loads), Interval, scheduler);
    using var model = new GpuModel(monitor);
    var vm = CreateVm(model);

    scheduler.AdvanceBy(Interval.Ticks);
    var dedicated = vm.Adapters.Single(a => a.Name == "NVIDIA GeForce RTX 4070");
    Assert.Equal(88, dedicated.Load);
    Assert.Equal(65, dedicated.TemperatureC);
    Assert.Equal(2600, dedicated.ClockMhz);
    Assert.Equal(180, dedicated.PowerW);

    // The integrated adapter had no matching load reading, so its load stays at the default.
    Assert.Equal(0, vm.Adapters.Single(a => a.Name == "Intel(R) UHD Graphics 770").Load);
  }

  [Fact]
  public void The_sensor_poll_is_cold_until_the_view_model_subscribes() {
    var loads = new ScriptedLoadSource(() => [Load("NVIDIA GeForce RTX 4070", core: 42)]);
    var scheduler = new TestScheduler();
    using var monitor = new GpuMonitor(new GpuInfoBuilder(MixedAdapters(), loads), Interval, scheduler);
    using var model = new GpuModel(monitor);

    // The eager specs build already ran BuildAsync once (which reads loads once). With no Sensors
    // subscriber, advancing time must not poll again.
    int readsAfterSpecs = loads.ReadCount;
    scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
    Assert.Equal(readsAfterSpecs, loads.ReadCount);

    var vm = CreateVm(model);           // the VM subscribes to Sensors in its ctor
    scheduler.AdvanceBy(Interval.Ticks);

    Assert.True(loads.ReadCount > readsAfterSpecs);
    Assert.Equal(42, vm.Adapters.Single(a => a.Name == "NVIDIA GeForce RTX 4070").Load);
  }
}
