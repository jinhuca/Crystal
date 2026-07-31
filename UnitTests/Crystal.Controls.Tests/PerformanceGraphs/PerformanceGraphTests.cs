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
}
