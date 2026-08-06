using System.Windows.Media;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using Xunit;

namespace Crystal.Controls.Tests.PerformanceGraphs;

public class GraphThemesTests {
  [Fact]
  public void FromAccent_SetsLineBrushToAccent() {
    var accent = Color.FromRgb(0x12, 0x34, 0x56);

    GraphTheme theme = GraphThemes.FromAccent(accent);

    var line = Assert.IsType<SolidColorBrush>(theme.LineBrush);
    Assert.Equal(accent, line.Color);
  }

  [Fact]
  public void FromAccent_LineKind_UsesGradientFill() {
    GraphTheme theme = GraphThemes.FromAccent(Color.FromRgb(1, 2, 3), GraphKind.Line);

    Assert.IsType<LinearGradientBrush>(theme.FillBrush);
  }

  [Theory]
  [InlineData(GraphKind.Bar)]
  [InlineData(GraphKind.SegmentedBar)]
  public void FromAccent_DiscreteKinds_UseFlatSolidFill(GraphKind kind) {
    GraphTheme theme = GraphThemes.FromAccent(Color.FromRgb(1, 2, 3), kind);

    Assert.IsType<SolidColorBrush>(theme.FillBrush);
  }

  [Fact]
  public void FromAccent_ReturnsFrozenBrushes() {
    GraphTheme theme = GraphThemes.FromAccent(Color.FromRgb(1, 2, 3));

    Assert.True(theme.LineBrush!.IsFrozen);
    Assert.True(theme.FillBrush!.IsFrozen);
    Assert.True(theme.GraphBackground!.IsFrozen);
    Assert.True(theme.GridBrush!.IsFrozen);
    Assert.True(theme.BorderBrush!.IsFrozen);
  }

  [Fact]
  public void FromAccent_SetsLineThickness() {
    GraphTheme theme = GraphThemes.FromAccent(Color.FromRgb(1, 2, 3));

    Assert.Equal(1.5, theme.LineThickness);
  }

  [Fact]
  public void Emerald_UsesExpectedAccent() {
    GraphTheme theme = GraphThemes.Emerald();

    var line = Assert.IsType<SolidColorBrush>(theme.LineBrush);
    Assert.Equal(Color.FromRgb(0x3B, 0xD1, 0x5A), line.Color);
  }

  [Fact]
  public void Presets_ProduceDistinctAccents() {
    var rose = ((SolidColorBrush)GraphThemes.Rose().LineBrush!).Color;
    var emerald = ((SolidColorBrush)GraphThemes.Emerald().LineBrush!).Color;
    var amber = ((SolidColorBrush)GraphThemes.Amber().LineBrush!).Color;
    var sky = ((SolidColorBrush)GraphThemes.Sky().LineBrush!).Color;

    var colors = new[] { rose, emerald, amber, sky };
    Assert.Equal(colors.Length, colors.Distinct().Count());
  }
}
