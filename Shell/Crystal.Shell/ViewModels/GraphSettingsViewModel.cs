using Crystal.Shell.Settings;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Crystal.Shell.ViewModels;

/// <summary>
/// One selectable colour in a graph row: the accent it represents, the swatch brush to paint, and a
/// two-way <see cref="IsSelected"/> the swatch radio binds to. Selecting one pushes the accent back
/// to the owning row, which clears the other swatches (and any custom colour).
/// </summary>
public sealed class AccentOptionViewModel : BindableBase {
  /// <summary>
  /// The row that owns this accent option.
  /// </summary>
  private readonly GraphRowViewModel _row;

  /// <summary>
  /// True if the swatch is selected; false if not. The row's accent drives the other swatches to
  /// </summary>
  private bool _isSelected;

  /// <summary>
  /// Initializes a new instance of the <see cref="AccentOptionViewModel"/> class.
  /// </summary>
  /// <param name="row">The row that owns this accent option.</param>
  /// <param name="accent">The accent represented by this option.</param>
  public AccentOptionViewModel(GraphRowViewModel row, GraphAccent accent) {
    _row = row;
    Accent = accent;
    Swatch = BrushFor(accent);
  }

  public GraphAccent Accent { get; }
  public Brush Swatch { get; }

  public bool IsSelected {
    get => _isSelected;
    set {
      if (!SetProperty(ref _isSelected, value)) return;
      // Only a user checking a swatch drives the selection; unchecking is the echo of another
      // swatch being picked, so it must not recurse back into the row.
      if (value) _row.Accent = Accent;
    }
  }

  // Refresh the checked state from the row's current accent. A custom colour deselects every
  // predefined swatch, so it wins over the accent match.
  internal void SyncFromRow() => IsSelected = !_row.HasCustomColor && _row.Accent == Accent;

  // The six palette brushes are merged app-wide (GraphPalette.xaml); fall back to a hard-coded
  // colour so the swatch never renders empty if the resource is somehow missing.
  private static Brush BrushFor(GraphAccent accent) {
    string key = accent switch {
      GraphAccent.Grey => "GraphGreyBrush",
      GraphAccent.Rose => "GraphRoseBrush",
      GraphAccent.Emerald => "GraphEmeraldBrush",
      GraphAccent.Sky => "GraphSkyBrush",
      GraphAccent.Amber => "GraphAmberBrush",
      GraphAccent.Purple => "GraphPurpleBrush",
      _ => "GraphGreyBrush",
    };
    if (Application.Current?.Resources[key] is Brush brush) return brush;
    return new SolidColorBrush(accent switch {
      GraphAccent.Rose => Color.FromRgb(0xE8, 0x2A, 0x7A),
      GraphAccent.Emerald => Color.FromRgb(0x3B, 0xD1, 0x5A),
      GraphAccent.Sky => Color.FromRgb(0x3E, 0x9B, 0xE8),
      GraphAccent.Amber => Color.FromRgb(0xE8, 0x9B, 0x2A),
      GraphAccent.Purple => Color.FromRgb(0x9B, 0x5A, 0xE8),
      _ => Color.FromRgb(0x8A, 0x94, 0xA0),
    });
  }
}

/// <summary>
/// One graph's row in the popup: its label plus the current category, kind, history and colour
/// selection. Each is overridable per row and defaults to the general (apply-to-all) choice.
/// </summary>
public sealed class GraphRowViewModel : BindableBase {
  private GraphCategory _category;
  private GraphKindChoice _kind;
  private GraphAccent _accent;
  private Color? _customColor;
  private int _historyLength;

  public GraphRowViewModel(GraphDescriptor descriptor, GraphSetting setting) {
    Id = descriptor.Id;
    Component = descriptor.Component;
    Metric = descriptor.Metric;
    _category = setting.Category;
    _kind = setting.Kind;
    _accent = setting.Accent;
    _customColor = ParseColor(setting.CustomColor);
    _historyLength = setting.HistoryLength;

    var options = new List<AccentOptionViewModel>();
    foreach (GraphAccent accent in System.Enum.GetValues<GraphAccent>())
      options.Add(new AccentOptionViewModel(this, accent));
    AccentOptions = new ReadOnlyCollection<AccentOptionViewModel>(options);
    foreach (var option in AccentOptions) option.SyncFromRow();
  }

  public string Id { get; }
  public string Component { get; }
  public string Metric { get; }

  public GraphCategory Category {
    get => _category;
    set {
      if (!SetProperty(ref _category, value)) return;
      RaisePropertyChanged(nameof(IsNoFrills));
      RaisePropertyChanged(nameof(IsFullGraph));
    }
  }

  /// <summary>True if this graph uses the No-Frills chrome; setting it true selects that category.</summary>
  public bool IsNoFrills {
    get => Category == GraphCategory.NoFrills;
    set { if (value) Category = GraphCategory.NoFrills; }
  }

  /// <summary>True if this graph uses the Full-Graph chrome; setting it true selects that category.</summary>
  public bool IsFullGraph {
    get => Category == GraphCategory.FullGraph;
    set { if (value) Category = GraphCategory.FullGraph; }
  }

  public GraphKindChoice Kind {
    get => _kind;
    set {
      if (!SetProperty(ref _kind, value)) return;
      RaisePropertyChanged(nameof(IsSegmentedBar));
      RaisePropertyChanged(nameof(IsFilledLine));
      RaisePropertyChanged(nameof(IsDot));
    }
  }

  public GraphAccent Accent {
    get => _accent;
    set {
      bool changed = SetProperty(ref _accent, value);
      // Choosing a predefined accent always clears any custom colour.
      if (_customColor is not null) {
        _customColor = null;
        RaisePropertyChanged(nameof(HasCustomColor));
        RaisePropertyChanged(nameof(CustomSwatch));
        changed = true;
      }
      if (changed) foreach (var option in AccentOptions) option.SyncFromRow();
    }
  }

  /// <summary>
  /// The user-picked colour, or null to use the predefined <see cref="Accent"/>. Setting a colour
  /// deselects every predefined swatch; clearing it restores the accent selection.
  /// </summary>
  public Color? CustomColor {
    get => _customColor;
    set {
      if (!SetProperty(ref _customColor, value)) return;
      RaisePropertyChanged(nameof(HasCustomColor));
      RaisePropertyChanged(nameof(CustomSwatch));
      foreach (var option in AccentOptions) option.SyncFromRow();
    }
  }

  /// <summary>True when a custom colour is set (drives the selection ring on the custom swatch).</summary>
  public bool HasCustomColor => _customColor is not null;

  /// <summary>Brush painting the custom-colour swatch; transparent when no custom colour is set.</summary>
  public Brush CustomSwatch => _customColor is Color c ? new SolidColorBrush(c) : Brushes.Transparent;

  /// <summary>
  /// True if the graph kind is SegmentedBar; false if not. Setting to true changes the kind to SegmentedBar.
  /// </summary>
  public bool IsSegmentedBar {
    get => Kind == GraphKindChoice.SegmentedBar;
    set { if (value) Kind = GraphKindChoice.SegmentedBar; }
  }

  /// <summary>
  /// True if the graph kind is FilledLine; false if not. Setting to true changes the kind to FilledLine.
  /// </summary>
  public bool IsFilledLine {
    get => Kind == GraphKindChoice.FilledLine;
    set { if (value) Kind = GraphKindChoice.FilledLine; }
  }

  /// <summary>
  /// True if the graph kind is Dot; false if not. Setting to true changes the kind to Dot.
  /// </summary>
  public bool IsDot {
    get => Kind == GraphKindChoice.Dot;
    set { if (value) Kind = GraphKindChoice.Dot; }
  }

  /// <summary>
  /// The number of historical data points to show in the graph.
  /// This is a user-editable integer, and changing it will update the graph's display accordingly.
  /// </summary>
  public int HistoryLength {
    get => _historyLength;
    set => SetProperty(ref _historyLength, value);
  }

  /// <summary>
  /// The list of accent options available for this graph row. Each option represents a selectable accent color.
  /// </summary>
  public IReadOnlyList<AccentOptionViewModel> AccentOptions { get; }

  /// <summary>
  /// Converts the current state of this view model into a <see cref="GraphSetting"/> object, which can be
  /// persisted or used for further processing.
  /// </summary>
  /// <returns>GraphSetting</returns>
  public GraphSetting ToSetting() => new() {
    Category = Category,
    Kind = Kind,
    Accent = Accent,
    CustomColor = ToHex(_customColor),
    HistoryLength = HistoryLength,
  };

  private static Color? ParseColor(string? hex) {
    if (string.IsNullOrWhiteSpace(hex)) return null;
    try { return (Color)ColorConverter.ConvertFromString(hex); }
    catch { return null; }
  }

  private static string? ToHex(Color? color) =>
      color is Color c ? $"#{c.R:X2}{c.G:X2}{c.B:X2}" : null;
}

/// <summary>
/// One component's block in the popup: a header (e.g. "CPU") and its graph rows.
/// </summary>
public sealed class GraphGroupViewModel {
  public GraphGroupViewModel(string component, IReadOnlyList<GraphRowViewModel> rows) {
    Component = component;
    Rows = rows;
  }

  public string Component { get; }
  public IReadOnlyList<GraphRowViewModel> Rows { get; }
}

/// <summary>
/// The popup's root view-model: the general (apply-to-all) category / kind / history choices, and a
/// row per configured graph (grouped by component for display). Changing a general choice pushes it
/// to every row; each row can then be overridden individually. Built from the persisted
/// <see cref="GraphSettings"/> and converted back on save.
/// </summary>
public sealed class GraphSettingsViewModel : BindableBase {
  private GraphCategory _category;
  private GraphKindChoice _kind;
  private int _historyLength;
  private CoreBarStyle _coreBarStyle;
  private CoreBarColor _coreBarColor;

  public GraphSettingsViewModel(GraphSettings settings) {
    var rows = new List<GraphRowViewModel>();
    foreach (var descriptor in GraphCatalog.Graphs) {
      settings.Graphs.TryGetValue(descriptor.Id, out var setting);
      rows.Add(new GraphRowViewModel(descriptor, setting ?? GraphCatalog.DefaultFor(descriptor.Id)));
    }
    Graphs = new ReadOnlyCollection<GraphRowViewModel>(rows);

    // Seed the general selectors to the current shared state: use a uniform value where every row
    // agrees, else the persisted category / a sensible default. These are only the apply-to-all
    // starting positions; the per-row values remain authoritative. Set the backing fields directly
    // so seeding does not propagate back over the freshly-loaded rows.
    _category = settings.Category;
    _kind = rows.Count > 0 && rows.All(r => r.Kind == rows[0].Kind) ? rows[0].Kind : GraphDefaults.Kind;
    _historyLength = rows.Count > 0 && rows.All(r => r.HistoryLength == rows[0].HistoryLength)
        ? rows[0].HistoryLength : GraphDefaults.HistoryLength;
    _coreBarStyle = settings.CoreBarStyle;
    _coreBarColor = settings.CoreBarColor;

    // Group by component while preserving first-seen order, so the popup shows one header block per
    // component in catalog order.
    var groups = new List<GraphGroupViewModel>();
    var byComponent = new Dictionary<string, List<GraphRowViewModel>>();
    foreach (var row in rows) {
      if (!byComponent.TryGetValue(row.Component, out var list)) {
        list = new List<GraphRowViewModel>();
        byComponent[row.Component] = list;
        groups.Add(new GraphGroupViewModel(row.Component, list));
      }
      list.Add(row);
    }
    Groups = new ReadOnlyCollection<GraphGroupViewModel>(groups);

    ResetCommand = new DelegateCommand(Reset);
  }

  /// <summary>General category; selecting one applies it to every graph row.</summary>
  public GraphCategory Category {
    get => _category;
    set {
      if (!SetProperty(ref _category, value)) return;
      RaisePropertyChanged(nameof(IsNoFrills));
      RaisePropertyChanged(nameof(IsFullGraph));
      foreach (var row in Graphs) row.Category = value;
    }
  }

  public bool IsNoFrills {
    get => Category == GraphCategory.NoFrills;
    set { if (value) Category = GraphCategory.NoFrills; }
  }

  public bool IsFullGraph {
    get => Category == GraphCategory.FullGraph;
    set { if (value) Category = GraphCategory.FullGraph; }
  }

  /// <summary>General graph kind; selecting one applies it to every graph row.</summary>
  public GraphKindChoice Kind {
    get => _kind;
    set {
      if (!SetProperty(ref _kind, value)) return;
      RaisePropertyChanged(nameof(IsSegmentedBar));
      RaisePropertyChanged(nameof(IsFilledLine));
      RaisePropertyChanged(nameof(IsDot));
      foreach (var row in Graphs) row.Kind = value;
    }
  }

  public bool IsSegmentedBar {
    get => Kind == GraphKindChoice.SegmentedBar;
    set { if (value) Kind = GraphKindChoice.SegmentedBar; }
  }

  public bool IsFilledLine {
    get => Kind == GraphKindChoice.FilledLine;
    set { if (value) Kind = GraphKindChoice.FilledLine; }
  }

  public bool IsDot {
    get => Kind == GraphKindChoice.Dot;
    set { if (value) Kind = GraphKindChoice.Dot; }
  }

  /// <summary>
  /// General history length. A valid value (1–240) is applied to every graph row; out-of-range input
  /// is kept in the box but not propagated, so a stray keystroke never rewrites every graph.
  /// </summary>
  public int HistoryLength {
    get => _historyLength;
    set {
      if (!SetProperty(ref _historyLength, value)) return;
      if (value is >= 1 and <= 240) foreach (var row in Graphs) row.HistoryLength = value;
    }
  }

  /// <summary>Global bar shape for the CPU core strip (solid bar vs segmented LED-meter).</summary>
  public CoreBarStyle CoreBarStyle {
    get => _coreBarStyle;
    set {
      if (!SetProperty(ref _coreBarStyle, value)) return;
      RaisePropertyChanged(nameof(IsCoreBar));
      RaisePropertyChanged(nameof(IsCoreSegmentedBar));
    }
  }

  public bool IsCoreBar {
    get => CoreBarStyle == CoreBarStyle.Bar;
    set { if (value) CoreBarStyle = CoreBarStyle.Bar; }
  }

  public bool IsCoreSegmentedBar {
    get => CoreBarStyle == CoreBarStyle.SegmentedBar;
    set { if (value) CoreBarStyle = CoreBarStyle.SegmentedBar; }
  }

  /// <summary>Global colouring for the CPU core strip (per-metric colours vs uniform grey).</summary>
  public CoreBarColor CoreBarColor {
    get => _coreBarColor;
    set {
      if (!SetProperty(ref _coreBarColor, value)) return;
      RaisePropertyChanged(nameof(IsCoreColorful));
      RaisePropertyChanged(nameof(IsCoreGrey));
    }
  }

  public bool IsCoreColorful {
    get => CoreBarColor == CoreBarColor.Colorful;
    set { if (value) CoreBarColor = CoreBarColor.Colorful; }
  }

  public bool IsCoreGrey {
    get => CoreBarColor == CoreBarColor.Grey;
    set { if (value) CoreBarColor = CoreBarColor.Grey; }
  }

  public IReadOnlyList<GraphRowViewModel> Graphs { get; }
  public IReadOnlyList<GraphGroupViewModel> Groups { get; }

  /// <summary>Resets the general selectors and every row to the factory defaults.</summary>
  public ICommand ResetCommand { get; }

  private void Reset() {
    // Update the general selectors (raises their bound properties) …
    Category = GraphDefaults.Category;
    Kind = GraphDefaults.Kind;
    HistoryLength = GraphDefaults.HistoryLength;
    CoreBarStyle = GraphDefaults.CoreBarStyle;
    CoreBarColor = GraphDefaults.CoreBarColor;
    // … then force every row, including any it did not touch because a general value was unchanged.
    foreach (var row in Graphs) {
      row.Category = GraphDefaults.Category;
      row.Kind = GraphDefaults.Kind;
      row.HistoryLength = GraphDefaults.HistoryLength;
      row.CustomColor = null;
      row.Accent = GraphDefaults.Accent;
    }
  }

  public GraphSettings ToSettings() {
    var settings = new GraphSettings {
      Category = Category,
      CoreBarStyle = CoreBarStyle,
      CoreBarColor = CoreBarColor,
    };
    foreach (var row in Graphs) settings.Graphs[row.Id] = row.ToSetting();
    return settings;
  }
}
