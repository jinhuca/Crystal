using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using System.Windows.Media;
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
  // A second vivid color, distinct from Plot and the neutral background layers, for overlay series.
  private static readonly Color Overlay = Color.FromRgb(0x20, 0xFF, 0x20);

  [Fact]
  public void OverlaySeries_PaintsAlongsideThePrimary() => StaRunner.Run(() => {
    var graph = NewGraph(GraphKind.Line);
    int write = graph.AddSeries(new SolidColorBrush(Overlay), fillBrush: null, thickness: 3);
    // Feed the two series to clearly different heights so each owns its own rows.
    for (int i = 0; i < 10; i++) { graph.AddValue(30); graph.AddValue(write, 80); }

    var px = new PixelRenderer(graph, 240, 120);
    Assert.True(px.CountColor(Plot) > 0, "primary series should still paint");
    Assert.True(px.CountColor(Overlay) > 0, "overlay series should paint too");
  });

  [Fact]
  public void OverlaySeries_IsLineOnly_NotDrawnForBars() => StaRunner.Run(() => {
    var graph = NewGraph(GraphKind.Bar);
    int extra = graph.AddSeries(new SolidColorBrush(Overlay), fillBrush: null, thickness: 3);
    for (int i = 0; i < 10; i++) { graph.AddValue(30); graph.AddValue(extra, 80); }

    // Bars draw the primary series alone; the overlay must not appear.
    Assert.Equal(0, new PixelRenderer(graph, 240, 120).CountColor(Overlay));
  });

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

  private static PerformanceGraph NewGraph(GraphKind kind) {
    var graph = new PerformanceGraph {
      Kind = kind,
      MinValue = 0,
      MaxValue = 100,
      // Neutral background/grid/border so the only Plot-colored pixels come from the data layer.
      GraphBackground = Brushes.Black,
      GridBrush = Brushes.Black,
      BorderBrush = Brushes.Black,
      // Paint the data layer Plot for every kind: Line draws its stroke from LineBrush, while the
      // discrete Bar/SegmentedBar renderers draw their solid block from FillBrush, so both brushes
      // must be Plot for pixel-counting to isolate the data layer regardless of GraphKind.
      FillBrush = new SolidColorBrush(Plot),
      LineBrush = new SolidColorBrush(Plot),
      // Must be a value other than the LineThickness metadata default (2.0): assigning a DP its
      // existing/default value is a no-op that never fires OnLineThicknessChanged, so the internal
      // pen would keep the control's default width. A 3px line also guarantees at least one fully
      // covered device row of the exact Plot color — a 1px line anti-aliases across two rows and
      // blends below PixelRenderer's tolerance, drawing no matching pixels.
      LineThickness = 3
    };
    // A control never attached to a shown window has IsVisible == false, so rendering starts
    // suspended and AddValue/ClearValues would only defer their repaint. These tests exercise the
    // visible-graph render path (the state in which a graph is actually on screen), so mark it
    // visible up front — otherwise RenderTargetBitmap reuses stale cached drawing after a clear.
    graph.ApplyVisibility(true);
    return graph;
  }
}
