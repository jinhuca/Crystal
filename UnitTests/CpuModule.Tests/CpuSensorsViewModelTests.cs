using CpuModule.ViewModels.Implementations;
using Xunit;

namespace CpuModule.Tests;

public class CpuSensorsViewModelTests {
  [Fact]
  public void Update_copies_live_readings_and_scales_speed_to_ghz() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(load: 42, voltage: 1.25f, speedMHz: 4200, power: 65, temperature: 58));

    Assert.Equal(42, vm.Load);
    Assert.Equal(1.25, vm.Voltage, precision: 3);
    // 4200 MHz surfaces as 4.2 GHz.
    Assert.Equal(4.2, vm.SpeedGhz, precision: 3);
    Assert.Equal(65, vm.Power);
    Assert.Equal(58, vm.Temperature);
  }

  [Fact]
  public void Update_treats_missing_readings_as_zero() {
    var vm = new CpuSensorsViewModel();

    // No MSR values at all: OS load is present but voltage/speed/power/temp are empty.
    vm.Update(Fakes.System(load: 10));

    Assert.Equal(10, vm.Load);
    Assert.Equal(0, vm.Voltage);
    Assert.Equal(0, vm.SpeedGhz);
    Assert.Equal(0, vm.Power);
    Assert.Equal(0, vm.Temperature);
  }

  [Fact]
  public void Msr_availability_stays_false_when_no_msr_reading_arrives() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(load: 10));

    Assert.False(vm.MsrSensorsAvailable);
  }

  [Fact]
  public void Msr_availability_latches_true_on_first_non_null_msr_reading() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(load: 10, temperature: 55));

    Assert.True(vm.MsrSensorsAvailable);
  }

  [Fact]
  public void Msr_availability_stays_true_after_a_later_empty_poll() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(load: 10, voltage: 1.2f));
    vm.Update(Fakes.System(load: 10));   // MSR readings momentarily drop out

    Assert.True(vm.MsrSensorsAvailable);
  }

  [Fact]
  public void Update_creates_one_core_row_per_core_labelled_in_order() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(coreLoads: [10f, 20f, 30f]));

    Assert.Equal(3, vm.CoreLoads.Count);
    Assert.Equal("C00", vm.CoreLoads[0].Label);
    Assert.Equal("C01", vm.CoreLoads[1].Label);
    Assert.Equal("C02", vm.CoreLoads[2].Label);
    Assert.Equal(10, vm.CoreLoads[0].Load);
    Assert.Equal(30, vm.CoreLoads[2].Load);
  }

  [Fact]
  public void Update_refreshes_core_loads_in_place_without_reallocating_rows() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(coreLoads: [10f, 20f]));
    var firstRow = vm.CoreLoads[0];
    vm.Update(Fakes.System(coreLoads: [55f, 60f]));

    // Same row count and same instances — only the Load value changes on subsequent polls.
    Assert.Equal(2, vm.CoreLoads.Count);
    Assert.Same(firstRow, vm.CoreLoads[0]);
    Assert.Equal(55, vm.CoreLoads[0].Load);
  }

  [Fact]
  public void Update_with_no_socket_is_a_noop() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.Empty());

    Assert.Equal(0, vm.Load);
    Assert.Empty(vm.CoreLoads);
    Assert.False(vm.MsrSensorsAvailable);
  }

  [Fact]
  public void UpdateFan_latches_presence_and_truncates_rpm() {
    var vm = new CpuSensorsViewModel();

    vm.UpdateFan(1234.7f);

    Assert.True(vm.HasCpuFan);
    Assert.Equal(1234, vm.FanRpm);
  }

  [Fact]
  public void UpdateFan_with_null_before_any_reading_leaves_fan_absent() {
    var vm = new CpuSensorsViewModel();

    vm.UpdateFan(null);

    Assert.False(vm.HasCpuFan);
    Assert.Equal(0, vm.FanRpm);
  }

  [Fact]
  public void UpdateFan_keeps_last_known_state_on_a_later_null_poll() {
    var vm = new CpuSensorsViewModel();

    vm.UpdateFan(900f);
    vm.UpdateFan(null);   // a single poll momentarily reports no fan

    Assert.True(vm.HasCpuFan);
    Assert.Equal(900, vm.FanRpm);
  }
}
