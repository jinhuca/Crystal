using Crystal.Controls.Meters;
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
    _store.Changed += OnChanged;

    // Anything already tagged before we subscribed (defensive; normally none yet).
    ApplyCoreBars();
    ApplyToAll();
  }

  private void OnChanged() {
    ApplyCoreBars();
    ApplyToAll();
  }

  private void ApplyToAll() {
    foreach (var graph in GraphIdentity.LiveGraphs()) Apply(graph);
  }

  // The CPU core strip binds to CoreBarAppearance across the module boundary; push the global
  // selection there so it takes effect immediately and is reproduced on the next launch. Both this
  // ctor call and store saves originate on the UI thread, so the bound bars update in place.
  private void ApplyCoreBars() {
    var settings = _store.Current;
    CoreBarAppearance.Current.Segmented = settings.CoreBarStyle == CoreBarStyle.SegmentedBar;
    CoreBarAppearance.Current.Monochrome = settings.CoreBarColor == CoreBarColor.Grey;
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
    // No saved entry → the graph's built-in default look (kind/accent), so a graph newly brought
    // under the settings system keeps its original appearance until the user changes it.
    setting ??= GraphCatalog.DefaultFor(id);

    ApplyCategory(graph, setting.Category);
    ApplyKindAndAccent(graph, setting);
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

  private static void ApplyKindAndAccent(PerformanceGraph graph, GraphSetting setting) {
    // A custom colour overrides the predefined accent everywhere the graph is coloured.
    var custom = ParseColor(setting.CustomColor);
    var flat = custom is Color c ? new SolidColorBrush(c) : AccentBrush(setting.Accent);
    if (setting.Kind == GraphKindChoice.FilledLine) {
      graph.Kind = GraphKind.Line;
      graph.LineBrush = flat;
      graph.LineThickness = 1.0;
      graph.FillBrush = custom is Color cf ? CustomFill(cf) : (AccentFill(setting.Accent) ?? flat);
    } else {
      // Both discrete kinds draw many separate shapes, so a flat accent fill (no glow gradient).
      graph.Kind = setting.Kind == GraphKindChoice.Dot ? GraphKind.Dot : GraphKind.SegmentedBar;
      graph.LineBrush = flat;
      graph.FillBrush = flat;
    }
  }

  private static Color? ParseColor(string? hex) {
    if (string.IsNullOrWhiteSpace(hex)) return null;
    try { return (Color)ColorConverter.ConvertFromString(hex); }
    catch { return null; }
  }

  // Vertical glow-gradient fill for a custom line colour, mirroring the predefined *Fill brushes.
  private static Brush CustomFill(Color c) => new LinearGradientBrush(
      Color.FromArgb(0x66, c.R, c.G, c.B), Color.FromArgb(0x00, c.R, c.G, c.B), 90);

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
