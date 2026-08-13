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

  [Fact]
  public void Update_copies_intel_power_and_clock_extras() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(
        effectiveSpeedMHz: 3600, busSpeedMHz: 100.5f,
        powerLimitLongW: 65, powerLimitShortW: 90, distanceToTjMax: 22));

    Assert.Equal(3.6, vm.EffectiveSpeedGhz, precision: 3);
    Assert.Equal(100.5, vm.BusSpeedMHz, precision: 3);
    Assert.Equal(65, vm.PowerLimitLongW);
    Assert.Equal(90, vm.PowerLimitShortW);
    Assert.Equal(22, vm.DistanceToTjMax);
  }

  [Fact]
  public void Update_copies_amd_current_and_soc_voltage() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(socVoltage: 1.1f, tdcAmps: 95, edcAmps: 140));

    Assert.Equal(1.1, vm.SocVoltage, precision: 3);
    Assert.Equal(95, vm.TdcAmps);
    Assert.Equal(140, vm.EdcAmps);
  }

  [Fact]
  public void Update_copies_package_cstate_residency() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(c2Pct: 5, c3Pct: 10, c6Pct: 60, c7Pct: 25));

    Assert.Equal(5, vm.PackageC2Pct);
    Assert.Equal(10, vm.PackageC3Pct);
    Assert.Equal(60, vm.PackageC6Pct);
    Assert.Equal(25, vm.PackageC7Pct);
  }

  [Fact]
  public void Update_leaves_unexposed_extras_at_zero() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(load: 10));

    Assert.Equal(0, vm.EffectiveSpeedGhz);
    Assert.Equal(0, vm.BusSpeedMHz);
    Assert.Equal(0, vm.PowerLimitLongW);
    Assert.Equal(0, vm.TdcAmps);
    Assert.Equal(0, vm.PackageC6Pct);
    Assert.Equal(0, vm.DistanceToTjMax);
  }

  [Fact]
  public void ThrottleStatus_is_empty_when_no_flags_are_set() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(temperature: 55, distanceToTjMax: 40));

    Assert.False(vm.IsThrottling);
    Assert.Equal(string.Empty, vm.ThrottleStatus);
  }

  [Fact]
  public void ThrottleStatus_reports_each_active_flag() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(thermalThrottling: 1, powerLimitThrottling: 1, prochot: 1));

    Assert.True(vm.IsThrottling);
    Assert.Equal("THROTTLING: Thermal, Power Limit, PROCHOT", vm.ThrottleStatus);
  }

  [Fact]
  public void ThrottleStatus_falls_back_to_thermal_headroom_when_flag_absent() {
    var vm = new CpuSensorsViewModel();

    // No provider throttle flags, but the hottest core has reached TjMax.
    vm.Update(Fakes.System(distanceToTjMax: 0));

    Assert.True(vm.IsThrottling);
    Assert.Equal("THROTTLING: Thermal", vm.ThrottleStatus);
  }

  [Fact]
  public void ThrottleStatus_ignores_unexposed_tjmax_headroom() {
    var vm = new CpuSensorsViewModel();

    // Neither the throttle flag nor the distance-to-TjMax sensor is exposed: an
    // unexposed 0 must not masquerade as "at TjMax".
    vm.Update(Fakes.System(load: 10));

    Assert.False(vm.IsThrottling);
    Assert.Equal(string.Empty, vm.ThrottleStatus);
  }

  [Fact]
  public void Update_maps_per_core_detail_readings() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(cores: [
        Fakes.Core(load: 40, speedMHz: 4200, effectiveSpeedMHz: 3800,
                   multiplier: 42, distanceToTjMax: 18, power: 12),
    ]));

    var core = vm.CoreLoads[0];
    Assert.Equal(40, core.Load);
    Assert.Equal(4.2, core.SpeedGhz, precision: 3);
    Assert.Equal(3.8, core.EffectiveSpeedGhz, precision: 3);
    Assert.Equal(42, core.Multiplier);
    Assert.Equal(18, core.DistanceToTjMax);
    Assert.Equal(12, core.Power);
  }

  [Fact]
  public void Update_creates_one_thread_row_per_thread_load() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(cores: [Fakes.Core(load: 50, threadLoads: [30f, 70f])]));

    var core = vm.CoreLoads[0];
    Assert.Equal(2, core.Threads.Count);
    Assert.Equal(30, core.Threads[0].Load);
    Assert.Equal(70, core.Threads[1].Load);
  }

  [Fact]
  public void Update_refreshes_thread_loads_in_place() {
    var vm = new CpuSensorsViewModel();

    vm.Update(Fakes.System(cores: [Fakes.Core(threadLoads: [10f, 20f])]));
    var firstThread = vm.CoreLoads[0].Threads[0];
    vm.Update(Fakes.System(cores: [Fakes.Core(threadLoads: [55f, 60f])]));

    Assert.Equal(2, vm.CoreLoads[0].Threads.Count);
    Assert.Same(firstThread, vm.CoreLoads[0].Threads[0]);
    Assert.Equal(55, vm.CoreLoads[0].Threads[0].Load);
  }
}
