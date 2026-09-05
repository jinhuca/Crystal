using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// A single-series graph host that literally swaps its inner control between a real
/// <see cref="PerformanceGraph"/> (drawn as a filled line) and a real <see cref="PerformanceGraphLite"/>
/// (a dot-matrix gauge) according to the global <see cref="GraphAppearance.Current"/> mode. Every
/// dashboard tile hosts one of these instead of a fixed graph type, so the title-bar Line/Dot toggle
/// re-renders all graphs at once by rebuilding each host's inner control.
///
/// <para>Feeding is control-agnostic: the host implements <see cref="ISingleSeriesGraph"/> and forwards
/// <see cref="AddValue(double)"/> to whichever inner control is live. Overlay series (via
/// <see cref="AddSeries"/> + <see cref="AddValue(int,double)"/>, used by the Storage transfer graph's
/// read/write pair) are supported in Line mode and degrade to the primary series alone in Dot mode,
/// since <see cref="PerformanceGraphLite"/> is single-series.</para>
/// </summary>
/// <remarks>
/// <para><b>Keeping Line and Dot in sync.</b> Configure the host, never the inner controls — this
/// forwards its properties to whichever child is live, so both modes stay in step by construction.
/// Set <see cref="Capacity"/>, <see cref="MinValue"/>/<see cref="MaxValue"/>, <see cref="Accent"/>
/// and <see cref="BandedLine"/> here; height/margin/layout go on the host too, so both modes inherit
/// the same box (height is synced for free).</para>
/// <para>Pick exactly one sizing regime per tile — never set both <see cref="CellPitch"/> and
/// <see cref="DotStyle"/>:
/// <list type="bullet">
/// <item><b>Stretch-to-fill tile</b> (the dashboard default): set <see cref="CellPitch"/> (the shared
/// DashboardDotPitch resource) and let the tile stretch. Both modes then draw the same most-recent
/// width/pitch samples at the same spacing, so window, density and dot size match — across tiles too.
/// Here <see cref="Capacity"/> is only the max history retained (shown = width/pitch), so keep it at
/// least the widest tile's width/pitch.</item>
/// <item><b>Fixed-cell tile</b>: set <see cref="DotStyle"/> (fixed Height + Rows), leave
/// <see cref="CellPitch"/> unset. Dot renders a fixed square-cell grid and Line auto-pins to that
/// footprint, so both occupy identical space.</item>
/// </list>
/// If Line and Dot legitimately need to differ, that's the signal to use the concrete control
/// directly rather than an <see cref="AdaptiveGraph"/>.</para>
/// </remarks>
public sealed class AdaptiveGraph : Decorator, ISingleSeriesGraph {
  // Overlay series registered via AddSeries, replayed onto a freshly-built PerformanceGraph so a
  // mode toggle (or disk-selection template swap) reconstructs the same read/write pair.
  private readonly record struct SeriesDef(Brush LineBrush, Brush? FillBrush, double Thickness);
  private readonly List<SeriesDef> _series = [];

  // Rolling sample history, one list per series (index 0 is the primary). Kept trimmed to Capacity
  // so a mode toggle can replay it into the freshly-built inner control — the graph keeps its trace
  // across the swap instead of restarting from an empty plot. Overlay samples are buffered even in
  // Dot mode (where Lite can't draw them) so switching back to Line restores the full read/write pair.
  private readonly List<List<double>> _history = [[]];

  private FrameworkElement? _inner;

  public AdaptiveGraph() {
    Loaded += OnLoaded;
    Unloaded += OnUnloaded;
  }

  /// <summary>Identifies the <see cref="MinValue"/> dependency property.</summary>
  public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(AdaptiveGraph),
          new FrameworkPropertyMetadata(0.0, OnRangeChanged));

  /// <summary>Identifies the <see cref="MaxValue"/> dependency property.</summary>
  public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(AdaptiveGraph),
          new FrameworkPropertyMetadata(100.0, OnRangeChanged));

  /// <summary>Identifies the <see cref="Capacity"/> dependency property.</summary>
  public static readonly DependencyProperty CapacityProperty =
      DependencyProperty.Register(nameof(Capacity), typeof(int), typeof(AdaptiveGraph),
          new FrameworkPropertyMetadata(60), v => v is int c && c > 0);

  /// <summary>Identifies the <see cref="Accent"/> dependency property.</summary>
  public static readonly DependencyProperty AccentProperty =
      DependencyProperty.Register(nameof(Accent), typeof(Color), typeof(AdaptiveGraph),
          new FrameworkPropertyMetadata(Color.FromRgb(0x3B, 0xD1, 0x5A)));

  /// <summary>Identifies the <see cref="ShowChrome"/> dependency property.</summary>
  public static readonly DependencyProperty ShowChromeProperty =
      DependencyProperty.Register(nameof(ShowChrome), typeof(bool), typeof(AdaptiveGraph),
          new FrameworkPropertyMetadata(false));

  /// <summary>Identifies the <see cref="BandedLine"/> dependency property.</summary>
  public static readonly DependencyProperty BandedLineProperty =
      DependencyProperty.Register(nameof(BandedLine), typeof(bool), typeof(AdaptiveGraph),
          new FrameworkPropertyMetadata(true));

  /// <summary>Identifies the <see cref="DotStyle"/> dependency property.</summary>
  public static readonly DependencyProperty DotStyleProperty =
      DependencyProperty.Register(nameof(DotStyle), typeof(Style), typeof(AdaptiveGraph),
          new FrameworkPropertyMetadata(null));

  /// <summary>Identifies the <see cref="CellPitch"/> dependency property.</summary>
  public static readonly DependencyProperty CellPitchProperty =
      DependencyProperty.Register(nameof(CellPitch), typeof(double), typeof(AdaptiveGraph),
          new FrameworkPropertyMetadata(0.0));

  /// <summary>Value mapped to the bottom edge of the plot (forwarded to the inner control).</summary>
  public double MinValue {
    get => (double)GetValue(MinValueProperty);
    set => SetValue(MinValueProperty, value);
  }

  /// <summary>Value mapped to the top edge of the plot (forwarded to the inner control).</summary>
  public double MaxValue {
    get => (double)GetValue(MaxValueProperty);
    set => SetValue(MaxValueProperty, value);
  }

  /// <summary>Samples retained/plotted across the width.</summary>
  public int Capacity {
    get => (int)GetValue(CapacityProperty);
    set => SetValue(CapacityProperty, value);
  }

  /// <summary>Line colour used in Line mode. Ignored in Dot mode, which uses the banded gauge ramp.</summary>
  public Color Accent {
    get => (Color)GetValue(AccentProperty);
    set => SetValue(AccentProperty, value);
  }

  /// <summary>When true, Line mode paints the full dark background/grid/border chrome; when false it
  /// draws a chrome-less accent line for inline sparklines and tile-embedded graphs.</summary>
  public bool ShowChrome {
    get => (bool)GetValue(ShowChromeProperty);
    set => SetValue(ShowChromeProperty, value);
  }

  /// <summary>When true (the default), Line mode colors the trace by value using the green→red
  /// gauge ramp instead of the flat <see cref="Accent"/>, matching how Dot mode already bands its
  /// dots — so the Line/Dot toggle reads as the same gauge either way. Set false to keep a flat
  /// single-<see cref="Accent"/> line. Has no effect on graphs with overlay series (e.g. a
  /// read/write pair), which stay unbanded so their per-series colors remain distinct.</summary>
  public bool BandedLine {
    get => (bool)GetValue(BandedLineProperty);
    set => SetValue(BandedLineProperty, value);
  }

  /// <summary>Optional style applied to the inner <see cref="PerformanceGraphLite"/> in Dot mode
  /// (e.g. the fixed-cell utilization style). Null lets the dot matrix stretch to fill.</summary>
  public Style? DotStyle {
    get => (Style?)GetValue(DotStyleProperty);
    set => SetValue(DotStyleProperty, value);
  }

  /// <summary>When greater than 0, Dot mode draws dots at this fixed pixel pitch (forwarded to
  /// <see cref="PerformanceGraphLite.CellPitch"/>) while still stretching to fill the tile, so every
  /// dashboard graph sharing one pitch renders dots the same size regardless of width or Capacity.
  /// Takes precedence over <see cref="DotStyle"/>. Line mode is unaffected (it has no dots) and
  /// keeps its full stretch-to-fill footprint, since no fixed-size <see cref="DotStyle"/> is applied.</summary>
  public double CellPitch {
    get => (double)GetValue(CellPitchProperty);
    set => SetValue(CellPitchProperty, value);
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    GraphAppearance.Current.PropertyChanged += OnModeChanged;
    Rebuild();
  }

  private void OnUnloaded(object sender, RoutedEventArgs e) {
    GraphAppearance.Current.PropertyChanged -= OnModeChanged;
  }

  private void OnModeChanged(object? sender, PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(GraphAppearance.Mode)) Rebuild();
  }

  private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (((AdaptiveGraph)d)._inner is IPerformanceGraph graph) {
      graph.MinValue = ((AdaptiveGraph)d).MinValue;
      graph.MaxValue = ((AdaptiveGraph)d).MaxValue;
    }
  }

  // Builds the inner control matching the current global mode, then replays the buffered sample
  // history into it so a mode toggle keeps the existing trace instead of restarting from zero.
  // Registered overlay series are re-added in BuildLine so a read/write pair survives the swap.
  private void Rebuild() {
    _inner = GraphAppearance.Current.Mode == GraphRenderMode.Dot ? BuildDot() : BuildLine();
    Child = _inner;
    ReplayHistory();
  }

  private FrameworkElement BuildLine() {
    var graph = new PerformanceGraph {
      Kind = GraphKind.Line,
      HistoryLength = Capacity,
      MinValue = MinValue,
      MaxValue = MaxValue,
      BandedLine = BandedLine,
      // Match Dot mode's fixed-pitch windowing so toggling keeps the same visible history: with a
      // pitch set, both modes draw the same most-recent width/pitch samples at the same spacing.
      CellPitch = CellPitch,
      HorizontalAlignment = HorizontalAlignment.Stretch,
      VerticalAlignment = VerticalAlignment.Stretch,
    };

    // ApplyTheme still sets the flat accent line/fill; when BandedLine is on, PerformanceGraph
    // ignores those for the primary series and paints the value-banded ramp instead. Applying it
    // regardless keeps the accent as the fallback if banding is turned off at runtime.
    graph.ApplyTheme(GraphThemes.FromAccent(Accent, GraphKind.Line));
    if (!ShowChrome) {
      // Accent line + glow over the tile's own background, no dark backdrop, grid or border —
      // for inline sparklines and tile-embedded graphs that shouldn't carry a framed plot.
      graph.GraphBackground = Brushes.Transparent;
      graph.GridBrush = Brushes.Transparent;
      graph.BorderThickness = 0;
    }

    // Fixed-cell tiles (CPU/GPU) size the Dot control to a square-cell footprint via DotStyle. Left
    // to stretch, the Line control instead measures to Capacity×12px in the horizontal GPU strip — a
    // different width — which reflows the surrounding tile on every toggle. Pin the Line graph to the
    // same footprint so Line and Dot occupy identical space.
    if (TryGetDotFootprint(out double dotWidth, out double dotHeight)) {
      graph.Width = dotWidth;
      graph.Height = dotHeight;
      graph.HorizontalAlignment = HorizontalAlignment.Left;
      graph.VerticalAlignment = VerticalAlignment.Center;
    }

    foreach (var s in _series) graph.AddSeries(s.LineBrush, s.FillBrush, s.Thickness);
    return graph;
  }

  // Reproduces the size a DotStyle-driven PerformanceGraphLite resolves to (SquareDotWidthConverter:
  // fixed Height, Width = Height / SquareDotAspectRatio), reading the Height (and any Rows override)
  // straight from the applied style so it stays in step with UtilizationGraphHeight rather than
  // duplicating the constant. Returns false when no DotStyle is set (the stretch-to-fill graphs),
  // leaving the Line graph to stretch exactly as the Dot graph does in that case.
  private bool TryGetDotFootprint(out double width, out double height) {
    width = height = 0;
    if (DotStyle is not { } style) return false;

    double dotHeight = 0;
    int rows = (int)PerformanceGraphLite.RowsProperty.DefaultMetadata.DefaultValue;
    foreach (var setter in style.Setters.OfType<Setter>()) {
      if (setter.Property == HeightProperty && setter.Value is double h) dotHeight = h;
      else if (setter.Property == PerformanceGraphLite.RowsProperty && setter.Value is int r) rows = r;
    }
    if (dotHeight <= 0) return false;

    height = dotHeight;
    width = Math.Max(20.0, dotHeight / PerformanceGraphLite.SquareDotAspectRatio(rows, Capacity));
    return true;
  }

  private FrameworkElement BuildDot() {
    var lite = new PerformanceGraphLite {
      Capacity = Capacity,
      MinValue = MinValue,
      MaxValue = MaxValue,
    };

    if (CellPitch > 0) {
      // Fixed-pitch dots that still fill the tile: uniform dot size across graphs of differing
      // width/Capacity, without pinning a fixed footprint the way a fixed-Height DotStyle would.
      lite.CellPitch = CellPitch;
      lite.CornerRadius = 1;
      lite.HorizontalAlignment = HorizontalAlignment.Stretch;
      lite.VerticalAlignment = VerticalAlignment.Stretch;
    } else if (DotStyle is { } style) {
      lite.Style = style;
      lite.HorizontalAlignment = HorizontalAlignment.Left;
      lite.VerticalAlignment = VerticalAlignment.Center;
    } else {
      lite.CornerRadius = 1;
      lite.HorizontalAlignment = HorizontalAlignment.Stretch;
      lite.VerticalAlignment = VerticalAlignment.Stretch;
    }

    return lite;
  }

  /// <summary>Registers an overlay line series, returning its feed index (1-based; the primary is 0).
  /// Recorded so it is replayed whenever the inner control is rebuilt. In Dot mode the overlay isn't
  /// drawn (Lite is single-series) but the index stays valid so feeding it is a safe no-op.</summary>
  public int AddSeries(Brush lineBrush, Brush? fillBrush = null, double thickness = 2) {
    _series.Add(new SeriesDef(lineBrush, fillBrush, thickness));
    if (_inner is PerformanceGraph graph) graph.AddSeries(lineBrush, fillBrush, thickness);
    return _series.Count;
  }

  /// <summary>Appends a sample to the primary series (index 0).</summary>
  public void AddValue(double value) => AddValue(0, value);

  /// <summary>Appends a sample to the given series. Series 0 is the primary; 1..N are overlays from
  /// <see cref="AddSeries"/>. In Dot mode only the primary series is plotted, so overlay samples are
  /// dropped from the live control — but still buffered, so switching back to Line restores them.</summary>
  public void AddValue(int series, double value) {
    Record(series, value);
    switch (_inner) {
      case PerformanceGraph graph:
        graph.AddValue(series, value);
        break;
      case PerformanceGraphLite lite when series == 0:
        lite.AddValue(value);
        break;
    }
  }

  // Buffers one sample into the given series' rolling window, trimmed from the front to Capacity so
  // the replayed history never exceeds what the inner control plots across its width.
  private void Record(int series, double value) {
    while (_history.Count <= series) _history.Add([]);
    var buffer = _history[series];
    buffer.Add(value);
    int capacity = Capacity;
    if (buffer.Count > capacity) buffer.RemoveRange(0, buffer.Count - capacity);
  }

  // Feeds the buffered history back into the freshly-built inner control after a rebuild. Line mode
  // replays every series in order (its per-series buffers are independent); Dot mode replays only the
  // primary, since Lite is single-series.
  private void ReplayHistory() {
    switch (_inner) {
      case PerformanceGraph graph:
        int seriesCount = Math.Min(_history.Count, _series.Count + 1);
        for (int s = 0; s < seriesCount; s++)
          foreach (var value in _history[s]) graph.AddValue(s, value);
        break;
      case PerformanceGraphLite lite:
        foreach (var value in _history[0]) lite.AddValue(value);
        break;
    }
  }
}
