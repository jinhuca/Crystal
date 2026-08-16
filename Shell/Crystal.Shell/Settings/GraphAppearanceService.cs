using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Shell.Settings;

/// <summary>
/// Applies the persisted graph-appearance selection to the live dashboard graphs. Graphs opt in by
/// tagging themselves with <see cref="GraphIdentity"/>.<c>Id</c> in XAML; this service listens for
/// those registrations and re-applies whenever the settings are saved, so a change takes effect
/// immediately and is reproduced on the next launch. App-lifetime singleton, resolved eagerly so it is
/// subscribed before any tile's graphs are realized.
/// </summary>
public sealed class GraphAppearanceService {
  private readonly GraphSettingsStore _store;

  public GraphAppearanceService(GraphSettingsStore store) {
    _store = store;

    // Newly-tagged graphs apply as they appear; a Save re-applies to every live graph.
    GraphIdentity.GraphRegistered += Apply;
    _store.Changed += ApplyToAll;

    // Anything already tagged before we subscribed (defensive; normally none yet).
    ApplyToAll();
  }

  private void ApplyToAll() {
    foreach (var graph in GraphIdentity.LiveGraphs()) Apply(graph);
  }

  private void Apply(PerformanceGraph graph) {
    // Property mutation must run on the graph's dispatcher; registrations and saves both originate on
    // the UI thread, but marshal defensively in case that ever changes.
    if (!graph.CheckAccess()) {
      graph.Dispatcher.BeginInvoke(() => Apply(graph));
      return;
    }

    var id = GraphIdentity.GetId(graph);
    if (string.IsNullOrEmpty(id)) return;

    var settings = _store.Current;
    settings.Graphs.TryGetValue(id, out var setting);
    setting ??= new GraphSetting();

    ApplyCategory(graph, settings.Category);
    ApplyKindAndAccent(graph, setting.Kind, setting.Accent);
    if (setting.HistoryLength > 0) graph.HistoryLength = setting.HistoryLength;
  }

  // Chrome only (border / grid / background) — the two selectable categories.
  private static void ApplyCategory(PerformanceGraph graph, GraphCategory category) {
    if (category == GraphCategory.FullGraph) {
      graph.GraphBackground = Res("GraphPlotBackgroundBrush") ?? Brushes.Black;
      graph.GridBrush = Res("GraphGridBrush") ?? new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30));
      graph.GridThickness = 0.6;
      graph.BorderBrush = Res("GraphAxisBorderBrush") ?? new SolidColorBrush(Color.FromRgb(0x3E, 0x7B, 0xC4));
      graph.BorderThickness = 0.8;
    } else {
      graph.GraphBackground = Brushes.Transparent;
      graph.GridBrush = Brushes.Transparent;
      graph.GridThickness = 0;
      graph.BorderThickness = 0;
    }
  }

  private static void ApplyKindAndAccent(PerformanceGraph graph, GraphKindChoice kind, GraphAccent accent) {
    var flat = AccentBrush(accent);
    if (kind == GraphKindChoice.FilledLine) {
      graph.Kind = GraphKind.Line;
      graph.LineBrush = flat;
      graph.LineThickness = 1.0;
      graph.FillBrush = AccentFill(accent) ?? flat;
    } else {
      graph.Kind = GraphKind.SegmentedBar;
      graph.LineBrush = flat;
      graph.FillBrush = flat;
    }
  }

  private static Brush? Res(string key) => Application.Current?.Resources[key] as Brush;

  // Flat solid accent brush, matching GraphPalette.xaml; hard-coded fallback keeps the graph coloured
  // if the resource is somehow missing.
  private static Brush AccentBrush(GraphAccent accent) {
    if (Res(accent switch {
      GraphAccent.Rose => "GraphRoseBrush",
      GraphAccent.Emerald => "GraphEmeraldBrush",
      GraphAccent.Sky => "GraphSkyBrush",
      GraphAccent.Amber => "GraphAmberBrush",
      GraphAccent.Purple => "GraphPurpleBrush",
      _ => "GraphGreyBrush",
    }) is { } brush) return brush;

    return new SolidColorBrush(accent switch {
      GraphAccent.Rose => Color.FromRgb(0xE8, 0x2A, 0x7A),
      GraphAccent.Emerald => Color.FromRgb(0x3B, 0xD1, 0x5A),
      GraphAccent.Sky => Color.FromRgb(0x3E, 0x9B, 0xE8),
      GraphAccent.Amber => Color.FromRgb(0xE8, 0x9B, 0x2A),
      GraphAccent.Purple => Color.FromRgb(0x9B, 0x5A, 0xE8),
      _ => Color.FromRgb(0x8A, 0x94, 0xA0),
    });
  }

  // Vertical glow-gradient fill for line kinds.
  private static Brush? AccentFill(GraphAccent accent) => Res(accent switch {
    GraphAccent.Rose => "GraphRoseFill",
    GraphAccent.Emerald => "GraphEmeraldFill",
    GraphAccent.Sky => "GraphSkyFill",
    GraphAccent.Amber => "GraphAmberFill",
    GraphAccent.Purple => "GraphPurpleFill",
    GraphAccent.Grey => "GraphGreyFill",
    _ => "",
  });
}
