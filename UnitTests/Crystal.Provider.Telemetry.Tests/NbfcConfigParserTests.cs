using Crystal.Provider.Telemetry.Hardware.Motherboard.Lpc.EC.Nbfc;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

public class NbfcConfigParserTests {
  private const string SampleConfig = """
    <?xml version="1.0"?>
    <FanControlConfigV2 xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <NotebookModel>HP ProBook 450 G5  </NotebookModel>
      <Author>tester</Author>
      <EcPollInterval>3000</EcPollInterval>
      <ReadWriteWords>false</ReadWriteWords>
      <FanConfigurations>
        <FanConfiguration>
          <ReadRegister>45</ReadRegister>
          <WriteRegister>46</WriteRegister>
          <MinSpeedValue>150</MinSpeedValue>
          <MaxSpeedValue>27</MaxSpeedValue>
          <IndependentReadMinMaxValues>false</IndependentReadMinMaxValues>
          <MinSpeedValueRead>0</MinSpeedValueRead>
          <MaxSpeedValueRead>0</MaxSpeedValueRead>
          <ResetRequired>true</ResetRequired>
          <FanSpeedResetValue>255</FanSpeedResetValue>
          <FanDisplayName>CPU</FanDisplayName>
        </FanConfiguration>
        <FanConfiguration>
          <ReadRegister>46</ReadRegister>
          <MinSpeedValue>0</MinSpeedValue>
          <MaxSpeedValue>100</MaxSpeedValue>
          <IndependentReadMinMaxValues>true</IndependentReadMinMaxValues>
          <MinSpeedValueRead>10</MinSpeedValueRead>
          <MaxSpeedValueRead>90</MaxSpeedValueRead>
          <FanDisplayName>GPU</FanDisplayName>
        </FanConfiguration>
      </FanConfigurations>
    </FanControlConfigV2>
    """;

  [Fact]
  public void Parse_ReadsModelAndReadWriteWords() {
    NbfcFanConfig config = NbfcConfigParser.Parse(SampleConfig);

    Assert.NotNull(config);
    Assert.Equal("HP ProBook 450 G5", config.NotebookModel); // trimmed
    Assert.False(config.ReadWriteWords);
    Assert.Equal(2, config.Fans.Count);
  }

  [Fact]
  public void Parse_ReadValueIsRpm_DefaultsFalseAndParsesWhenSet() {
    Assert.False(NbfcConfigParser.Parse(SampleConfig).ReadValueIsRpm);

    NbfcFanConfig rpm = NbfcConfigParser.Parse(
        "<FanControlConfigV2><NotebookModel>X</NotebookModel><ReadValueIsRpm>true</ReadValueIsRpm></FanControlConfigV2>");
    Assert.True(rpm.ReadValueIsRpm);
  }

  [Fact]
  public void Parse_ReadsFanRegistersAndNames() {
    NbfcFanConfig config = NbfcConfigParser.Parse(SampleConfig);

    NbfcFanConfiguration cpu = config.Fans[0];
    Assert.Equal(45, cpu.ReadRegister);
    Assert.Equal(150, cpu.MinSpeedValue);
    Assert.Equal(27, cpu.MaxSpeedValue);
    Assert.Equal("CPU", cpu.FanDisplayName);
  }

  [Fact]
  public void EffectiveReadMinMax_UsesWriteScale_WhenNotIndependent() {
    NbfcFanConfig config = NbfcConfigParser.Parse(SampleConfig);

    NbfcFanConfiguration cpu = config.Fans[0];
    Assert.False(cpu.IndependentReadMinMaxValues);
    Assert.Equal(150, cpu.EffectiveReadMin);
    Assert.Equal(27, cpu.EffectiveReadMax);
  }

  [Fact]
  public void EffectiveReadMinMax_UsesReadScale_WhenIndependent() {
    NbfcFanConfig config = NbfcConfigParser.Parse(SampleConfig);

    NbfcFanConfiguration gpu = config.Fans[1];
    Assert.True(gpu.IndependentReadMinMaxValues);
    Assert.Equal(10, gpu.EffectiveReadMin);
    Assert.Equal(90, gpu.EffectiveReadMax);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("not xml at all")]
  [InlineData("<Unclosed>")]
  public void Parse_InvalidInput_ReturnsNull(string xml) {
    Assert.Null(NbfcConfigParser.Parse(xml));
  }

  [Fact]
  public void Parse_MissingFanConfigurations_ReturnsEmptyFanList() {
    NbfcFanConfig config = NbfcConfigParser.Parse("<FanControlConfigV2><NotebookModel>X</NotebookModel></FanControlConfigV2>");

    Assert.NotNull(config);
    Assert.Empty(config.Fans);
  }
}
