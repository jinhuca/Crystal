using System.Windows.Media;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Xunit;

namespace Crystal.Controls.Tests.PerformanceGraphs;

/// <summary>
/// End-to-end render tests: they push the control through its OnRender pipeline and the internal
/// line/bar/segmented-bar renderers, then assert on the pixels actually produced. The data is
/// painted in <see cref="Plot"/> (a vivid color that no other layer uses) so counting those pixels
/// isolates the data layer from the background, grid, and border.
/// </summary>
public class PerformanceGraphRenderTests {
  private static readonly Color Plot = Color.FromRgb(0xFF, 0x20, 0x20);

  [Theory]
  [InlineData(GraphKind.Line)]
  [InlineData(GraphKind.Bar)]
  [InlineData(GraphKind.SegmentedBar)]
  public void AddingValues_DrawsDataPixels(GraphKind kind) => StaRunner.Run(() => {
    var graph = NewGraph(kind);
    for (int i = 0; i < 10; i++) graph.AddValue(60);

    Assert.True(new PixelRenderer(graph, 240, 120).CountColor(Plot) > 0,
        $"{kind} should paint data pixels once values are added");
  });

  [Theory]
  [InlineData(GraphKind.Line)]
  [InlineData(GraphKind.Bar)]
  [InlineData(GraphKind.SegmentedBar)]
  public void EmptyBuffer_DrawsNoDataPixels(GraphKind kind) => StaRunner.Run(() => {
    var graph = NewGraph(kind);

    Assert.Equal(0, new PixelRenderer(graph, 240, 120).CountColor(Plot));
  });

  [Theory]
  [InlineData(GraphKind.Line)]
  [InlineData(GraphKind.Bar)]
  [InlineData(GraphKind.SegmentedBar)]
  public void ClearValues_RemovesDataPixels(GraphKind kind) => StaRunner.Run(() => {
    var graph = NewGraph(kind);
    for (int i = 0; i < 10; i++) graph.AddValue(60);
    Assert.True(new PixelRenderer(graph, 240, 120).CountColor(Plot) > 0);

    graph.ClearValues();

    Assert.Equal(0, new PixelRenderer(graph, 240, 120).CountColor(Plot));
  });

  [Theory]
  [InlineData(GraphKind.Line)]
  [InlineData(GraphKind.Bar)]
  [InlineData(GraphKind.SegmentedBar)]
  public void HigherValues_ReachHigherUpThePlot(GraphKind kind) => StaRunner.Run(() => {
    // Row 0 is the top edge, so a higher value should produce a data pixel on a *lower*-numbered row.
    int lowTop = TopDataRow(kind, 20);
    int highTop = TopDataRow(kind, 95);

    Assert.True(highTop >= 0 && lowTop >= 0, "both readings should draw data");
    Assert.True(highTop < lowTop,
        $"{kind}: a higher value should reach nearer the top, got low@{lowTop} high@{highTop}");
  });

  [Fact]
  public void ValueAboveMax_IsClampedToTopEdge() => StaRunner.Run(() => {
    int atMax = TopDataRow(GraphKind.Bar, 100);
    int overMax = TopDataRow(GraphKind.Bar, 500);

    Assert.True(overMax >= atMax - 3,
        $"over-range value should not climb above a full-scale reading, got max@{atMax} over@{overMax}");
  });

  private static int TopDataRow(GraphKind kind, double value) {
    var graph = NewGraph(kind);
    for (int i = 0; i < 10; i++) graph.AddValue(value);
    return new PixelRenderer(graph, 240, 120).TopMostRowWithColor(Plot);
  }

  private static PerformanceGraph NewGraph(GraphKind kind) => new() {
    Kind = kind,
    MinValue = 0,
    MaxValue = 100,
    // Neutral background/grid/border so the only Plot-colored pixels come from the data layer.
    GraphBackground = Brushes.Black,
    GridBrush = Brushes.Black,
    BorderBrush = Brushes.Black,
    FillBrush = Brushes.Transparent,
    LineBrush = new SolidColorBrush(Plot),
    LineThickness = 2
  };
}
