namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// The common surface a performance-graph control exposes, so a consumer - the shell's
/// graph-settings registry (<see cref="GraphIdentity"/>), or a module view model that holds and
/// feeds a graph - can address it through an interface rather than being hard-wired to the concrete
/// type. Implemented by <see cref="PerformanceGraph"/>.
/// </summary>
public interface IPerformanceGraph {
  /// <summary>Value mapped to the bottom edge of the plot.</summary>
  double MinValue { get; set; }

  /// <summary>Value mapped to the top edge of the plot.</summary>
  double MaxValue { get; set; }
}
