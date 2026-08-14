using System.Windows.Media;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using Xunit;

namespace Crystal.Controls.Tests.PerformanceGraphs;

public class PerformanceGraphTests {
  [Fact]
  public void Defaults_MatchDocumentedValues() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();

    Assert.Equal(60, graph.Capacity);
    Assert.Equal(60, graph.GridColumns);
    Assert.Equal(GraphKind.Line, graph.Kind);
    Assert.Equal(0.0, graph.MinValue);
    Assert.Equal(100.0, graph.MaxValue);
  });

  [Fact]
  public void Constructor_NonPositiveHistoryLength_Throws() => StaRunner.Run(() => {
    Assert.Throws<ArgumentOutOfRangeException>(() => new PerformanceGraph(0, 60));
  });

  [Fact]
  public void Constructor_NonPositiveGridColumns_Throws() => StaRunner.Run(() => {
    Assert.Throws<ArgumentOutOfRangeException>(() => new PerformanceGraph(60, 0));
  });

  [Fact]
  public void Constructor_HistoryAndGridColumns_AreIndependent() => StaRunner.Run(() => {
    var graph = new PerformanceGraph(120, 30);

    Assert.Equal(120, graph.Capacity);
    Assert.Equal(30, graph.GridColumns);
  });

  [Fact]
  public void AddValue_DoesNotThrow_AndKindStaysConfigurable() => StaRunner.Run(() => {
    var graph = new PerformanceGraph { Kind = GraphKind.Bar };

    graph.AddValue(42);
    graph.AddValue(43);

    Assert.Equal(GraphKind.Bar, graph.Kind);
  });

  [Fact]
  public void ClearValues_DoesNotThrow() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();
    graph.AddValue(1);

    graph.ClearValues();
  });

  // ── Multi-series ───────────────────────────────────────────────────────

  [Fact]
  public void NewGraph_HasOneSeries() => StaRunner.Run(() => {
    Assert.Equal(1, new PerformanceGraph().SeriesCount);
  });

  [Fact]
  public void AddSeries_ReturnsIncrementingIndices_AndBumpsCount() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();

    // Index 0 is the primary, so the first overlay is 1.
    Assert.Equal(1, graph.AddSeries(Brushes.Red));
    Assert.Equal(2, graph.AddSeries(Brushes.Green));
    Assert.Equal(3, graph.SeriesCount);
  });

  [Fact]
  public void AddValue_ToOverlaySeries_DoesNotThrow() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();
    int write = graph.AddSeries(Brushes.Green);

    graph.AddValue(10);          // primary
    graph.AddValue(write, 20);   // overlay
  });

  [Fact]
  public void ClearValues_ClearsEverySeries() => StaRunner.Run(() => {
    var green = Color.FromRgb(0x20, 0xFF, 0x20);
    var graph = new PerformanceGraph { GridBrush = Brushes.Black, LineThickness = 3 };
    int write = graph.AddSeries(new SolidColorBrush(green), thickness: 3);
    graph.ApplyVisibility(true);
    for (int i = 0; i < 10; i++) { graph.AddValue(30); graph.AddValue(write, 80); }
    Assert.True(new PixelRenderer(graph, 240, 120).CountColor(green) > 0, "overlay should paint before clear");

    graph.ClearValues();

    Assert.Equal(0, new PixelRenderer(graph, 240, 120).CountColor(green));
  });

  [Fact]
  public void ApplyTheme_UpdatesVisualProperties() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();
    GraphTheme theme = GraphThemes.Emerald(GraphKind.Line);

    graph.ApplyTheme(theme);

    Assert.Same(theme.LineBrush, graph.LineBrush);
    Assert.Same(theme.FillBrush, graph.FillBrush);
    Assert.Same(theme.GridBrush, graph.GridBrush);
    Assert.Same(theme.BorderBrush, graph.BorderBrush);
    Assert.Same(theme.GraphBackground, graph.GraphBackground);
    Assert.Equal(theme.LineThickness, graph.LineThickness);
  });

  [Fact]
  public void ApplyTheme_Null_IsNoOp() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();
    Brush before = graph.LineBrush;

    graph.ApplyTheme(null!);

    Assert.Same(before, graph.LineBrush);
  });

  [Fact]
  public void ApplyTheme_LeavesUnsetPropertiesUntouched() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();
    Brush originalBackground = graph.GraphBackground;
    var partialTheme = new GraphTheme { LineBrush = Brushes.Red };

    graph.ApplyTheme(partialTheme);

    Assert.Same(Brushes.Red, graph.LineBrush);
    Assert.Same(originalBackground, graph.GraphBackground);
  });

  // ── Off-screen render suspension ───────────────────────────────────────
  // A never-shown control starts life not connected to a visible tree, so IsVisible is false and
  // rendering begins suspended — exactly the state these tests assert against.

  [Fact]
  public void NewGraph_StartsRenderSuspended_WhileOffScreen() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();

    Assert.True(graph.RenderSuspended);
    Assert.False(graph.HasPendingRender);
  });

  [Fact]
  public void AddValue_WhileSuspended_DefersRenderInsteadOfDropping() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();

    graph.AddValue(42);

    // The sample is still buffered (no data gap), but the repaint is only owed, not issued.
    Assert.True(graph.HasPendingRender);
  });

  [Fact]
  public void BecomingVisible_FlushesTheDeferredRender() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();
    graph.AddValue(42);
    Assert.True(graph.HasPendingRender);

    graph.ApplyVisibility(true);

    // The owed repaint is consumed on the transition to visible.
    Assert.False(graph.RenderSuspended);
    Assert.False(graph.HasPendingRender);
  });

  [Fact]
  public void BecomingVisible_WithNoBufferedSamples_HasNothingToFlush() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();

    graph.ApplyVisibility(true);

    Assert.False(graph.RenderSuspended);
    Assert.False(graph.HasPendingRender);
  });

  [Fact]
  public void AddValue_WhileVisible_DoesNotAccumulatePendingRender() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();
    graph.ApplyVisibility(true);

    graph.AddValue(42);

    // Visible: the repaint goes out immediately, so nothing is left owed.
    Assert.False(graph.HasPendingRender);
  });

  [Fact]
  public void GoingBackOffScreen_ReSuspendsRendering() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();
    graph.ApplyVisibility(true);
    Assert.False(graph.RenderSuspended);

    graph.ApplyVisibility(false);

    Assert.True(graph.RenderSuspended);
  });
}
