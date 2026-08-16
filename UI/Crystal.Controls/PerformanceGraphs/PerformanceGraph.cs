using Crystal.Controls.PerformanceGraphs.Buffers;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Renders;
using Crystal.Controls.PerformanceGraphs.Styles;
using Crystal.Controls.PerformanceGraphs.Themes;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs;

public class PerformanceGraph : FrameworkElement {
  private const int DefaultHistoryLength = 60;
  private const int DefaultGridColumns = 60;
  private const int Rows = 12;

  /// <summary>Identifies the <see cref="Kind"/> dependency property.</summary>
  public static readonly DependencyProperty KindProperty =
      DependencyProperty.Register(nameof(Kind), typeof(GraphKind), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(GraphKind.Line, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="Flip"/> dependency property.</summary>
  public static readonly DependencyProperty FlipProperty =
      DependencyProperty.Register(nameof(Flip), typeof(bool), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="MinValue"/> dependency property.</summary>
  public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="MaxValue"/> dependency property.</summary>
  public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="HistoryLength"/> dependency property.</summary>
  public static readonly DependencyProperty HistoryLengthProperty =
      DependencyProperty.Register(nameof(HistoryLength), typeof(int), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(DefaultHistoryLength, FrameworkPropertyMetadataOptions.AffectsRender, OnHistoryLengthChanged),
          ValidateHistoryLength);

  private static bool ValidateHistoryLength(object value) => value is int length && length > 0;

  /// <summary>Identifies the <see cref="LineBrush"/> dependency property.</summary>
  public static readonly DependencyProperty LineBrushProperty =
      DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(Brushes.Blue, FrameworkPropertyMetadataOptions.AffectsRender, OnLineBrushChanged));

  /// <summary>Identifies the <see cref="LineThickness"/> dependency property.</summary>
  public static readonly DependencyProperty LineThicknessProperty =
      DependencyProperty.Register(nameof(LineThickness), typeof(double), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLineThicknessChanged));

  /// <summary>Identifies the <see cref="FillBrush"/> dependency property.</summary>
  public static readonly DependencyProperty FillBrushProperty =
      DependencyProperty.Register(nameof(FillBrush), typeof(Brush), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender, OnFillBrushChanged));

  /// <summary>Identifies the <see cref="GraphBackground"/> dependency property.</summary>
  public static readonly DependencyProperty GraphBackgroundProperty =
      DependencyProperty.Register(nameof(GraphBackground), typeof(Brush), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnGraphBackgroundChanged));

  /// <summary>Identifies the <see cref="GridBrush"/> dependency property.</summary>
  public static readonly DependencyProperty GridBrushProperty =
      DependencyProperty.Register(nameof(GridBrush), typeof(Brush), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(Brushes.DarkBlue, FrameworkPropertyMetadataOptions.AffectsRender, OnGridBrushChanged));

  /// <summary>Identifies the <see cref="GridThickness"/> dependency property.</summary>
  public static readonly DependencyProperty GridThicknessProperty =
      DependencyProperty.Register(nameof(GridThickness), typeof(double), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(0.6, FrameworkPropertyMetadataOptions.AffectsRender, OnGridThicknessChanged));

  /// <summary>Identifies the <see cref="BorderBrush"/> dependency property.</summary>
  public static readonly DependencyProperty BorderBrushProperty =
      DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnBorderBrushChanged));

  /// <summary>Identifies the <see cref="BorderThickness"/> dependency property.</summary>
  public static readonly DependencyProperty BorderThicknessProperty =
      DependencyProperty.Register(nameof(BorderThickness), typeof(double), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(0.8, FrameworkPropertyMetadataOptions.AffectsRender, OnBorderThicknessChanged));

  /// <summary>Identifies the <see cref="MarkerBrush"/> dependency property.</summary>
  public static readonly DependencyProperty MarkerBrushProperty =
      DependencyProperty.Register(nameof(MarkerBrush), typeof(Brush), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnMarkerBrushChanged));

  /// <summary>Identifies the <see cref="LowMarker"/> dependency property.</summary>
  public static readonly DependencyProperty LowMarkerProperty =
      DependencyProperty.Register(nameof(LowMarker), typeof(double), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="HighMarker"/> dependency property.</summary>
  public static readonly DependencyProperty HighMarkerProperty =
      DependencyProperty.Register(nameof(HighMarker), typeof(double), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="MarkerFormat"/> dependency property.</summary>
  public static readonly DependencyProperty MarkerFormatProperty =
      DependencyProperty.Register(nameof(MarkerFormat), typeof(string), typeof(PerformanceGraph),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

  private readonly BackgroundRenderer _backgroundRender = new();
  private readonly GridRenderer _gridRender;
  private readonly FilledLineRenderer _filledLineRender = new();
  private readonly BarRenderer _barRender = new();
  private readonly SegmentedBarRenderer _segmentedBarRender = new();
  private readonly BorderRenderer _borderRender = new();
  private readonly MarkerRenderer _markerRender = new();
  private readonly GraphStyle _graphStyle = new();

  // Right-aligned sample buffer for the primary series (index 0): index 0 is oldest, [Count-1] is
  // the most recent value. The primary series' line/fill live in _graphStyle and are driven by the
  // LineBrush/FillBrush/LineThickness dependency properties, so every existing single-series graph
  // is unaffected.
  // Not readonly: the HistoryLength dependency property rebuilds this buffer (and _historyLength)
  // when the plotted sample count changes.
  private CircularBuffer<double> _values;
  private int _historyLength;

  // Additional overlay series (index 1..N), each with its own buffer, line pen and optional fill.
  // Only meaningful for GraphKind.Line — bars/segmented bars draw the primary series alone. Populated
  // via AddSeries and fed via AddValue(series, value); empty for every graph that never opts in.
  private readonly List<Series> _extraSeries = new();

  // Buffer + per-series pens for an overlay series. A missing FillBrush draws the series as a plain
  // line (the usual choice for an overlaid read/write pair, where two filled areas would occlude).
  // Each series owns its own renderer: FilledLineRenderer reuses its StreamGeometry across frames,
  // which is only safe when a single geometry isn't drawn twice within one render pass — so per-series
  // renderers (not one shared instance looped) keep the overlays from stomping each other's geometry.
  private sealed class Series {
    // Not readonly: rebuilt by CopyMostRecent when HistoryLength changes.
    public CircularBuffer<double> Values;
    public readonly FilledLineRenderer Renderer = new();
    public Pen LinePen;
    public Brush? FillBrush;

    public Series(int capacity, Pen linePen, Brush? fillBrush) {
      Values = new CircularBuffer<double>(capacity);
      LinePen = linePen;
      FillBrush = fillBrush;
    }
  }

  // Rendering is suspended while the control is off-screen — a collapsed tile, a minimized window,
  // or a closed detail window all flip IsVisible to false. Samples still land in the buffer so no
  // data gap forms, but InvalidateVisual (which queues a render pass every poll for something nobody
  // is looking at) is skipped. A single deferred invalidation is coalesced and flushed the moment the
  // control becomes visible again, so the plot is correct as soon as it reappears.
  private bool _renderSuspended;
  private bool _pendingRender;

  /// <summary>Creates a graph with the default 60-sample history and a 60-column grid.</summary>
  public PerformanceGraph() : this(DefaultHistoryLength, DefaultGridColumns) { }

  /// <summary>
  /// Creates a graph whose sample history and grid density are set independently: the
  /// number of grid columns is purely cosmetic and has no effect on where samples land —
  /// only <paramref name="historyLength"/> (the ring buffer's capacity) does that. Set them
  /// to different values freely, e.g. a longer history than the grid resolution shows, or a
  /// finer/coarser grid than the sample rate would otherwise suggest.
  /// </summary>
  public PerformanceGraph(int historyLength, int gridColumns) {
    if (historyLength <= 0) throw new ArgumentOutOfRangeException(nameof(historyLength), "History length must be positive.");
    if (gridColumns <= 0) throw new ArgumentOutOfRangeException(nameof(gridColumns), "Grid column count must be positive.");

    _historyLength = historyLength;
    _values = new CircularBuffer<double>(historyLength);
    _gridRender = new GridRenderer(Rows, gridColumns);
    GridColumns = gridColumns;

    // Keep the HistoryLength DP in step with the constructor argument (the DP defaults to
    // DefaultHistoryLength, so a non-default programmatic size would otherwise disagree with it).
    // SetCurrentValue leaves a later Style/binding free to override; OnHistoryLengthChanged no-ops
    // here because _values is already this size.
    SetCurrentValue(HistoryLengthProperty, historyLength);

    SnapsToDevicePixels = true;
    UseLayoutRounding = true;

    // Suspend rendering whenever the control leaves the screen and flush any deferred render when it
    // returns. IsVisible already folds in every ancestor's visibility and the window's, so this one
    // hook covers collapsed tiles, minimized windows, and closed detail windows alike.
    IsVisibleChanged += (_, e) => ApplyVisibility((bool)e.NewValue);
    ApplyVisibility(IsVisible);
  }

  /// <summary>True while rendering is suspended because the control is off-screen.</summary>
  internal bool RenderSuspended => _renderSuspended;

  /// <summary>True when a sample arrived while suspended, so a repaint is owed on the next show.</summary>
  internal bool HasPendingRender => _pendingRender;

  // Core of the visibility gate, split out from the IsVisibleChanged hook so it can be driven
  // deterministically in tests (IsVisible only flips true under a live, shown window).
  internal void ApplyVisibility(bool visible) {
    _renderSuspended = !visible;
    // Became visible with samples added while hidden — repaint once to show the current buffer.
    if (visible && _pendingRender) {
      _pendingRender = false;
      InvalidateVisual();
    }
  }

  // Queue a repaint, unless the control is off-screen — then just remember one is owed so it can be
  // flushed on the next IsVisible transition. Keeps the ring buffer and the screen in sync without
  // spending a render pass on an invisible control every poll.
  private void RequestRender() {
    if (_renderSuspended) {
      _pendingRender = true;
      return;
    }
    InvalidateVisual();
  }

  /// <summary>Number of samples retained/plotted — independent of <see cref="GridColumns"/>.</summary>
  public int Capacity => _historyLength;

  /// <summary>Number of vertical grid lines drawn — a purely cosmetic density, independent of <see cref="Capacity"/>.</summary>
  public int GridColumns { get; }

  /// <summary>Selects whether buffered samples are drawn as a continuous filled line/area or as discrete bars.</summary>
  public GraphKind Kind {
    get => (GraphKind)GetValue(KindProperty);
    set => SetValue(KindProperty, value);
  }

  /// <summary>When true, a <see cref="GraphKind.SegmentedBar"/> is drawn mirrored (180°): its
  /// segments hang from the top edge and grow downward instead of rising from the bottom. Has no
  /// effect on the other kinds.</summary>
  public bool Flip {
    get => (bool)GetValue(FlipProperty);
    set => SetValue(FlipProperty, value);
  }

  /// <summary>Values at or below this map to the bottom edge of the plot area.</summary>
  public double MinValue {
    get => (double)GetValue(MinValueProperty);
    set => SetValue(MinValueProperty, value);
  }

  /// <summary>Values at or above this map to the top edge of the plot area.</summary>
  public double MaxValue {
    get => (double)GetValue(MaxValueProperty);
    set => SetValue(MaxValueProperty, value);
  }

  /// <summary>
  /// Number of samples retained and plotted across the width — the graph's x-axis span (at the
  /// default 1-second poll cadence, the number of seconds shown, e.g. 30/60/120). Settable in XAML;
  /// changing it rebuilds the sample buffer, keeping the most recent samples that still fit.
  /// Must be positive. Independent of <see cref="GridColumns"/> (a purely cosmetic vertical-line
  /// density that is fixed at construction).
  /// </summary>
  public int HistoryLength {
    get => (int)GetValue(HistoryLengthProperty);
    set => SetValue(HistoryLengthProperty, value);
  }

  /// <summary>Stroke color/brush of the data line.</summary>
  public Brush LineBrush {
    get => (Brush)GetValue(LineBrushProperty);
    set => SetValue(LineBrushProperty, value);
  }

  /// <summary>Stroke thickness of the data line.</summary>
  public double LineThickness {
    get => (double)GetValue(LineThicknessProperty);
    set => SetValue(LineThicknessProperty, value);
  }

  /// <summary>Fill brush painted under the data line, down to the baseline (use a vertical gradient for the "glow" look).</summary>
  public Brush FillBrush {
    get => (Brush)GetValue(FillBrushProperty);
    set => SetValue(FillBrushProperty, value);
  }

  /// <summary>Solid backdrop painted behind the grid and data.</summary>
  public Brush GraphBackground {
    get => (Brush)GetValue(GraphBackgroundProperty);
    set => SetValue(GraphBackgroundProperty, value);
  }

  /// <summary>Brush used for the grid lines.</summary>
  public Brush GridBrush {
    get => (Brush)GetValue(GridBrushProperty);
    set => SetValue(GridBrushProperty, value);
  }

  /// <summary>Stroke thickness of the grid lines.</summary>
  public double GridThickness {
    get => (double)GetValue(GridThicknessProperty);
    set => SetValue(GridThicknessProperty, value);
  }

  /// <summary>Brush used for the outer border.</summary>
  public Brush BorderBrush {
    get => (Brush)GetValue(BorderBrushProperty);
    set => SetValue(BorderBrushProperty, value);
  }

  /// <summary>Stroke thickness of the outer border. Zero draws no border.</summary>
  public double BorderThickness {
    get => (double)GetValue(BorderThicknessProperty);
    set => SetValue(BorderThicknessProperty, value);
  }

  /// <summary>Brush for the horizontal session-extreme marker lines. Null (the default) draws no
  /// markers, so a graph opts in only by setting this.</summary>
  public Brush? MarkerBrush {
    get => (Brush?)GetValue(MarkerBrushProperty);
    set => SetValue(MarkerBrushProperty, value);
  }

  /// <summary>Data value at which to draw the low marker line (e.g. the lowest sample seen this
  /// session). <see cref="double.NaN"/> (the default) draws nothing.</summary>
  public double LowMarker {
    get => (double)GetValue(LowMarkerProperty);
    set => SetValue(LowMarkerProperty, value);
  }

  /// <summary>Data value at which to draw the high marker line (e.g. the highest sample seen this
  /// session). <see cref="double.NaN"/> (the default) draws nothing.</summary>
  public double HighMarker {
    get => (double)GetValue(HighMarkerProperty);
    set => SetValue(HighMarkerProperty, value);
  }

  /// <summary>Numeric format string (e.g. "0.00") for the value printed beside each marker line.
  /// Null (the default) draws the lines without labels, so a graph opts into labels by setting this.</summary>
  public string? MarkerFormat {
    get => (string?)GetValue(MarkerFormatProperty);
    set => SetValue(MarkerFormatProperty, value);
  }

  /// <summary>
  /// Applies every property the given theme sets, leaving anything it leaves null untouched.
  /// A theme's fill brush is typically chosen for a specific <see cref="Kind"/> — see
  /// <see cref="Themes.GraphThemes.FromAccent"/> — so re-apply after changing <see cref="Kind"/>
  /// if you're switching between line and bar styles at runtime.
  /// </summary>
  public void ApplyTheme(GraphTheme theme) {
    if (theme == null) return;

    if (theme.GraphBackground != null) GraphBackground = theme.GraphBackground;
    if (theme.GridBrush != null) GridBrush = theme.GridBrush;
    if (theme.BorderBrush != null) BorderBrush = theme.BorderBrush;
    if (theme.LineBrush != null) LineBrush = theme.LineBrush;
    if (theme.LineThickness.HasValue) LineThickness = theme.LineThickness.Value;
    if (theme.FillBrush != null) FillBrush = theme.FillBrush;
  }

  /// <summary>Number of series plotted: the primary (index 0) plus any added via <see cref="AddSeries"/>.</summary>
  internal int SeriesCount => 1 + _extraSeries.Count;

  /// <summary>
  /// Registers an additional line series overlaid on the primary one, returning the index used to
  /// feed it via <see cref="AddValue(int, double)"/>. The primary series is index 0; the first
  /// added series is index 1, and so on. Overlay series are drawn only for <see cref="GraphKind.Line"/>.
  /// Pass a <paramref name="fillBrush"/> only when a filled area is wanted — for an overlaid pair
  /// (e.g. read/write) leave it null so the second series is a plain line and doesn't occlude the first.
  /// Call on the UI thread (e.g. from the graph's Loaded handler).
  /// </summary>
  public int AddSeries(Brush lineBrush, Brush? fillBrush = null, double thickness = 2) {
    _extraSeries.Add(new Series(_historyLength, Helpers.CreateFrozenPen(lineBrush, thickness), fillBrush));
    RequestRender();
    return _extraSeries.Count; // index 0 is the primary, so the first overlay is 1
  }

  /// <summary>Appends a new sample to the primary series (index 0). O(1).</summary>
  public void AddValue(double value) => AddValue(0, value);

  /// <summary>
  /// Appends a new sample to the given series, dropping the oldest once <see cref="Capacity"/> is
  /// exceeded. Series 0 is the primary; 1..N are overlays returned by <see cref="AddSeries"/>. O(1).
  /// </summary>
  public void AddValue(int series, double value) {
    // Sensor streams push from a background thread; InvalidateVisual (and the
    // buffer) require this element's dispatcher, so hop onto it if we're not already there.
    if (!CheckAccess()) {
      Dispatcher.BeginInvoke(() => AddValue(series, value));
      return;
    }
    BufferFor(series).Add(value);
    RequestRender();
  }

  /// <summary>Discards all buffered samples across every series.</summary>
  public void ClearValues() {
    if (!CheckAccess()) {
      Dispatcher.BeginInvoke(ClearValues);
      return;
    }
    _values.Clear();
    foreach (var s in _extraSeries) s.Values.Clear();
    RequestRender();
  }

  private CircularBuffer<double> BufferFor(int series) =>
      series == 0 ? _values : _extraSeries[series - 1].Values;

  private static void OnLineBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.LinePen = Helpers.CreateFrozenPen((Brush)e.NewValue, graph._graphStyle.LinePen.Thickness);
  }

  private static void OnLineThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.LinePen = Helpers.CreateFrozenPen(graph._graphStyle.LinePen.Brush, (double)e.NewValue);
  }

  private static void OnFillBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((PerformanceGraph)d)._graphStyle.FillBrush = (Brush)e.NewValue;

  private static void OnHistoryLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    int newLength = (int)e.NewValue;
    if (newLength == graph._historyLength) return; // e.g. the constructor's own sync set

    graph._historyLength = newLength;
    graph._values = CopyMostRecent(graph._values, newLength);
    foreach (var s in graph._extraSeries)
      s.Values = CopyMostRecent(s.Values, newLength);
  }

  // Rebuilds a buffer at a new capacity, carrying over the most recent samples that still fit
  // (the newest min(Count, newCapacity) values): growing keeps everything, shrinking drops the oldest.
  private static CircularBuffer<double> CopyMostRecent(CircularBuffer<double> source, int newCapacity) {
    var next = new CircularBuffer<double>(newCapacity);
    int start = source.Count > newCapacity ? source.Count - newCapacity : 0;
    for (int i = start; i < source.Count; i++) next.Add(source[i]);
    return next;
  }

  private static void OnGraphBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((PerformanceGraph)d)._graphStyle.BackgroundBrush = (Brush)e.NewValue;

  private static void OnGridBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.GridPen = Helpers.CreateFrozenPen((Brush)e.NewValue, graph._graphStyle.GridPen.Thickness);
  }

  private static void OnGridThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.GridPen = Helpers.CreateFrozenPen(graph._graphStyle.GridPen.Brush, (double)e.NewValue);
  }

  private static void OnBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.BorderPen = Helpers.CreateFrozenPen((Brush)e.NewValue, graph._graphStyle.BorderPen.Thickness);
  }

  private static void OnBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    double thickness = (double)e.NewValue;
    graph._graphStyle.BorderThickness = thickness;
    graph._graphStyle.BorderPen = Helpers.CreateFrozenPen(graph._graphStyle.BorderPen.Brush, thickness);
  }

  private static void OnMarkerBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.MarkerPen = e.NewValue is Brush brush ? Helpers.CreateDashedPen(brush, 2) : null;
  }

  protected override Size MeasureOverride(Size availableSize) {
    // Provide a default desired size if constraints are infinite
    double width = double.IsInfinity(availableSize.Width) ? 200 : availableSize.Width;
    double height = double.IsInfinity(availableSize.Height) ? 100 : availableSize.Height;
    return new Size(width, height);
  }

  protected override Size ArrangeOverride(Size finalSize) {
    return finalSize;
  }

  protected override void OnRender(DrawingContext dc) {
    base.OnRender(dc);

    Rect bounds = new(RenderSize);

    // Background fills the whole control first, behind everything else.
    _backgroundRender.Draw(dc, bounds, _graphStyle);

    // Grid on top of the background — GridColumns purely cosmetic, unrelated to history length.
    _gridRender.Draw(dc, bounds, _graphStyle);

    // Data on top of the grid — a continuous filled line, plain bars, or segmented bars.
    // _historyLength (not GridColumns) is what "capacity" means here: it's how many slots
    // the data's own horizontal layout is divided into, so a full buffer spans the width
    // regardless of how many grid lines happen to be drawn across it.
    switch (Kind) {
      case GraphKind.Bar:
        _barRender.Draw(dc, bounds, _graphStyle, _values, _historyLength, MinValue, MaxValue);
        break;
      case GraphKind.SegmentedBar:
        _segmentedBarRender.Draw(dc, bounds, _graphStyle, _values, _historyLength, MinValue, MaxValue, Rows, Flip);
        break;
      default:
        // Primary series first (so its fill sits underneath), then each overlay on top. Each series
        // draws through its own renderer so the reused-across-frames StreamGeometry of one isn't
        // re-Opened by another within this same pass (which would render both with the last geometry).
        _filledLineRender.Draw(dc, bounds, _values, _historyLength, MinValue, MaxValue,
            _graphStyle.LinePen, _graphStyle.FillBrush);
        foreach (var s in _extraSeries)
          s.Renderer.Draw(dc, bounds, s.Values, _historyLength, MinValue, MaxValue,
              s.LinePen, s.FillBrush);
        break;
    }

    // Session-extreme markers over the data line but under the border, so a recovered dip/spike
    // stays visible. No-ops unless MarkerBrush is set and the value is a real number. When
    // MarkerFormat is set, each line is labeled with its value: the high label drops below its line,
    // the low label lifts above its, so neither is clipped at the plot edge.
    double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
    _markerRender.Draw(dc, bounds, _graphStyle, LowMarker, MinValue, MaxValue,
        FormatMarker(LowMarker), topBiased: false, dpi);
    _markerRender.Draw(dc, bounds, _graphStyle, HighMarker, MinValue, MaxValue,
        FormatMarker(HighMarker), topBiased: true, dpi);

    // Border drawn last so its edge stays crisp over the fill/grid instead of being covered.
    _borderRender.Draw(dc, bounds, _graphStyle);
  }

  // The label for a marker value, or null to draw the line unlabeled — when no format is set, or the
  // value is NaN (the marker itself is a no-op then anyway).
  private string? FormatMarker(double value) =>
      MarkerFormat is { } format && !double.IsNaN(value)
          ? value.ToString(format, System.Globalization.CultureInfo.InvariantCulture)
          : null;
}
