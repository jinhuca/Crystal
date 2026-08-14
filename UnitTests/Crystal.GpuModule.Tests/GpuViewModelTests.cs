using Crystal.GpuModule.Models;
using Crystal.GpuModule.ViewModels;
using Crystal.Service.Gpu;
using Prism.Events;
using System.Reactive.Subjects;
using Xunit;

namespace Crystal.GpuModule.Tests;

public class GpuViewModelTests {
  private sealed class FakeGpuModel : IGpuModel {
    public Subject<GpuSnapshot> SpecsSubject { get; } = new();
    public Subject<GpuSnapshot> SensorsSubject { get; } = new();
    public IObservable<GpuSnapshot> Specs => SpecsSubject;
    public IObservable<GpuSnapshot> Sensors => SensorsSubject;
  }

  private static GpuViewModel CreateVm(out FakeGpuModel model) {
    model = new FakeGpuModel();
    return new GpuViewModel(model, new EventAggregator());
  }

  private static GpuAdapterInfo Adapter(string name, GpuKind kind) =>
      new(Name: name, Kind: kind, VideoRamGB: 8, DisplayMode: "1920x1080",
          DriverVersion: "31.0", DriverDate: null, VideoProcessor: null,
          PhysicalLocation: null, RefreshRateHz: 60);

  private static GpuLoadReading Load(string name, double core = 0, double? temp = null,
                                     double? clock = null, double? power = null) =>
      new(AdapterName: name, CoreLoadPercent: core, TemperatureC: temp, ClockMhz: clock, PowerW: power);

  [Fact]
  public void Specs_emission_builds_one_adapter_row_per_adapter() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(new GpuSnapshot(
        [Adapter("NVIDIA GTX 1070", GpuKind.Dedicated)], []));

    var adapter = Assert.Single(vm.Adapters);
    Assert.Equal("NVIDIA GTX 1070", adapter.Name);
  }

  [Fact]
  public void Specs_emission_orders_integrated_before_dedicated() {
    var vm = CreateVm(out var model);

    // Supplied dedicated-first; the VM must reorder so integrated lands in the left column.
    model.SpecsSubject.OnNext(new GpuSnapshot(
        [Adapter("NVIDIA GTX 1070", GpuKind.Dedicated), Adapter("Intel UHD 630", GpuKind.Integrated)],
        []));

    Assert.Equal("Intel UHD 630", vm.Adapters[0].Name);
    Assert.Equal("NVIDIA GTX 1070", vm.Adapters[1].Name);
  }

  [Fact]
  public void Specs_emission_rebuilds_the_adapter_list_without_duplicating() {
    var vm = CreateVm(out var model);
    var snapshot = new GpuSnapshot([Adapter("Intel UHD 630", GpuKind.Integrated)], []);

    model.SpecsSubject.OnNext(snapshot);
    model.SpecsSubject.OnNext(snapshot);

    // The list is cleared and rebuilt on each spec emission — a second one must not append.
    Assert.Single(vm.Adapters);
  }

  [Fact]
  public void Specs_emission_applies_any_matching_load_carried_on_the_same_snapshot() {
    var vm = CreateVm(out var model);

    model.SpecsSubject.OnNext(new GpuSnapshot(
        [Adapter("NVIDIA GTX 1070", GpuKind.Dedicated)],
        [Load("NVIDIA GTX 1070", core: 45, temp: 62, clock: 1800, power: 120)]));

    var adapter = Assert.Single(vm.Adapters);
    Assert.Equal(45, adapter.Load);
    Assert.Equal(62, adapter.TemperatureC);
    Assert.Equal(1800, adapter.ClockMhz);
    Assert.Equal(120, adapter.PowerW);
  }

  [Fact]
  public void Sensors_emission_updates_load_matching_adapter_by_name_case_insensitively() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(new GpuSnapshot([Adapter("NVIDIA GTX 1070", GpuKind.Dedicated)], []));

    model.SensorsSubject.OnNext(new GpuSnapshot(
        [], [Load("nvidia gtx 1070", core: 88)]));

    Assert.Equal(88, Assert.Single(vm.Adapters).Load);
  }

  [Fact]
  public void Sensors_emission_ignores_a_load_with_no_matching_adapter() {
    var vm = CreateVm(out var model);
    model.SpecsSubject.OnNext(new GpuSnapshot([Adapter("NVIDIA GTX 1070", GpuKind.Dedicated)], []));

    model.SensorsSubject.OnNext(new GpuSnapshot([], [Load("Some Other GPU", core: 99)]));

    // No match → the existing adapter's load stays at its default.
    Assert.Equal(0, Assert.Single(vm.Adapters).Load);
  }

  [Fact]
  public void Sensors_emission_before_specs_arrive_is_harmless() {
    var vm = CreateVm(out var model);

    model.SensorsSubject.OnNext(new GpuSnapshot([], [Load("NVIDIA GTX 1070", core: 50)]));

    Assert.Empty(vm.Adapters);
  }

  [Fact]
  public void ShowDetailCommand_publishes_the_gpu_detail_event() {
    var events = new EventAggregator();
    var vm = new GpuViewModel(new FakeGpuModel(), events);
    string? published = null;
    events.GetEvent<Crystal.Infrastructure.Constants.Navigation.ShowDetailEvent>()
        .Subscribe(name => published = name);

    vm.ShowDetailCommand.Execute(null);

    Assert.Equal(Crystal.Infrastructure.Constants.Navigation.DetailViewNames.Gpu, published);
  }
}
