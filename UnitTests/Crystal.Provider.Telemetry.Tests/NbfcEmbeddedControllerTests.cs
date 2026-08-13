using System.Collections.Generic;
using Crystal.Provider.Telemetry.Hardware;
using Crystal.Provider.Telemetry.Hardware.Motherboard.Lpc.EC;
using Crystal.Provider.Telemetry.Hardware.Motherboard.Lpc.EC.Nbfc;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

public class NbfcEmbeddedControllerTests {
  private static NbfcFanConfig ConfigWithFan(NbfcFanConfiguration fan, bool words = false) {
    var config = new NbfcFanConfig { ReadWriteWords = words };
    config.Fans.Add(fan);
    return config;
  }

  [Fact]
  public void BuildSources_MapsRawRangeOntoZeroToHundredPercent() {
    // Inverted scale: raw 150 = 0%, raw 27 = 100% (NBFC-style, min > max).
    var config = ConfigWithFan(new NbfcFanConfiguration {
      ReadRegister = 45, MinSpeedValue = 150, MaxSpeedValue = 27, FanDisplayName = "CPU"
    });

    IReadOnlyList<EmbeddedControllerSource> sources = NbfcEmbeddedController.BuildSources(config);

    EmbeddedControllerSource cpu = Assert.Single(sources);
    Assert.Equal("CPU", cpu.Name);
    Assert.Equal(SensorType.Control, cpu.Type);
    Assert.Equal(45, cpu.Register);
    Assert.Equal(1, cpu.Size);
    Assert.Equal(0f, cpu.ClampMin);
    Assert.Equal(100f, cpu.ClampMax);

    // Verify the linear map at both endpoints and midpoint via factor/offset.
    Assert.Equal(0f, 150 * cpu.Factor + cpu.Offset, precision: 3);
    Assert.Equal(100f, 27 * cpu.Factor + cpu.Offset, precision: 3);
    Assert.Equal(50f, 88.5f * cpu.Factor + cpu.Offset, precision: 2);
  }

  [Fact]
  public void BuildSources_UsesIndependentReadScale_WhenSet() {
    var config = ConfigWithFan(new NbfcFanConfiguration {
      ReadRegister = 46,
      MinSpeedValue = 0, MaxSpeedValue = 100,
      IndependentReadMinMaxValues = true,
      MinSpeedValueRead = 10, MaxSpeedValueRead = 90,
      FanDisplayName = "GPU"
    });

    EmbeddedControllerSource gpu = Assert.Single(NbfcEmbeddedController.BuildSources(config));
    Assert.Equal(0f, 10 * gpu.Factor + gpu.Offset, precision: 3);
    Assert.Equal(100f, 90 * gpu.Factor + gpu.Offset, precision: 3);
  }

  [Fact]
  public void BuildSources_WordReads_UseTwoBytesLittleEndian() {
    var config = ConfigWithFan(new NbfcFanConfiguration {
      ReadRegister = 4, MinSpeedValue = 0, MaxSpeedValue = 255
    }, words: true);

    EmbeddedControllerSource fan = Assert.Single(NbfcEmbeddedController.BuildSources(config));
    Assert.Equal(2, fan.Size);
    Assert.True(fan.IsLittleEndian);
  }

  [Fact]
  public void BuildSources_DegenerateScale_IsSkipped() {
    var config = ConfigWithFan(new NbfcFanConfiguration {
      ReadRegister = 45, MinSpeedValue = 50, MaxSpeedValue = 50
    });

    Assert.Empty(NbfcEmbeddedController.BuildSources(config));
  }

  [Fact]
  public void BuildSources_BlankName_FallsBackToIndexedName() {
    var config = new NbfcFanConfig();
    config.Fans.Add(new NbfcFanConfiguration { ReadRegister = 1, MinSpeedValue = 0, MaxSpeedValue = 100 });
    config.Fans.Add(new NbfcFanConfiguration { ReadRegister = 2, MinSpeedValue = 0, MaxSpeedValue = 100 });

    IReadOnlyList<EmbeddedControllerSource> sources = NbfcEmbeddedController.BuildSources(config);
    Assert.Equal("Fan #1", sources[0].Name);
    Assert.Equal("Fan #2", sources[1].Name);
  }

  [Fact]
  public void BuildSources_RpmMode_EmitsRpmFanSensorUnscaled() {
    var config = new NbfcFanConfig { ReadWriteWords = true, ReadValueIsRpm = true };
    config.Fans.Add(new NbfcFanConfiguration {
      ReadRegister = 132, MinSpeedValue = 0, MaxSpeedValue = 5500, FanDisplayName = "CPU Fan"
    });

    EmbeddedControllerSource fan = Assert.Single(NbfcEmbeddedController.BuildSources(config));
    Assert.Equal("CPU Fan", fan.Name);
    Assert.Equal(SensorType.Fan, fan.Type);
    Assert.Equal(132, fan.Register);
    Assert.Equal(2, fan.Size);
    Assert.True(fan.IsLittleEndian);
    // Raw reading passes through as RPM: no duty mapping.
    Assert.Equal(1f, fan.Factor);
    Assert.Equal(0f, fan.Offset);
    Assert.Equal(0f, fan.ClampMin);
    Assert.Null(fan.ClampMax);
  }

  [Fact]
  public void BuildSources_RpmMode_IgnoresDegenerateSpeedScale() {
    // A degenerate min==max scale would be skipped in percentage mode; in RPM mode the scale is
    // irrelevant, so the fan must still be emitted.
    var config = new NbfcFanConfig { ReadValueIsRpm = true };
    config.Fans.Add(new NbfcFanConfiguration { ReadRegister = 132, MinSpeedValue = 50, MaxSpeedValue = 50 });

    EmbeddedControllerSource fan = Assert.Single(NbfcEmbeddedController.BuildSources(config));
    Assert.Equal(SensorType.Fan, fan.Type);
  }

  [Fact]
  public void Create_NoUsableFans_ReturnsNull() {
    var config = ConfigWithFan(new NbfcFanConfiguration { ReadRegister = 1, MinSpeedValue = 50, MaxSpeedValue = 50 });
    Assert.Null(NbfcEmbeddedController.Create(config, new TestSettings()));
  }

  private sealed class TestSettings : ISettings {
    public bool Contains(string name) => false;
    public void SetValue(string name, string value) { }
    public string GetValue(string name, string value) => value;
    public void Remove(string name) { }
  }
}
