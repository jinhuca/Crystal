using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using System.Windows.Media;
using Xunit;

namespace Crystal.Controls.Tests.PerformanceGraphs;

/// <summary>Render tests for the opt-in session-extreme marker lines. The marker is painted in a
/// vivid color no other layer uses, so counting those pixels isolates the marker from data/grid.</summary>
public class PerformanceGraphMarkerTests {
  private static readonly Color Mark = Color.FromRgb(0x20, 0xFF, 0x20);

  [Fact]
  public void NoMarkerBrush_DrawsNoMarker() => StaRunner.Run(() => {
    var graph = NewGraph();
    graph.HighMarker = 80;  // value set, but no brush → nothing drawn

    Assert.Equal(0, new PixelRenderer(graph, 240, 120).CountColor(Mark));
  });

  [Fact]
  public void MarkerBrushWithoutValue_DrawsNoMarker() => StaRunner.Run(() => {
    var graph = NewGraph();
    graph.MarkerBrush = new SolidColorBrush(Mark);  // NaN markers (default) → nothing drawn

    Assert.Equal(0, new PixelRenderer(graph, 240, 120).CountColor(Mark));
  });

  [Fact]
  public void HighMarker_DrawsNearerTheTopThanLowMarker() => StaRunner.Run(() => {
    var high = NewGraph();
    high.MarkerBrush = new SolidColorBrush(Mark);
    high.HighMarker = 90;

    var low = NewGraph();
    low.MarkerBrush = new SolidColorBrush(Mark);
    low.LowMarker = 10;

    int highRow = new PixelRenderer(high, 240, 120).TopMostRowWithColor(Mark);
    int lowRow = new PixelRenderer(low, 240, 120).TopMostRowWithColor(Mark);

    Assert.True(highRow >= 0 && lowRow >= 0, "both markers should draw");
    Assert.True(highRow < lowRow, $"high marker should sit above low, got high@{highRow} low@{lowRow}");
  });

  [Fact]
  public void MarkerFormat_LabelsTheLine_AddingMarkColoredPixels() => StaRunner.Run(() => {
    var unlabeled = NewGraph();
    unlabeled.MarkerBrush = new SolidColorBrush(Mark);
    unlabeled.HighMarker = 90;

    var labeled = NewGraph();
    labeled.MarkerBrush = new SolidColorBrush(Mark);
    labeled.HighMarker = 90;
    labeled.MarkerFormat = "0.0";  // adds a "90.0" glyph run in the same mark color

    int lineOnly = new PixelRenderer(unlabeled, 240, 120).CountColor(Mark);
    int lineAndLabel = new PixelRenderer(labeled, 240, 120).CountColor(Mark);

    Assert.True(lineAndLabel > lineOnly,
        $"labeled marker should paint more mark pixels than the bare line, got {lineAndLabel} vs {lineOnly}");
  });

  [Fact]
  public void MarkerFormat_WithoutValue_DrawsNothing() => StaRunner.Run(() => {
    var graph = NewGraph();
    graph.MarkerBrush = new SolidColorBrush(Mark);
    graph.MarkerFormat = "0.0";  // format set, but markers are NaN → still nothing drawn

    Assert.Equal(0, new PixelRenderer(graph, 240, 120).CountColor(Mark));
  });

  // Neutral background/grid/border/line so the only Mark-colored pixels come from the marker layer.
  private static PerformanceGraph NewGraph() => new() {
    Kind = GraphKind.Line,
    MinValue = 0,
    MaxValue = 100,
    GraphBackground = Brushes.Black,
    GridBrush = Brushes.Black,
    BorderBrush = Brushes.Black,
    FillBrush = Brushes.Transparent,
    LineBrush = Brushes.Black,
  };
}
