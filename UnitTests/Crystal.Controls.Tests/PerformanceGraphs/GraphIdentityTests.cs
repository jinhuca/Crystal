using Crystal.Controls.PerformanceGraphs;
using System.Collections.Generic;
using Xunit;

namespace Crystal.Controls.Tests.PerformanceGraphs;

public class GraphIdentityTests {
  [Fact]
  public void SettingId_RaisesGraphRegistered_WithTheTaggedGraph() => StaRunner.Run(() => {
    var raised = new List<PerformanceGraph>();
    void Handler(PerformanceGraph g) => raised.Add(g);
    GraphIdentity.GraphRegistered += Handler;
    try {
      var graph = new PerformanceGraph();
      GraphIdentity.SetId(graph, "Cpu.Utilization");

      Assert.Contains(graph, raised);
      Assert.Equal("Cpu.Utilization", GraphIdentity.GetId(graph));
    } finally {
      GraphIdentity.GraphRegistered -= Handler;
    }
  });

  [Fact]
  public void SettingEmptyOrNullId_DoesNotRegister() => StaRunner.Run(() => {
    var raised = new List<PerformanceGraph>();
    void Handler(PerformanceGraph g) => raised.Add(g);
    GraphIdentity.GraphRegistered += Handler;
    try {
      var graph = new PerformanceGraph();
      GraphIdentity.SetId(graph, "");
      GraphIdentity.SetId(graph, null);

      Assert.Empty(raised);
    } finally {
      GraphIdentity.GraphRegistered -= Handler;
    }
  });

  [Fact]
  public void LiveGraphs_IncludesTaggedGraph() => StaRunner.Run(() => {
    var graph = new PerformanceGraph();
    GraphIdentity.SetId(graph, "Cpu.Temperature");

    Assert.Contains(graph, GraphIdentity.LiveGraphs());
  });
}
