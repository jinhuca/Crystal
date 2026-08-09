using GpuModule.Models;
using GpuModule.ViewModels;
using Xunit;

namespace GpuModule.Tests;

public class GpuAdapterViewModelTests {
  private static GpuAdapterInfo Info(string name = "NVIDIA GeForce GTX 1070",
                                     GpuKind kind = GpuKind.Dedicated) =>
      new(Name: name, Kind: kind, VideoRamGB: 8, DisplayMode: "1920x1080",
          DriverVersion: "31.0.15", DriverDate: null, VideoProcessor: "GP104",
          PhysicalLocation: "PCI bus 1", RefreshRateHz: 60);

  [Fact]
  public void Dedicated_adapter_labels_as_dedicated_gpu() {
    var vm = new GpuAdapterViewModel();

    vm.UpdateSpecs(Info(kind: GpuKind.Dedicated));

    Assert.Equal("Dedicated GPU", vm.KindLabel);
    Assert.Equal("NVIDIA GeForce GTX 1070", vm.Name);
  }

  [Fact]
  public void Integrated_adapter_labels_as_integrated_gpu() {
    var vm = new GpuAdapterViewModel();

    vm.UpdateSpecs(Info(name: "Intel UHD Graphics 630", kind: GpuKind.Integrated));

    Assert.Equal("Integrated GPU", vm.KindLabel);
    Assert.Equal("Intel UHD Graphics 630", vm.Name);
  }

  [Fact]
  public void Name_defaults_to_placeholder_before_specs_arrive() {
    var vm = new GpuAdapterViewModel();

    Assert.Equal("—", vm.Name);
    Assert.Equal(string.Empty, vm.KindLabel);
  }

  [Fact]
  public void Update_specs_refreshes_static_identity_fields() {
    var vm = new GpuAdapterViewModel();

    vm.UpdateSpecs(Info());

    Assert.Equal(8, vm.VideoRamGB);
    Assert.Equal("1920x1080", vm.DisplayMode);
    Assert.Equal("31.0.15", vm.DriverVersion);
    Assert.Equal("GP104", vm.VideoProcessor);
  }

  [Fact]
  public void Update_load_sets_live_values_without_a_graph_attached() {
    var vm = new GpuAdapterViewModel();

    // No graph attached: pushing samples must set the scalar values and not throw.
    vm.UpdateLoad(loadPercent: 31.5, temperatureC: 62, clockMhz: 1800, powerW: 120);

    Assert.Equal(31.5, vm.Load);
    Assert.Equal(62, vm.TemperatureC);
    Assert.Equal(1800, vm.ClockMhz);
    Assert.Equal(120, vm.PowerW);
  }

  [Fact]
  public void Update_load_keeps_null_optional_readings() {
    var vm = new GpuAdapterViewModel();

    vm.UpdateLoad(loadPercent: 10, temperatureC: null, clockMhz: null, powerW: null);

    Assert.Equal(10, vm.Load);
    Assert.Null(vm.TemperatureC);
    Assert.Null(vm.ClockMhz);
    Assert.Null(vm.PowerW);
  }
}
