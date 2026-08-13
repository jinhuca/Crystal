using Crystal.Provider.Telemetry.Hardware.Motherboard.Lpc.EC.Nbfc;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

public class NbfcConfigStoreTests {
  private static NbfcFanConfig Config(string model) => new() { NotebookModel = model };

  [Fact]
  public void TryGetForModel_ExactMatch_ReturnsConfig() {
    var store = new NbfcConfigStore([Config("HP ProBook 450 G5"), Config("Lenovo ThinkPad X1")]);

    Assert.True(store.TryGetForModel("HP ProBook 450 G5", out var config));
    Assert.Equal("HP ProBook 450 G5", config.NotebookModel);
  }

  [Theory]
  [InlineData("hp probook 450 g5")]        // case-insensitive
  [InlineData("  HP ProBook 450 G5  ")]    // surrounding whitespace
  [InlineData("HP  ProBook   450 G5")]     // collapsed internal whitespace
  public void TryGetForModel_NormalizesModelString(string query) {
    var store = new NbfcConfigStore([Config("HP ProBook 450 G5")]);

    Assert.True(store.TryGetForModel(query, out var config));
    Assert.NotNull(config);
  }

  [Theory]
  [InlineData("Unknown Model")]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public void TryGetForModel_NoMatch_ReturnsFalse(string? query) {
    var store = new NbfcConfigStore([Config("HP ProBook 450 G5")]);

    Assert.False(store.TryGetForModel(query, out var config));
    Assert.Null(config);
  }

  [Fact]
  public void Constructor_DuplicateModel_FirstWins() {
    var first = new NbfcFanConfig { NotebookModel = "Dup" };
    first.Fans.Add(new NbfcFanConfiguration { ReadRegister = 1 });
    var second = new NbfcFanConfig { NotebookModel = "dup" };
    second.Fans.Add(new NbfcFanConfiguration { ReadRegister = 2 });

    var store = new NbfcConfigStore([first, second]);

    Assert.True(store.TryGetForModel("Dup", out var config));
    Assert.Equal(1, config.Fans[0].ReadRegister);
  }

  [Fact]
  public void Constructor_SkipsBlankModelNames() {
    var store = new NbfcConfigStore([Config(""), Config("   "), Config("Real")]);

    Assert.True(store.TryGetForModel("Real", out _));
    Assert.False(store.TryGetForModel("", out _));
  }

  [Fact]
  public void TryGetForModel_Candidates_MatchesSecondWhenFirstMisses() {
    // Lenovo: SMBIOS product name is the machine-type code; the friendly name lives in the version.
    var store = new NbfcConfigStore([Config("ThinkPad P1")]);

    Assert.True(store.TryGetForModel(["20MD000", "ThinkPad P1"], out var config));
    Assert.Equal("ThinkPad P1", config.NotebookModel);
  }

  [Fact]
  public void TryGetForModel_Candidates_PrefersFirstMatch() {
    var store = new NbfcConfigStore([Config("Primary"), Config("Secondary")]);

    Assert.True(store.TryGetForModel(["Primary", "Secondary"], out var config));
    Assert.Equal("Primary", config.NotebookModel);
  }

  [Fact]
  public void TryGetForModel_Candidates_NoMatch_ReturnsFalse() {
    var store = new NbfcConfigStore([Config("ThinkPad P1")]);

    Assert.False(store.TryGetForModel([null, "", "Unknown"], out var config));
    Assert.Null(config);
  }

  [Fact]
  public void FromDirectory_MissingDirectory_ReturnsEmptyStore() {
    var store = NbfcConfigStore.FromDirectory(@"Z:\definitely\not\here\fanconfigs");

    Assert.False(store.TryGetForModel("anything", out _));
  }
}
