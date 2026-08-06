using Crystal.Controls.PerformanceGraphs.Controls;
using Xunit;

namespace Crystal.Controls.UnitTests.PerformanceGraphs;

public class PerformanceGraphViewTests {
  [Fact]
  public void Defaults_MatchDocumentedValues() => StaRunner.Run(() => {
    var view = new PerformanceGraphView();

    Assert.Equal(string.Empty, view.Title);
    Assert.Equal(100.0, view.MaxValue);
    Assert.Equal(0.0, view.MinValue);
    Assert.Equal(60.0, view.MaxTime);
    Assert.Equal(0.0, view.MinTime);
    Assert.Equal("{0}", view.MaxValueFormat);
    Assert.Equal("{0}", view.MaxTimeFormat);
    Assert.Equal("{0}", view.MinTimeFormat);
  });

  [Fact]
  public void Graph_IsNullBeforeTemplateApplied() => StaRunner.Run(() => {
    var view = new PerformanceGraphView();

    Assert.Null(view.Graph);
  });

  [Fact]
  public void SettingScaleProperties_BeforeTemplate_DoesNotThrow() => StaRunner.Run(() => {
    var view = new PerformanceGraphView();

    // The MinValue/MaxValue changed-handlers guard on a null Graph; setting these
    // before the template has produced the graph must be safe.
    view.MinValue = 5;
    view.MaxValue = 50;

    Assert.Equal(5, view.MinValue);
    Assert.Equal(50, view.MaxValue);
  });

  [Fact]
  public void Properties_RoundTrip() => StaRunner.Run(() => {
    var view = new PerformanceGraphView {
      Title = "Voltage",
      MaxValueFormat = "{0}V",
      MaxTime = 30,
      MaxTimeFormat = "{0} seconds"
    };

    Assert.Equal("Voltage", view.Title);
    Assert.Equal("{0}V", view.MaxValueFormat);
    Assert.Equal(30.0, view.MaxTime);
    Assert.Equal("{0} seconds", view.MaxTimeFormat);
  });
}
