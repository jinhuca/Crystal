using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Service.Gpu.Tests;

public class GpuSensorSelectorTests {
  [Fact]
  public void SelectCoreLoad_TakesMaxAcrossEngineLoadsExcludingMemory() {
    var sensors = new ISensor[] {
      Sensors.Load("GPU Core", 10),
      Sensors.Load("3D", 65),
      Sensors.Load("Video Decode", 40),
      Sensors.Load("GPU Memory", 95),   // memory load must not count as core activity
    };

    Assert.Equal(65, GpuSensorSelector.SelectCoreLoad(sensors));
  }

  [Fact]
  public void SelectCoreLoad_NoLoadSensors_ReturnsZero() {
    var sensors = new ISensor[] { Sensors.Clock("GPU Core", 2400) };

    Assert.Equal(0, GpuSensorSelector.SelectCoreLoad(sensors));
  }

  [Fact]
  public void SelectCoreLoad_IgnoresNullValues() {
    var sensors = new ISensor[] {
      Sensors.Load("GPU Core", null),
      Sensors.Load("3D", 12),
    };

    Assert.Equal(12, GpuSensorSelector.SelectCoreLoad(sensors));
  }

  [Fact]
  public void SelectCoreClock_ReadsGpuCoreClockOnly() {
    var sensors = new ISensor[] {
      Sensors.Clock("GPU Memory", 9000),
      Sensors.Clock("GPU Core", 2400),
    };

    Assert.Equal(2400, GpuSensorSelector.SelectCoreClock(sensors));
  }

  [Fact]
  public void SelectCoreClock_ZeroOrNull_TreatedAsAbsent() {
    Assert.Null(GpuSensorSelector.SelectCoreClock([Sensors.Clock("GPU Core", 0)]));
    Assert.Null(GpuSensorSelector.SelectCoreClock([Sensors.Clock("GPU Core", null)]));
    Assert.Null(GpuSensorSelector.SelectCoreClock([Sensors.Clock("GPU Memory", 9000)]));
  }

  [Theory]
  [InlineData("GPU Package")]
  [InlineData("GPU Power")]
  [InlineData("GPU Total")]
  public void SelectPackagePower_MatchesAnyKnownRail(string railName) {
    var sensors = new ISensor[] { Sensors.Power(railName, 120) };

    Assert.Equal(120, GpuSensorSelector.SelectPackagePower(sensors));
  }

  [Fact]
  public void SelectPackagePower_PrefersPackageOverFallbacks() {
    var sensors = new ISensor[] {
      Sensors.Power("GPU Total", 200),
      Sensors.Power("GPU Package", 150),
    };

    Assert.Equal(150, GpuSensorSelector.SelectPackagePower(sensors));
  }

  [Fact]
  public void SelectPackagePower_NoPowerSensor_ReturnsNull() =>
      Assert.Null(GpuSensorSelector.SelectPackagePower([Sensors.Load("3D", 50)]));

  [Fact]
  public void SelectCoreTemperature_PrefersGpuCore() {
    var sensors = new ISensor[] {
      Sensors.Temp("GPU Hot Spot", 90),
      Sensors.Temp("GPU Core", 65),
    };

    Assert.Equal(65, GpuSensorSelector.SelectCoreTemperature(sensors));
  }

  [Fact]
  public void SelectCoreTemperature_FallsBackToAnyTempWhenNoGpuCore() {
    var sensors = new ISensor[] {
      Sensors.Load("3D", 50),
      Sensors.Temp("GPU Hot Spot", 88),
    };

    Assert.Equal(88, GpuSensorSelector.SelectCoreTemperature(sensors));
  }

  [Fact]
  public void SelectCoreTemperature_NoTempSensor_ReturnsNull() =>
      Assert.Null(GpuSensorSelector.SelectCoreTemperature([Sensors.Load("3D", 50)]));

  [Theory]
  [InlineData("CPU Package")]
  [InlineData("CPU Cores")]
  [InlineData("Core Max")]
  public void SelectCpuPackageTemperature_MatchesKnownAliases(string name) {
    var sensors = new ISensor[] { Sensors.Temp(name, 55) };

    Assert.Equal(55, GpuSensorSelector.SelectCpuPackageTemperature(sensors));
  }

  [Fact]
  public void SelectCpuPackageTemperature_UnknownName_ReturnsNull() =>
      Assert.Null(GpuSensorSelector.SelectCpuPackageTemperature([Sensors.Temp("VRM", 60)]));
}
