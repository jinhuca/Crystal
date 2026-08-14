using Crystal.Service.Gpu;
using Crystal.GpuModule.ViewModels;
using Xunit;

namespace Crystal.GpuModule.Tests;

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
    vm.UpdateLoad(new GpuLoadReading("GPU", 31.5, TemperatureC: 62, ClockMhz: 1800, PowerW: 120,
        MemoryUsedGB: 3, MemoryTotalGB: 8, MemoryClockMhz: 9000, FanRpm: 1400, CoreVoltageV: 0.85,
        PcieRxMBps: 12.5, PcieTxMBps: 3.2));

    Assert.Equal(31.5, vm.Load);
    Assert.Equal(62, vm.TemperatureC);
    Assert.Equal(1800, vm.ClockMhz);
    Assert.Equal(120, vm.PowerW);
    Assert.Equal(3, vm.MemoryUsedGB);
    Assert.Equal(8, vm.MemoryTotalGB);
    Assert.Equal(37.5, vm.MemoryUsedPercent);
    Assert.Equal(9000, vm.MemoryClockMhz);
    Assert.Equal(1400, vm.FanRpm);
    Assert.Equal(0.85, vm.CoreVoltageV);
    Assert.Equal(12.5, vm.PcieRxMBps);
    Assert.Equal(3.2, vm.PcieTxMBps);
  }

  [Fact]
  public void Update_load_keeps_null_optional_readings() {
    var vm = new GpuAdapterViewModel();

    vm.UpdateLoad(new GpuLoadReading("GPU", 10, TemperatureC: null, ClockMhz: null, PowerW: null));

    Assert.Equal(10, vm.Load);
    Assert.Null(vm.TemperatureC);
    Assert.Null(vm.ClockMhz);
    Assert.Null(vm.PowerW);
    Assert.Null(vm.MemoryUsedGB);
    Assert.Null(vm.MemoryUsedPercent);
    Assert.Null(vm.FanRpm);
    Assert.Null(vm.CoreVoltageV);
  }

  [Fact]
  public void Update_load_reconciles_engine_rows_in_place() {
    var vm = new GpuAdapterViewModel();

    vm.UpdateLoad(new GpuLoadReading("GPU", 60,
        TemperatureC: null, ClockMhz: null, PowerW: null,
        EngineLoads: [new GpuEngineLoad("3D", 60), new GpuEngineLoad("Copy", 5)]));

    Assert.True(vm.HasEngineLoads);
    var threeD = Assert.Single(vm.EngineLoads, e => e.Name == "3D");

    // Second poll updates the same row instance (matched by name) rather than replacing it.
    vm.UpdateLoad(new GpuLoadReading("GPU", 80,
        TemperatureC: null, ClockMhz: null, PowerW: null,
        EngineLoads: [new GpuEngineLoad("3D", 80), new GpuEngineLoad("Copy", 5)]));

    Assert.Same(threeD, Assert.Single(vm.EngineLoads, e => e.Name == "3D"));
    Assert.Equal(80, threeD.LoadPercent);
    Assert.Equal(2, vm.EngineLoads.Count);
  }

  [Fact]
  public void Update_load_without_engine_loads_leaves_collection_empty() {
    var vm = new GpuAdapterViewModel();

    vm.UpdateLoad(new GpuLoadReading("GPU", 10, TemperatureC: null, ClockMhz: null, PowerW: null));

    Assert.False(vm.HasEngineLoads);
    Assert.Empty(vm.EngineLoads);
    Assert.False(vm.HasPowerRails);
    Assert.Empty(vm.PowerRails);
  }

  [Fact]
  public void Clock_and_power_scale_start_at_their_floors() {
    var vm = new GpuAdapterViewModel();

    Assert.Equal(500, vm.ClockScaleMax);
    Assert.Equal(50, vm.PowerScaleMax);
  }

  [Fact]
  public void Clock_and_power_scale_ratchet_to_a_nice_value_above_the_peak() {
    var vm = new GpuAdapterViewModel();

    vm.UpdateLoad(new GpuLoadReading("GPU", 50, TemperatureC: 60, ClockMhz: 2600, PowerW: 320));

    // 2600 → nice 5000; 320 → nice 500.
    Assert.Equal(5000, vm.ClockScaleMax);
    Assert.Equal(500, vm.PowerScaleMax);
  }

  [Fact]
  public void Clock_and_power_scale_hold_their_floor_for_small_readings() {
    var vm = new GpuAdapterViewModel();

    vm.UpdateLoad(new GpuLoadReading("GPU", 5, TemperatureC: 40, ClockMhz: 300, PowerW: 12));

    Assert.Equal(500, vm.ClockScaleMax);
    Assert.Equal(50, vm.PowerScaleMax);
  }

  [Fact]
  public void Update_load_reconciles_power_rails_in_place() {
    var vm = new GpuAdapterViewModel();

    vm.UpdateLoad(new GpuLoadReading("GPU", 60,
        TemperatureC: null, ClockMhz: null, PowerW: 200,
        PowerRails: [new GpuPowerRail("PPT", 210), new GpuPowerRail("SoC", 15)]));

    Assert.True(vm.HasPowerRails);
    var ppt = Assert.Single(vm.PowerRails, r => r.Name == "PPT");

    vm.UpdateLoad(new GpuLoadReading("GPU", 60,
        TemperatureC: null, ClockMhz: null, PowerW: 200,
        PowerRails: [new GpuPowerRail("PPT", 240), new GpuPowerRail("SoC", 15)]));

    Assert.Same(ppt, Assert.Single(vm.PowerRails, r => r.Name == "PPT"));
    Assert.Equal(240, ppt.PowerW);
    Assert.Equal(2, vm.PowerRails.Count);
  }
}
