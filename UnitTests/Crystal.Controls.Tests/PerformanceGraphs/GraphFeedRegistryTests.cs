using System.Collections.Generic;
using Crystal.Controls.PerformanceGraphs;
using Xunit;

namespace Crystal.Controls.Tests.PerformanceGraphs;

public class GraphFeedRegistryTests {
  private sealed class FakeGraph : ISingleSeriesGraph {
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public List<double> Fed { get; } = [];
    public void AddValue(double value) => Fed.Add(value);
  }

  [Fact]
  public void Feed_ReachesEveryGraphAttachedUnderTheSameId() {
    var registry = new GraphFeedRegistry();
    var summary = new FakeGraph();
    var detail = new FakeGraph();
    registry.Attach("Cpu.Utilization", summary);
    registry.Attach("Cpu.Utilization", detail);

    registry.Feed("Cpu.Utilization", 42);

    Assert.Equal([42], summary.Fed);
    Assert.Equal([42], detail.Fed);
  }

  [Fact]
  public void Attach_IsIdempotentForTheSameInstance() {
    var registry = new GraphFeedRegistry();
    var graph = new FakeGraph();
    registry.Attach("Gpu.Utilization", graph);
    registry.Attach("Gpu.Utilization", graph);

    registry.Feed("Gpu.Utilization", 7);

    Assert.Equal([7], graph.Fed);
  }

  [Fact]
  public void Feed_ForUnknownId_IsNoOp() {
    var registry = new GraphFeedRegistry();

    registry.Feed("Nope", 1);
  }
}
