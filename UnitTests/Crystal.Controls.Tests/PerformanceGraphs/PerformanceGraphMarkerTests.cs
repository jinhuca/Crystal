using System.Windows.Media;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
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
