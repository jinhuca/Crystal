using Crystal.Shell.Settings;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Shell.ViewModels;

/// <summary>
/// One selectable colour in a graph row: the accent it represents, the swatch brush to paint, and a
/// two-way <see cref="IsSelected"/> the swatch radio binds to. Selecting one pushes the accent back
/// to the owning row, which clears the other swatches.
/// </summary>
public sealed class AccentOptionViewModel : BindableBase {
  private readonly GraphRowViewModel _row;
  private bool _isSelected;

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

  // Refresh the checked state from the row's current accent (called when the row's accent changes).
  internal void SyncFromRow() => IsSelected = _row.Accent == Accent;

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
/// One graph's row in the popup: its label plus the current kind and accent selection. Kind is two
/// radio buttons bound to <see cref="IsSegmentedBar"/>/<see cref="IsFilledLine"/>; accent is the six
/// <see cref="AccentOptions"/> swatches.
/// </summary>
public sealed class GraphRowViewModel : BindableBase {
  private GraphKindChoice _kind;
  private GraphAccent _accent;
  private int _historyLength;

  public GraphRowViewModel(GraphDescriptor descriptor, GraphSetting setting) {
    Id = descriptor.Id;
    Component = descriptor.Component;
    Metric = descriptor.Metric;
    _kind = setting.Kind;
    _accent = setting.Accent;
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

  public GraphKindChoice Kind {
    get => _kind;
    set {
      if (!SetProperty(ref _kind, value)) return;
      RaisePropertyChanged(nameof(IsSegmentedBar));
      RaisePropertyChanged(nameof(IsFilledLine));
    }
  }

  public GraphAccent Accent {
    get => _accent;
    set {
      if (!SetProperty(ref _accent, value)) return;
      foreach (var option in AccentOptions) option.SyncFromRow();
    }
  }

  // Two-way bool facets of Kind for the kind radio buttons. Only the transition to true drives the
  // change; the false echo (the other radio) is ignored so exclusivity stays data-driven.
  public bool IsSegmentedBar {
    get => Kind == GraphKindChoice.SegmentedBar;
    set { if (value) Kind = GraphKindChoice.SegmentedBar; }
  }

  public bool IsFilledLine {
    get => Kind == GraphKindChoice.FilledLine;
    set { if (value) Kind = GraphKindChoice.FilledLine; }
  }

  public int HistoryLength {
    get => _historyLength;
    set => SetProperty(ref _historyLength, value);
  }

  public IReadOnlyList<AccentOptionViewModel> AccentOptions { get; }

  public GraphSetting ToSetting() => new() { Kind = Kind, Accent = Accent, HistoryLength = HistoryLength };
}

/// <summary>One component's block in the popup: a header (e.g. "CPU") and its graph rows.</summary>
public sealed class GraphGroupViewModel {
  public GraphGroupViewModel(string component, IReadOnlyList<GraphRowViewModel> rows) {
    Component = component;
    Rows = rows;
  }

  public string Component { get; }
  public IReadOnlyList<GraphRowViewModel> Rows { get; }
}

/// <summary>
/// The popup's root view-model: the active category shared by every graph, and a row per configured
/// graph (grouped by component for display). Built from the persisted <see cref="GraphSettings"/>
/// and converted back on save.
/// </summary>
public sealed class GraphSettingsViewModel : BindableBase {
  private GraphCategory _category;

  public GraphSettingsViewModel(GraphSettings settings) {
    _category = settings.Category;

    var rows = new List<GraphRowViewModel>();
    foreach (var descriptor in GraphCatalog.Graphs) {
      settings.Graphs.TryGetValue(descriptor.Id, out var setting);
      rows.Add(new GraphRowViewModel(descriptor, setting ?? new GraphSetting()));
    }
    Graphs = new ReadOnlyCollection<GraphRowViewModel>(rows);

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
  }

  public GraphCategory Category {
    get => _category;
    set {
      if (!SetProperty(ref _category, value)) return;
      RaisePropertyChanged(nameof(IsNoFrills));
      RaisePropertyChanged(nameof(IsFullGraph));
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

  public IReadOnlyList<GraphRowViewModel> Graphs { get; }
  public IReadOnlyList<GraphGroupViewModel> Groups { get; }

  public GraphSettings ToSettings() {
    var settings = new GraphSettings { Category = Category };
    foreach (var row in Graphs) settings.Graphs[row.Id] = row.ToSetting();
    return settings;
  }
}
