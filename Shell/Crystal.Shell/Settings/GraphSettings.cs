using System.Collections.Generic;

namespace Crystal.Shell.Settings;

/// <summary>
/// The two selectable graph "looks" surfaced by the graph-settings popup. A category fixes only
/// the chrome (border / grid / background); it maps to the partial base styles
/// <c>NoFrillsGraphStyle</c> / <c>FullGraphStyle</c> in PerformanceGraphSampleStyles.xaml.
/// </summary>
public enum GraphCategory {
  NoFrills,
  FullGraph,
}

/// <summary>
/// The graph shapes the user can pick per graph. These map onto the control's Kind:
/// <see cref="SegmentedBar"/> → <c>SegmentedBar</c>, <see cref="FilledLine"/> → <c>Line</c> (with a
/// gradient fill). Bar (non-segmented) is intentionally not offered.
/// </summary>
public enum GraphKindChoice {
  SegmentedBar,
  FilledLine,
}

/// <summary>
/// The six predefined accent colours, matching the brushes in GraphPalette.xaml
/// (GraphGreyBrush, GraphRoseBrush, …).
/// </summary>
public enum GraphAccent {
  Grey,
  Rose,
  Emerald,
  Sky,
  Amber,
  Purple,
}

/// <summary>
/// The kind + accent chosen for a single dashboard graph, identified elsewhere by a stable graph id.
/// </summary>
public sealed class GraphSetting {
  // Chrome (border / grid / background) for this graph. Per-graph as of the settings redesign; a
  // file that predates this field keeps NoFrills because absent JSON leaves the initializer value.
  public GraphCategory Category { get; set; } = GraphCategory.NoFrills;
  public GraphKindChoice Kind { get; set; } = GraphKindChoice.SegmentedBar;
  public GraphAccent Accent { get; set; } = GraphAccent.Grey;

  // User-picked colour as "#RRGGBB", or null to use the predefined <see cref="Accent"/>. When set it
  // overrides the accent everywhere the graph is coloured.
  public string? CustomColor { get; set; }

  // X-axis span (number of samples plotted). Maps to PerformanceGraph.HistoryLength. Defaults to
  // 30, matching the existing sample styles; a settings file that predates this field keeps 30
  // because absent JSON properties leave the initializer value untouched.
  public int HistoryLength { get; set; } = 30;
}

/// <summary>
/// The factory-reset appearance applied to every graph by the settings popup's reset action:
/// a grey, No-Frills segmented bar with a 20-sample history.
/// </summary>
public static class GraphDefaults {
  public const GraphCategory Category = GraphCategory.NoFrills;
  public const GraphKindChoice Kind = GraphKindChoice.SegmentedBar;
  public const GraphAccent Accent = GraphAccent.Grey;
  public const int HistoryLength = 20;
}

/// <summary>
/// The full persisted graph-appearance selection: one active <see cref="Category"/> shared by every
/// graph, plus per-graph kind/accent overrides keyed by graph id. A graph with no entry falls back
/// to its built-in default style.
/// </summary>
public sealed class GraphSettings {
  public GraphCategory Category { get; set; } = GraphCategory.NoFrills;
  public Dictionary<string, GraphSetting> Graphs { get; set; } = new();
}
