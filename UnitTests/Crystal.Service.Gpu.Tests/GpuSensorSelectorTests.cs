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

  [Fact]
  public void SelectMemoryUsedGB_ConvertsDiscreteMbSensorToGb() {
    var sensors = new ISensor[] { Sensors.SmallData("GPU Memory Used", 4096) };

    Assert.Equal(4.0, GpuSensorSelector.SelectMemoryUsedGB(sensors));
  }

  [Fact]
  public void SelectMemoryUsedGB_IGpu_SumsDedicatedAndSharedD3DUsage() {
    var sensors = new ISensor[] {
      Sensors.SmallData("D3D Dedicated Memory Used", 512),
      Sensors.SmallData("D3D Shared Memory Used", 512),
    };

    Assert.Equal(1.0, GpuSensorSelector.SelectMemoryUsedGB(sensors));
  }

  [Fact]
  public void SelectMemoryUsedGB_NoMemorySensor_ReturnsNull() =>
      Assert.Null(GpuSensorSelector.SelectMemoryUsedGB([Sensors.Load("3D", 50)]));

  [Fact]
  public void SelectMemoryTotalGB_PrefersDiscreteTotalOverSharedLimit() {
    var sensors = new ISensor[] {
      Sensors.SmallData("D3D Shared Memory Total", 16384),
      Sensors.SmallData("GPU Memory Total", 8192),
    };

    Assert.Equal(8.0, GpuSensorSelector.SelectMemoryTotalGB(sensors));
  }

  [Fact]
  public void SelectMemoryClock_ReadsGpuMemoryClockOnly() {
    var sensors = new ISensor[] {
      Sensors.Clock("GPU Core", 2400),
      Sensors.Clock("GPU Memory", 9000),
    };

    Assert.Equal(9000, GpuSensorSelector.SelectMemoryClock(sensors));
  }

  [Fact]
  public void SelectMemoryClock_ZeroOrNull_TreatedAsAbsent() {
    Assert.Null(GpuSensorSelector.SelectMemoryClock([Sensors.Clock("GPU Memory", 0)]));
    Assert.Null(GpuSensorSelector.SelectMemoryClock([Sensors.Clock("GPU Memory", null)]));
  }

  [Fact]
  public void SelectFanRpm_TakesHighestFan() {
    var sensors = new ISensor[] {
      Sensors.Fan("GPU Fan 1", 1200),
      Sensors.Fan("GPU Fan 2", 1500),
    };

    Assert.Equal(1500, GpuSensorSelector.SelectFanRpm(sensors));
  }

  [Fact]
  public void SelectFanRpm_NoFanSensor_ReturnsNull() =>
      Assert.Null(GpuSensorSelector.SelectFanRpm([Sensors.Load("3D", 50)]));

  [Theory]
  [InlineData("GPU Core")]
  [InlineData("GPU Core Voltage")]
  public void SelectCoreVoltage_MatchesKnownNames(string name) {
    var sensors = new ISensor[] { Sensors.Voltage(name, 0.85f) };

    Assert.Equal(0.85f, GpuSensorSelector.SelectCoreVoltage(sensors));
  }

  [Fact]
  public void SelectCoreVoltage_UnknownName_ReturnsNull() =>
      Assert.Null(GpuSensorSelector.SelectCoreVoltage([Sensors.Voltage("GPU Memory", 1.35f)]));

  [Fact]
  public void SelectHotSpotTemperature_ReadsGpuHotSpotOnly() {
    var sensors = new ISensor[] {
      Sensors.Temp("GPU Core", 65),
      Sensors.Temp("GPU Hot Spot", 90),
    };

    Assert.Equal(90, GpuSensorSelector.SelectHotSpotTemperature(sensors));
  }

  [Fact]
  public void SelectHotSpotTemperature_NoHotSpotSensor_ReturnsNull() =>
      Assert.Null(GpuSensorSelector.SelectHotSpotTemperature([Sensors.Temp("GPU Core", 65)]));

  [Fact]
  public void SelectMemoryTemperature_PrefersMemoryJunctionOverGpuMemory() {
    var sensors = new ISensor[] {
      Sensors.Temp("GPU Memory", 70),
      Sensors.Temp("GPU Memory Junction", 84),
    };

    Assert.Equal(84, GpuSensorSelector.SelectMemoryTemperature(sensors));
  }

  [Fact]
  public void SelectMemoryTemperature_FallsBackToGpuMemory() =>
      Assert.Equal(72, GpuSensorSelector.SelectMemoryTemperature([Sensors.Temp("GPU Memory", 72)]));

  [Fact]
  public void SelectMemoryTemperature_NoMemoryTempSensor_ReturnsNull() =>
      Assert.Null(GpuSensorSelector.SelectMemoryTemperature([Sensors.Temp("GPU Core", 65)]));

  [Fact]
  public void SelectEngineLoads_TakesD3DAndIntelEnginesStripsPrefixOrdersByName() {
    var sensors = new ISensor[] {
      Sensors.Load("D3D Video Decode", 20),
      Sensors.Load("D3D 3D", 75),
      Sensors.Load("GPU Render/Compute", 60),
      Sensors.Load("GPU Media", 10),
    };

    var engines = GpuSensorSelector.SelectEngineLoads(sensors);

    Assert.Equal(
        new[] { ("3D", 75.0), ("Media", 10.0), ("Render/Compute", 60.0), ("Video Decode", 20.0) },
        engines.Select(e => (e.Name, e.LoadPercent)));
  }

  [Fact]
  public void SelectEngineLoads_ExcludesCoreMemoryAndPowerLoads() {
    var sensors = new ISensor[] {
      Sensors.Load("GPU Core", 80),
      Sensors.Load("GPU Memory", 90),
      Sensors.Load("GPU Memory Controller", 40),
      Sensors.Load("GPU Power", 120),        // NVIDIA publishes power rails as Load-typed sensors
      Sensors.Load("D3D 3D", 55),
    };

    var engines = GpuSensorSelector.SelectEngineLoads(sensors);

    Assert.Equal(new[] { ("3D", 55.0) }, engines.Select(e => (e.Name, e.LoadPercent)));
  }

  [Fact]
  public void SelectEngineLoads_ConsolidatesDuplicateNodesByMax() {
    var sensors = new ISensor[] {
      Sensors.Load("D3D 3D", 30),
      Sensors.Load("D3D 3D", 70),
    };

    var engines = GpuSensorSelector.SelectEngineLoads(sensors);

    Assert.Equal(new[] { ("3D", 70.0) }, engines.Select(e => (e.Name, e.LoadPercent)));
  }

  [Fact]
  public void SelectEngineLoads_IgnoresNullValuedSensors() {
    var sensors = new ISensor[] {
      Sensors.Load("D3D 3D", null),
      Sensors.Load("D3D Copy", 12),
    };

    var engines = GpuSensorSelector.SelectEngineLoads(sensors);

    Assert.Equal(new[] { ("Copy", 12.0) }, engines.Select(e => (e.Name, e.LoadPercent)));
  }

  [Fact]
  public void SelectEngineLoads_NoEngineSensors_ReturnsEmpty() =>
      Assert.Empty(GpuSensorSelector.SelectEngineLoads([Sensors.Load("GPU Core", 50)]));

  [Fact]
  public void SelectPcieRxTx_ConvertsBytesPerSecToMBps() {
    var sensors = new ISensor[] {
      Sensors.Throughput("GPU PCIe Rx", 2_000_000),
      Sensors.Throughput("GPU PCIe Tx", 500_000),
    };

    Assert.Equal(2.0, GpuSensorSelector.SelectPcieRxMBps(sensors));
    Assert.Equal(0.5, GpuSensorSelector.SelectPcieTxMBps(sensors));
  }

  [Fact]
  public void SelectPcieRxTx_NoThroughputSensor_ReturnsNull() {
    Assert.Null(GpuSensorSelector.SelectPcieRxMBps([Sensors.Load("GPU Core", 50)]));
    Assert.Null(GpuSensorSelector.SelectPcieTxMBps([Sensors.Load("GPU Core", 50)]));
  }

  [Fact]
  public void SelectPowerRails_ExcludesAggregateRailsStripsPrefixOrdersByName() {
    var sensors = new ISensor[] {
      Sensors.Power("GPU Package", 200),   // aggregate — shown as the headline figure, excluded
      Sensors.Power("GPU SoC", 15),
      Sensors.Power("GPU PPT", 220),
      Sensors.Power("GPU Core", 180),
    };

    var rails = GpuSensorSelector.SelectPowerRails(sensors);

    Assert.Equal(
        new[] { ("Core", 180.0), ("PPT", 220.0), ("SoC", 15.0) },
        rails.Select(r => (r.Name, r.PowerW)));
  }

  [Fact]
  public void SelectPowerRails_KeepsNvidia12VHpwrRailsVerbatim() {
    var sensors = new ISensor[] {
      Sensors.Power("GPU Package", 450),
      Sensors.Power("12VHPWR Connector", 440),
      Sensors.Power("12VHPWR Pin 1", 70),
    };

    var rails = GpuSensorSelector.SelectPowerRails(sensors);

    Assert.Equal(
        new[] { ("12VHPWR Connector", 440.0), ("12VHPWR Pin 1", 70.0) },
        rails.Select(r => (r.Name, r.PowerW)));
  }

  [Fact]
  public void SelectPowerRails_OnlyAggregatePresent_ReturnsEmpty() =>
      Assert.Empty(GpuSensorSelector.SelectPowerRails([Sensors.Power("GPU Package", 120)]));
}
