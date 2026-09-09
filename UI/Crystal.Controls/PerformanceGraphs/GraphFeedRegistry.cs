using System;
using System.Collections.Generic;

namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// Routes each sensor sample to every live graph registered under a string id. The same id is
/// commonly held by more than one graph at once — a summary tile and an open detail window both tag
/// their utilization graph "Cpu.Utilization" / "Gpu.Utilization", for instance — so a plain one-slot
/// map would let the last registration starve the earlier one (its graph freezes until its view
/// happens to reload). Graphs are held weakly, as <see cref="GraphIdentity"/> does, so a graph in a
/// closed detail window or a replaced view is collected rather than fed forever; dead entries are
/// pruned on attach and on feed.
/// </summary>
public sealed class GraphFeedRegistry {
  private readonly Dictionary<string, List<WeakReference<ISingleSeriesGraph>>> _byId = [];

  /// <summary>Registers a graph under an id. Idempotent: re-attaching the same instance is a no-op,
  /// so a repeated Loaded event does not double-feed the graph.</summary>
  public void Attach(string id, ISingleSeriesGraph graph) {
    if (!_byId.TryGetValue(id, out var list)) _byId[id] = list = [];
    list.RemoveAll(w => !w.TryGetTarget(out var g) || ReferenceEquals(g, graph));
    list.Add(new WeakReference<ISingleSeriesGraph>(graph));
  }

  /// <summary>Appends a sample to every live graph registered under the id, pruning collected ones.</summary>
  public void Feed(string id, double value) {
    if (!_byId.TryGetValue(id, out var list)) return;
    for (var i = list.Count - 1; i >= 0; i--) {
      if (list[i].TryGetTarget(out var graph)) graph.AddValue(value);
      else list.RemoveAt(i);
    }
  }
}
