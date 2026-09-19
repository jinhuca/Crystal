namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// The common surface every performance-graph control exposes, so a consumer - the shell's
/// graph-settings registry (<see cref="GraphIdentity"/>), or a module view model that holds and
/// feeds a graph - can address any of them uniformly rather than being hard-wired to one concrete
/// type. <see cref="PerformanceGraph"/>, <see cref="PerformanceGraphLite"/> and
/// <see cref="PerformanceGraphMultipleDS"/> all implement it, so any of the three can be dropped
/// into a module view where a <see cref="PerformanceGraph"/> used to be.
/// </summary>
public interface IPerformanceGraph {
  /// <summary>Value mapped to the bottom edge of the plot.</summary>
  double MinValue { get; set; }

  /// <summary>Value mapped to the top edge of the plot.</summary>
  double MaxValue { get; set; }
}
