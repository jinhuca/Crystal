using Crystal.Controls.PerformanceGraphs.Buffers;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Renders;
using Crystal.Controls.PerformanceGraphs.Styles;
using Crystal.Controls.PerformanceGraphs.Themes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// The single performance-graph control for this library. 
/// A <see cref="DisplayMode"/> selects how the buffered samples are drawn — a continuous filled <see cref="DisplayMode.Line"/> 
/// (with optional overlay series and value-banding), a <see cref="DisplayMode.Dot"/> matrix gauge, or several independent lines 
/// in <see cref="DisplayMode.MultipleLine"/> mode — and the <see cref="Border"/>/<see cref="Grid"/> toggles pick whether 
/// the framing chrome is drawn.
/// </summary>
public class PerformanceGraph : FrameworkElement, ISingleSeriesGraph {
  /// <summary>
  /// The default number of samples retained/plotted — independent of <see cref="GridColumns"/>.
  /// </summary>
  private const int DefaultHistoryLength = 60;

  /// <summary>
  /// The default number of vertical grid lines drawn — a purely cosmetic density, independent of <see cref="Capacity"/>.
  /// </summary>
  private const int DefaultGridColumns = 60;

  /// <summary>
  /// The default number of horizontal grid lines drawn — purely cosmetic.
  /// </summary>
  private const int DefaultGridRows = 12;

  /// <summary>
  /// The default number of rows in Dot mode, purely cosmetic.
  /// </summary>
  private const int DefaultRows = 10;

  /// <summary>
  /// The number of color bands in the default green→red gauge ramp (band 0 green … band 8 red) for Dot-mode banding, 
  /// single-sourced from GaugeBandPalette so the dot matrix and the banded Line share the exact same colors.
  /// </summary>
  private const int BandCount = GaugeBandPalette.BandCount;

  /// <summary>
  /// Fraction of each column slot's width the dot occupies, and of each row's height a *full* dot 
  /// occupies (Dot mode). A fractional dot keeps this same width and starting height, just shortened.
  /// </summary>
  private const double ColumnWidthRatio = 0.9;

  /// <summary>
  /// Fraction of each column slot's width the dot occupies, and of each row's height a *full* dot
  /// </summary>
  private const double DotSizeRatio = 0.85;

  /// <summary>
  /// Default green→red gauge ramp (band 0 green … band 8 red) for Dot-mode banding, single-sourced
  /// from GaugeBandPalette so the dot matrix and the banded Line share the exact same colors. 
  /// </summary>
  private static readonly Brush[] DefaultBandColors = GaugeBandPalette.Solid;


  /// <summary>
  /// Identifies the <see cref="ValuesSource"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty ValuesSourceProperty = DependencyProperty.Register(
    nameof(ValuesSource),
    typeof(ObservableCollection<double>),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(null, OnValuesSourceChanged));

  /// <summary>
  /// Identifies the <see cref="DisplayMode"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.Register(
    nameof(DisplayMode),
    typeof(DisplayMode),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(DisplayMode.Line, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="Border"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty BorderProperty = DependencyProperty.Register(
    nameof(Border),
    typeof(bool),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="Grid"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty GridProperty = DependencyProperty.Register(
    nameof(Grid),
    typeof(bool),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="Flip"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty FlipProperty = DependencyProperty.Register(
    nameof(Flip),
    typeof(bool),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="MinValue"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
    nameof(MinValue),
    typeof(double),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="MaxValue"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
    nameof(MaxValue),
    typeof(double),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="HistoryLength"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty HistoryLengthProperty = DependencyProperty.Register(
    nameof(HistoryLength),
    typeof(int),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(DefaultHistoryLength, FrameworkPropertyMetadataOptions.AffectsRender, OnHistoryLengthChanged),
    ValidateHistoryLength);

  /// <summary>
  /// Validates that the <see cref="HistoryLength"/> is a positive integer.
  /// </summary>
  /// <param name="value">The value to validate.</param>
  /// <returns>True if the value is valid; otherwise, false.</returns>
  private static bool ValidateHistoryLength(object value) => value is int length && length > 0;

  /// <summary>
  /// Identifies the <see cref="LineBrush"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(
    nameof(LineBrush),
    typeof(Brush),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(Brushes.Blue, FrameworkPropertyMetadataOptions.AffectsRender, OnLineBrushChanged));

  /// <summary>
  /// Identifies the <see cref="LineThickness"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register(
    nameof(LineThickness),
    typeof(double),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLineThicknessChanged));

  /// <summary>
  /// Identifies the <see cref="FillBrush"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty FillBrushProperty = DependencyProperty.Register(
    nameof(FillBrush),
    typeof(Brush),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender, OnFillBrushChanged));

  /// <summary>
  /// Identifies the <see cref="GraphBackground"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty GraphBackgroundProperty = DependencyProperty.Register(
    nameof(GraphBackground),
    typeof(Brush),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnGraphBackgroundChanged));

  /// <summary>
  /// Identifies the <see cref="GridBrush"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty GridBrushProperty = DependencyProperty.Register(
    nameof(GridBrush),
    typeof(Brush),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(Brushes.DarkBlue, FrameworkPropertyMetadataOptions.AffectsRender, OnGridBrushChanged));

  /// <summary>
  /// Identifies the <see cref="GridThickness"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty GridThicknessProperty = DependencyProperty.Register(
    nameof(GridThickness),
    typeof(double),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(0.6, FrameworkPropertyMetadataOptions.AffectsRender, OnGridThicknessChanged));

  /// <summary>
  /// Identifies the <see cref="BorderBrush"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
    nameof(BorderBrush),
    typeof(Brush),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnBorderBrushChanged));

  /// <summary>
  /// Identifies the <see cref="BorderThickness"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
    nameof(BorderThickness),
    typeof(double),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(0.8, FrameworkPropertyMetadataOptions.AffectsRender, OnBorderThicknessChanged));

  /// <summary>
  /// Identifies the <see cref="MarkerBrush"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty MarkerBrushProperty = DependencyProperty.Register(
    nameof(MarkerBrush),
    typeof(Brush),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnMarkerBrushChanged));

  /// <summary>
  /// Identifies the <see cref="LowMarker"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty LowMarkerProperty = DependencyProperty.Register(
    nameof(LowMarker),
    typeof(double),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="HighMarker"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty HighMarkerProperty = DependencyProperty.Register(
    nameof(HighMarker),
    typeof(double),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="MarkerFormat"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty MarkerFormatProperty = DependencyProperty.Register(
    nameof(MarkerFormat),
    typeof(string),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="BandedLine"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty BandedLineProperty = DependencyProperty.Register(
    nameof(BandedLine),
    typeof(bool),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="CellPitch"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty CellPitchProperty = DependencyProperty.Register(
    nameof(CellPitch),
    typeof(double),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), ValidateCellPitch);

  private static bool ValidateCellPitch(object value) => value is double pitch && pitch >= 0;

  /// <summary>
  /// Identifies the <see cref="BandColors"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty BandColorsProperty = DependencyProperty.Register(
    nameof(BandColors),
    typeof(Brush[]),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="Rows"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
    nameof(Rows),
    typeof(int),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(DefaultRows, FrameworkPropertyMetadataOptions.AffectsRender),
    value => value is int rows && rows > 0);

  /// <summary>
  /// Identifies the <see cref="ColorMode"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty ColorModeProperty = DependencyProperty.Register(
    nameof(ColorMode),
    typeof(DotColorMode),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(DotColorMode.Banded, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Identifies the <see cref="CornerRadius"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
    nameof(CornerRadius),
    typeof(double),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender),
    value => value is double radius && radius >= 0);

  /// <summary>
  /// Identifies the <see cref="DotColor"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty DotColorProperty = DependencyProperty.Register(
    nameof(DotColor),
    typeof(Brush),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender,
        (d, e) => ((PerformanceGraph)d)._resolvedDotColor = ResolveSolidBrush((Brush)e.NewValue)));

  /// <summary>
  /// Identifies the <see cref="Color1"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty Color1Property = RegisterBandColor(nameof(Color1), 0);

  /// <summary>
  /// Identifies the <see cref="Color2"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty Color2Property = RegisterBandColor(nameof(Color2), 1);

  /// <summary>
  /// Identifies the <see cref="Color3"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty Color3Property = RegisterBandColor(nameof(Color3), 2);

  /// <summary>
  /// Identifies the <see cref="Color4"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty Color4Property = RegisterBandColor(nameof(Color4), 3);

  /// <summary>
  /// Identifies the <see cref="Color5"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty Color5Property = RegisterBandColor(nameof(Color5), 4);

  /// <summary>
  /// Identifies the <see cref="Color6"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty Color6Property = RegisterBandColor(nameof(Color6), 5);

  /// <summary>
  /// Identifies the <see cref="Color7"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty Color7Property = RegisterBandColor(nameof(Color7), 6);

  /// <summary>
  /// Identifies the <see cref="Color8"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty Color8Property = RegisterBandColor(nameof(Color8), 7);

  /// <summary>
  /// Identifies the <see cref="Color9"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty Color9Property = RegisterBandColor(nameof(Color9), 8);

  private static DependencyProperty RegisterBandColor(string name, int band) =>
      DependencyProperty.Register(name, typeof(Brush), typeof(PerformanceGraph),
        new FrameworkPropertyMetadata(DefaultBandColors[band],
          FrameworkPropertyMetadataOptions.AffectsRender,
          (d, e) => ((PerformanceGraph)d)._resolvedColors[band] = ResolveSolidBrush((Brush)e.NewValue)));

  /// <summary>
  /// Identifies the <see cref="Accent"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty AccentProperty = DependencyProperty.Register(
    nameof(Accent),
    typeof(Color),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(Color.FromRgb(0x3B, 0xD1, 0x5A), OnAccentChanged));

  /// <summary>
  /// Identifies the <see cref="BandStartColor"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty BandStartColorProperty = DependencyProperty.Register(
    nameof(BandStartColor),
    typeof(Color?),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(null, OnBandRampChanged));

  /// <summary>
  /// Identifies the <see cref="BandEndColor"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty BandEndColorProperty = DependencyProperty.Register(
    nameof(BandEndColor),
    typeof(Color?),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(null, OnBandRampChanged));

  /// <summary>
  /// Identifies the <see cref="Series"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register(
    nameof(Series),
    typeof(ObservableCollection<DataSeries>),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnSeriesChanged));

  private readonly BackgroundRenderer _backgroundRender = new();
  private GridRenderer _gridRender;
  private readonly BorderRenderer _borderRender = new();
  private readonly GraphStyle _graphStyle = new();

  /// <summary>
  /// Only the renderer matching the current mode is exercised per frame, and the marker renderer only
  /// when a graph opts into markers — so each is created lazily on first use.
  /// </summary>
  private FilledLineRenderer? _filledLineRender;

  /// <summary>
  /// Only the renderer matching the current mode is exercised per frame, and the marker renderer only
  /// when a graph opts into markers — so each is created lazily on first use.
  /// </summary>
  private MarkerRenderer? _markerRender;

  /// <summary>
  /// Dot-mode geometry caches (see PerformanceGraphLite's original notes): one StreamGeometry per
  /// color band for Banded mode, one shared geometry for SingleColor mode, each lazily created on
  /// first render in that mode so a graph that never renders dots allocates neither.
  /// </summary>
  private StreamGeometry[]? _bandGeometries;

  /// <summary>
  /// Dot-mode geometry caches (see PerformanceGraphLite's original notes): one StreamGeometry per
  /// color band for Banded mode, one shared geometry for SingleColor mode, each lazily created on
  /// first render in that mode so a graph that never renders dots allocates neither.
  /// </summary>
  private StreamGeometry? _singleGeometry;

  /// <summary>
  /// Creates a new array of StreamGeometry objects, one for each band in the graph. 
  /// Each geometry is initialized as an empty StreamGeometry.
  /// </summary>
  /// <returns>Returns an array of StreamGeometry objects.</returns>
  private static StreamGeometry[] CreateBandGeometries() {
    var geometries = new StreamGeometry[BandCount];
    for (int i = 0; i < BandCount; i++) geometries[i] = new StreamGeometry();
    return geometries;
  }

  /// <summary>
  /// Dot-mode band brushes resolved once per Color1..9 change, not per frame. Cloned so freezing our
  /// own references never touches the shared defaults array.
  /// </summary>
  private readonly Brush[] _resolvedColors = (Brush[])DefaultBandColors.Clone();

  /// <summary>
  /// Dot-mode single-color brush resolved once per DotColor change, not per frame. Cloned so freezing
  /// our own references never touches the shared defaults array.
  /// </summary>
  private Brush _resolvedDotColor = ResolveSolidBrush(Brushes.Gray);

  /// <summary>
  /// Solid band pens for the banded Line, one per gauge band at the current LineThickness. Built
  /// lazily and rebuilt when thickness or the source palette changes.
  /// </summary>
  private Pen[]? _bandLinePens;

  /// <summary>
  /// The LineThickness value that produced the current _bandLinePens array, so a change in thickness
  /// requires rebuilding the pens.
  /// </summary>
  private double _bandPenThickness = double.NaN;

  /// <summary>
  /// The Brush[] source that produced the current _bandLinePens array, so a change in the source palette
  /// requires rebuilding the pens.
  /// </summary>
  private Brush[]? _bandPenSource;

  /// <summary>
  /// The solid band brushes the banded Line paints with: a per-instance BandColors override when it
  /// supplies the full BandCount of brushes, otherwise the shared default green→red ramp.
  /// </summary>
  /// <returns>Array of solid brushes for each band.</returns>
  private Brush[] BandSolids() =>
      BandColors is { Length: BandCount } custom ? custom : GaugeBandPalette.Solid;

  /// <summary>
  /// Creates a new array of Pen objects, one for each band in the graph. Each pen is created using
  /// the specified brush and thickness.
  /// </summary>
  /// <returns>Array of pens for each band.</returns>
  private Pen[] BandLinePens() {
    double thickness = LineThickness;
    Brush[] solids = BandSolids();
    if (_bandLinePens == null || _bandPenThickness != thickness || !ReferenceEquals(_bandPenSource, solids)) {
      var pens = new Pen[solids.Length];
      for (int i = 0; i < solids.Length; i++) pens[i] = Helpers.CreateFrozenPen(solids[i], thickness);
      _bandLinePens = pens;
      _bandPenThickness = thickness;
      _bandPenSource = solids;
    }
    return _bandLinePens;
  }

  /// <summary>
  /// Creates a new array of Brush objects, one for each band in the graph. Each brush is derived
  /// from the corresponding solid brush.
  /// </summary>
  private Brush[]? _bandFills;

  /// <summary>
  /// Creates a new array of Brush objects, one for each band in the graph. Each brush is derived
  /// from the corresponding solid brush.
  /// </summary>
  /// <returns>Array of fill brushes for each band.</returns>
  private Brush[]? _bandFillSource;

  private Brush[] BandFills() {
    Brush[] solids = BandSolids();
    if (ReferenceEquals(solids, GaugeBandPalette.Solid)) return GaugeBandPalette.Fill;
    if (_bandFills == null || !ReferenceEquals(_bandFillSource, solids)) {
      _bandFills = GaugeBandPalette.DeriveFill(solids);
      _bandFillSource = solids;
    }
    return _bandFills;
  }

  /// <summary>
  /// Right-aligned sample buffer for the primary series (index 0). Rebuilt by the HistoryLength DP.
  /// </summary>
  private CircularBuffer<double> _values;

  /// <summary>
  /// The number of samples retained/plotted — independent of <see cref="GridColumns"/>. 
  /// Rebuilt by the HistoryLength DP.
  /// </summary>
  private int _historyLength;

  /// <summary>
  /// Additional overlay series (index 1..N) for Line mode, each with its own buffer and pens.
  /// </summary>
  private readonly List<OverlaySeries> _extraSeries = new();

  /// <summary>
  /// A single overlay series, with its own buffer and pens, for Line mode. 
  /// This is a convenience for the common case of a single overlay series, so the user doesn't have to 
  /// create a collection and manage it.
  /// </summary>
  private sealed class OverlaySeries {
    public CircularBuffer<double> Values;
    public readonly FilledLineRenderer Renderer = new();
    public Pen LinePen;
    public Brush? FillBrush;

    public OverlaySeries(int capacity, Pen linePen, Brush? fillBrush) {
      Values = new CircularBuffer<double>(capacity);
      LinePen = linePen;
      FillBrush = fillBrush;
    }
  }

  /// <summary>
  /// True while rendering is suspended because the control is off-screen. When true, calls to 
  /// InvalidateVisual() are ignored, and a flag is set to request a render when the control becomes visible again.
  /// </summary>
  private bool _renderSuspended;

  /// <summary>
  /// True when a sample arrived while suspended, so a repaint is owed on the next show.
  /// </summary>
  private bool _pendingRender;

  /// <summary>
  /// Creates a graph with the default 60-sample history and a 60-column grid.
  /// </summary>
  public PerformanceGraph() : this(DefaultHistoryLength, DefaultGridColumns) { }

  /// <summary>
  /// Creates a graph whose sample history and grid density are set independently: the number of grid
  /// columns is purely cosmetic and has no effect on where samples land — only
  /// <paramref name="historyLength"/> (the ring buffer's capacity) does that.
  /// </summary>
  public PerformanceGraph(int historyLength, int gridColumns) {
    if (historyLength <= 0) throw new ArgumentOutOfRangeException(nameof(historyLength), "History length must be positive.");
    if (gridColumns <= 0) throw new ArgumentOutOfRangeException(nameof(gridColumns), "Grid column count must be positive.");

    _historyLength = historyLength;
    _values = new CircularBuffer<double>(historyLength);

    SetCurrentValue(HistoryLengthProperty, historyLength);
    SetCurrentValue(GridColumnsProperty, gridColumns);

    _gridRender = new GridRenderer(GridRows, GridColumns);

    // A live, empty series collection by default so plain XAML population works without {Binding} —
    // SetCurrentValue leaves a later Style/Binding/XAML attribute free to override it.
    SetCurrentValue(SeriesProperty, new ObservableCollection<DataSeries>());

    SnapsToDevicePixels = true;
    UseLayoutRounding = true;

    IsVisibleChanged += (_, e) => ApplyVisibility((bool)e.NewValue);
    ApplyVisibility(IsVisible);
  }

  /// <summary>
  /// True while rendering is suspended because the control is off-screen.
  /// </summary>
  internal bool RenderSuspended => _renderSuspended;

  /// <summary>
  /// True when a sample arrived while suspended, so a repaint is owed on the next show.
  /// </summary>
  internal bool HasPendingRender => _pendingRender;

  /// <summary>
  /// Applies the visibility state to the control, suspending or resuming rendering as appropriate.
  /// </summary>
  /// <param name="visible">Whether the control is visible.</param>
  internal void ApplyVisibility(bool visible) {
    _renderSuspended = !visible;
    if (visible && _pendingRender) {
      _pendingRender = false;
      InvalidateVisual();
    }
  }

  /// <summary>
  /// Queue a repaint, unless the control is off-screen — then just remember one is owed. Internal so
  /// DataSeries can request a repaint from its own AddValue/property changes.
  /// </summary>
  internal void RequestRender() {
    if (_renderSuspended) {
      _pendingRender = true;
      return;
    }
    InvalidateVisual();
  }

  /// <summary>
  /// Number of samples retained/plotted — independent of <see cref="GridColumns"/>.
  /// </summary>
  public int Capacity => _historyLength;

  /// <summary>
  /// Identifies the <see cref="GridColumns"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty GridColumnsProperty = DependencyProperty.Register(
    nameof(GridColumns),
    typeof(int),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(DefaultGridColumns, FrameworkPropertyMetadataOptions.AffectsRender, OnGridChanged),
    value => value is int c && c > 0);

  /// <summary>
  /// Number of vertical grid lines drawn — a purely cosmetic density, independent of <see cref="Capacity"/>.
  /// </summary>
  public int GridColumns {
    get => (int)GetValue(GridColumnsProperty);
    set => SetValue(GridColumnsProperty, value);
  }

  /// <summary>
  /// Identifies the <see cref="GridRows"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty GridRowsProperty = DependencyProperty.Register(
    nameof(GridRows),
    typeof(int),
    typeof(PerformanceGraph),
    new FrameworkPropertyMetadata(DefaultGridRows, FrameworkPropertyMetadataOptions.AffectsRender, OnGridChanged),
    value => value is int r && r > 0);

  /// <summary>
  /// Number of horizontal grid lines drawn — purely cosmetic.
  /// </summary>
  public int GridRows {
    get => (int)GetValue(GridRowsProperty);
    set => SetValue(GridRowsProperty, value);
  }

  /// <summary>
  /// Rebuilds the <see cref="GridRenderer"/> when either <see cref="GridRows"/> or <see cref="GridColumns"/>
  /// </summary>
  /// <param name="d">d</param>
  /// <param name="e">e</param>
  private static void OnGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._gridRender = new GridRenderer(graph.GridRows, graph.GridColumns);
  }

  /// <summary>
  /// The Height/Width ratio that makes every grid cell come out a perfect square for a graph with
  /// the given <paramref name="gridRows"/>/<paramref name="gridColumns"/>, matching exactly what
  /// <see cref="GridRenderer"/> computes internally. Multiply by an actual pixel width to get the
  /// height that squares every cell at that width.
  /// </summary>
  public static double SquareGridAspectRatio(int gridRows, int gridColumns) {
    if (gridRows <= 0) throw new ArgumentOutOfRangeException(nameof(gridRows), "Grid row count must be positive.");
    if (gridColumns <= 0) throw new ArgumentOutOfRangeException(nameof(gridColumns), "Grid column count must be positive.");
    return gridRows / (double)gridColumns;
  }

  /// <summary>
  /// The Height/Width ratio that makes every rendered <see cref="DisplayMode.Dot"/> come out
  /// perfectly square for a graph with the given <paramref name="rows"/>/<paramref name="capacity"/> —
  /// the actual drawn dot after <see cref="ColumnWidthRatio"/>/<see cref="DotSizeRatio"/> shrink each
  /// cell. Multiply by an actual pixel width to get the height that squares every dot at that width.
  /// </summary>
  public static double SquareDotAspectRatio(int rows, int capacity) {
    if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be positive.");
    if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
    return (ColumnWidthRatio / DotSizeRatio) * (rows / (double)capacity);
  }

  /// <summary>
  /// Binds the primary series' (index 0) data to an <see cref="ObservableCollection{T}"/> of
  /// <see cref="double"/> instead of driving it imperatively via <see cref="AddValue(double)"/>.
  /// Setting this property clears the primary series and seeds it with the collection's current
  /// contents, then appends items from subsequent <see cref="INotifyCollectionChanged"/>
  /// notifications through the same <see cref="AddValue(double)"/> path.
  /// </summary>
  public ObservableCollection<double>? ValuesSource {
    get => (ObservableCollection<double>?)GetValue(ValuesSourceProperty);
    set => SetValue(ValuesSourceProperty, value);
  }

  /// <summary>
  /// Called when the <see cref="ValuesSource"/> property changes. Unhooks from the old collection's
  /// <see cref="INotifyCollectionChanged"/> event and hooks into the new one.
  /// </summary>
  /// <param name="d">The dependency object.</param>
  /// <param name="e">The property changed event arguments.</param>
  private static void OnValuesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;

    if (e.OldValue is ObservableCollection<double> oldSource)
      oldSource.CollectionChanged -= graph.OnValuesSourceCollectionChanged;

    graph.ClearPrimarySeries();

    if (e.NewValue is ObservableCollection<double> newSource) {
      foreach (double value in newSource) graph.AddValue(value);
      newSource.CollectionChanged += graph.OnValuesSourceCollectionChanged;
    }
  }

  /// <summary>
  /// Called when the <see cref="ValuesSource"/> collection changes. 
  /// Appends new items to the primary series.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnValuesSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
    if (e.Action == NotifyCollectionChangedAction.Reset) {
      ClearPrimarySeries();
      if (sender is ObservableCollection<double> source)
        foreach (double value in source) AddValue(value);
      return;
    }

    if (e.NewItems == null) return;
    foreach (double value in e.NewItems) AddValue(value);
  }

  /// <summary>
  /// Clears the primary series (index 0) of all samples. If called from a non-UI thread, it
  /// will be invoked on the UI thread.
  /// </summary>
  private void ClearPrimarySeries() {
    if (!CheckAccess()) {
      Dispatcher.BeginInvoke(ClearPrimarySeries);
      return;
    }
    _values.Clear();
    RequestRender();
  }

  /// <summary>
  /// Selects how buffered samples are drawn: a filled <see cref="DisplayMode.Line"/>, a
  /// <see cref="DisplayMode.Dot"/> matrix, or several <see cref="DisplayMode.MultipleLine"/> lines.
  /// </summary>
  public DisplayMode DisplayMode {
    get => (DisplayMode)GetValue(DisplayModeProperty);
    set => SetValue(DisplayModeProperty, value);
  }

  /// <summary>
  /// Whether the outer border is drawn. False suppresses it regardless of
  /// <see cref="BorderBrush"/>/<see cref="BorderThickness"/>.
  /// </summary>
  public bool Border {
    get => (bool)GetValue(BorderProperty);
    set => SetValue(BorderProperty, value);
  }

  /// <summary>
  /// Whether the grid lines are drawn. False suppresses them regardless of
  /// <see cref="GridBrush"/>/<see cref="GridThickness"/>.
  /// </summary>
  public bool Grid {
    get => (bool)GetValue(GridProperty);
    set => SetValue(GridProperty, value);
  }

  /// <summary>
  /// When true, the <see cref="DisplayMode.Line"/> primary series is colored by value using a
  /// green→red gauge ramp for both the stroke and the area beneath it. Applies only to a
  /// single-series Line graph — a graph carrying overlay series keeps its per-series colors.
  /// Defaults to false.
  /// </summary>
  public bool BandedLine {
    get => (bool)GetValue(BandedLineProperty);
    set => SetValue(BandedLineProperty, value);
  }

  /// <summary>
  /// Per-instance override for the banded <see cref="DisplayMode.Line"/> gauge ramp: an
  /// array of exactly <see cref="GaugeBandPalette.BandCount"/> solid brushes (band 0 lowest). Null
  /// or a wrong-length array falls back to the shared green→red ramp.
  /// </summary>
  public Brush[]? BandColors {
    get => (Brush[]?)GetValue(BandColorsProperty);
    set => SetValue(BandColorsProperty, value);
  }

  /// <summary>
  /// When greater than 0, the graph plots samples at this fixed pixel pitch and draws only
  /// the most recent that fit the width — shared by Line and Dot modes so toggling between them
  /// shows the same time window. 0 (the default) spreads all samples across the full width.
  /// </summary>
  public double CellPitch {
    get => (double)GetValue(CellPitchProperty);
    set => SetValue(CellPitchProperty, value);
  }

  /// <summary>
  /// When true, <see cref="DisplayMode.Dot"/> dots stack from the top row downward instead
  /// of the bottom row upward. Has no effect on the other modes.
  /// </summary>
  public bool Flip {
    get => (bool)GetValue(FlipProperty);
    set => SetValue(FlipProperty, value);
  }

  /// <summary>
  /// Values at or below this map to the bottom edge of the plot area.
  /// </summary>
  public double MinValue {
    get => (double)GetValue(MinValueProperty);
    set => SetValue(MinValueProperty, value);
  }

  /// <summary>
  /// Values at or above this map to the top edge of the plot area.
  /// </summary>
  public double MaxValue {
    get => (double)GetValue(MaxValueProperty);
    set => SetValue(MaxValueProperty, value);
  }

  /// <summary>
  /// Number of samples retained and plotted across the width. Changing it rebuilds the sample
  /// buffer, keeping the most recent samples that still fit. Must be positive.
  /// </summary>
  public int HistoryLength {
    get => (int)GetValue(HistoryLengthProperty);
    set => SetValue(HistoryLengthProperty, value);
  }

  /// <summary>
  /// Stroke color/brush of the data line.
  /// </summary>
  public Brush LineBrush {
    get => (Brush)GetValue(LineBrushProperty);
    set => SetValue(LineBrushProperty, value);
  }

  /// <summary>
  /// Stroke thickness of the data line.
  /// </summary>
  public double LineThickness {
    get => (double)GetValue(LineThicknessProperty);
    set => SetValue(LineThicknessProperty, value);
  }

  /// <summary>
  /// Fill brush painted under the data line, down to the baseline.
  /// </summary>
  public Brush FillBrush {
    get => (Brush)GetValue(FillBrushProperty);
    set => SetValue(FillBrushProperty, value);
  }

  /// <summary>
  /// Solid backdrop painted behind the grid and data.
  /// </summary>
  public Brush GraphBackground {
    get => (Brush)GetValue(GraphBackgroundProperty);
    set => SetValue(GraphBackgroundProperty, value);
  }

  /// <summary>
  /// Brush used for the grid lines.
  /// </summary>
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
  /// markers.</summary>
  public Brush? MarkerBrush {
    get => (Brush?)GetValue(MarkerBrushProperty);
    set => SetValue(MarkerBrushProperty, value);
  }

  /// <summary>Data value at which to draw the low marker line. <see cref="double.NaN"/> draws
  /// nothing.</summary>
  public double LowMarker {
    get => (double)GetValue(LowMarkerProperty);
    set => SetValue(LowMarkerProperty, value);
  }

  /// <summary>Data value at which to draw the high marker line. <see cref="double.NaN"/> draws
  /// nothing.</summary>
  public double HighMarker {
    get => (double)GetValue(HighMarkerProperty);
    set => SetValue(HighMarkerProperty, value);
  }

  /// <summary>Numeric format string for the value printed beside each marker line. Null draws the
  /// lines without labels.</summary>
  public string? MarkerFormat {
    get => (string?)GetValue(MarkerFormatProperty);
    set => SetValue(MarkerFormatProperty, value);
  }

  /// <summary>
  /// Vertical dot resolution in <see cref="DisplayMode.Dot"/> mode — how many rows of dots
  /// a fully-lit column draws, and the resolution of the 9-band coloring. Ignored while
  /// <see cref="CellPitch"/> is set (rows follow from the pitch). Defaults to 10.
  /// </summary>
  public int Rows {
    get => (int)GetValue(RowsProperty);
    set => SetValue(RowsProperty, value);
  }

  /// <summary>
  /// Selects whether <see cref="DisplayMode.Dot"/> dots are colored by the
  /// <see cref="Color1"/>..<see cref="Color9"/> value bands (<see cref="DotColorMode.Banded"/>, the
  /// default) or by a single flat <see cref="DotColor"/> (<see cref="DotColorMode.SingleColor"/>).
  /// </summary>
  public DotColorMode ColorMode {
    get => (DotColorMode)GetValue(ColorModeProperty);
    set => SetValue(ColorModeProperty, value);
  }

  /// <summary>
  /// Uniform corner radius, in pixels, applied to every <see cref="DisplayMode.Dot"/> dot.
  /// Defaults to 0 (sharp corners); half the dot size gives a fully round dot.
  /// </summary>
  public double CornerRadius {
    get => (double)GetValue(CornerRadiusProperty);
    set => SetValue(CornerRadiusProperty, value);
  }

  /// <summary>
  /// The single color used for every dot when <see cref="ColorMode"/> is <see cref="DotColorMode.SingleColor"/>.
  /// </summary>
  public Brush DotColor {
    get => (Brush)GetValue(DotColorProperty);
    set => SetValue(DotColorProperty, value);
  }

  /// <summary>
  /// Color for the lowest (1st) of the 9 Dot-mode value bands. Defaults to green.
  /// </summary>
  public Brush Color1 { get => (Brush)GetValue(Color1Property); set => SetValue(Color1Property, value); }

  /// <summary>
  /// Color for the 2nd-lowest of the 9 Dot-mode value bands.
  /// </summary>
  public Brush Color2 { get => (Brush)GetValue(Color2Property); set => SetValue(Color2Property, value); }

  /// <summary>
  /// Color for the 3rd of the 9 Dot-mode value bands.
  /// </summary>
  public Brush Color3 { get => (Brush)GetValue(Color3Property); set => SetValue(Color3Property, value); }

  /// <summary>
  /// Color for the 4th of the 9 Dot-mode value bands.
  /// </summary>
  public Brush Color4 { get => (Brush)GetValue(Color4Property); set => SetValue(Color4Property, value); }

  /// <summary>
  /// Color for the middle (5th) of the 9 Dot-mode value bands. Defaults to yellow.
  /// </summary>
  public Brush Color5 { get => (Brush)GetValue(Color5Property); set => SetValue(Color5Property, value); }

  /// <summary>
  /// Color for the 6th of the 9 Dot-mode value bands.
  /// </summary>
  public Brush Color6 { get => (Brush)GetValue(Color6Property); set => SetValue(Color6Property, value); }

  /// <summary>
  /// Color for the 7th of the 9 Dot-mode value bands.
  /// </summary>
  public Brush Color7 { get => (Brush)GetValue(Color7Property); set => SetValue(Color7Property, value); }

  /// <summary>
  /// Color for the 8th of the 9 Dot-mode value bands.
  /// </summary>
  public Brush Color8 { get => (Brush)GetValue(Color8Property); set => SetValue(Color8Property, value); }

  /// <summary>
  /// Color for the highest (9th) of the 9 Dot-mode value bands. Defaults to red.
  /// </summary>
  public Brush Color9 { get => (Brush)GetValue(Color9Property); set => SetValue(Color9Property, value); }

  /// <summary>
  /// Convenience accent color for the trace. In Line mode it's the line color; in Dot mode
  /// the flat dot color. Both honor it only when <see cref="BandedLine"/> is false — with banding on,
  /// each mode paints its value-banded gauge ramp instead. Setting it assigns <see cref="LineBrush"/>
  /// and <see cref="DotColor"/>.
  /// </summary>
  public Color Accent {
    get => (Color)GetValue(AccentProperty);
    set => SetValue(AccentProperty, value);
  }

  /// <summary>
  /// Handles changes to <see cref="Accent"/>: builds a solid brush from the new color and applies it to. 
  /// </summary>
  /// <param name="d">d</param>
  /// <param name="e">e</param>
  private static void OnAccentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    var color = (Color)e.NewValue;
    var solid = new SolidColorBrush(color);
    solid.Freeze();
    graph.LineBrush = solid;
    graph.DotColor = solid;
    graph.FillBrush = CreateVerticalGlow(color);
  }

  /// <summary>
  /// Creates a vertical glow brush from the given accent color, for the un-banded filled Line.
  /// </summary>
  /// <param name="accent">The accent color.</param>
  /// <returns>The vertical glow brush.</returns>
  private static Brush CreateVerticalGlow(Color accent) {
    var brush = new LinearGradientBrush {
      StartPoint = new Point(0, 0),
      EndPoint = new Point(0, 1)
    };
    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xC0, accent.R, accent.G, accent.B), 0));
    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x50, accent.R, accent.G, accent.B), 1));
    brush.Freeze();
    return brush;
  }

  /// <summary>
  /// With <see cref="BandEndColor"/>, overrides the built-in green→red gauge ramp used when
  /// <see cref="BandedLine"/> is true: the range is banded as a linear interpolation from this color
  /// (lowest) to <see cref="BandEndColor"/> (highest), applied in both Line and Dot modes. Both
  /// endpoints must be set for the override to take effect.
  /// </summary>
  public Color? BandStartColor {
    get => (Color?)GetValue(BandStartColorProperty);
    set => SetValue(BandStartColorProperty, value);
  }

  /// <summary>
  /// The high-value endpoint of the custom gauge ramp; see <see cref="BandStartColor"/>.
  /// </summary>
  public Color? BandEndColor {
    get => (Color?)GetValue(BandEndColorProperty);
    set => SetValue(BandEndColorProperty, value);
  }

  /// <summary>
  /// Handles changes to <see cref="BandStartColor"/> or <see cref="BandEndColor"/>: 
  /// builds a custom band ramp from BandStartColor→BandEndColor and applies it to both the banded
  /// Line (BandColors) and the Dot matrix (Color1..9), so one setting tints both modes identically.
  /// </summary>
  /// <param name="d">The dependency object.</param>
  /// <param name="e">The dependency property changed event arguments.</param>
  private static void OnBandRampChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    if (graph.BandStartColor is { } start && graph.BandEndColor is { } end) {
      Brush[] ramp = GaugeBandPalette.BuildSolidRamp(start, end);
      graph.BandColors = ramp;
      for (int i = 0; i < BandCount; i++) graph._resolvedColors[i] = ResolveSolidBrush(ramp[i]);
    }
    else {
      graph.BandColors = null;
      for (int i = 0; i < BandCount; i++) graph._resolvedColors[i] = ResolveSolidBrush(DefaultBandColors[i]);
    }
    graph.RequestRender();
  }

  /// <summary>
  /// The lines this graph plots in <see cref="DisplayMode.MultipleLine"/> mode. Populate in
  /// XAML directly or assign/bind a collection built in code-behind.
  /// </summary>
  public ObservableCollection<DataSeries> Series {
    get => (ObservableCollection<DataSeries>)GetValue(SeriesProperty);
    set => SetValue(SeriesProperty, value);
  }

  /// <summary>
  /// Handles the <see cref="Series"/> property changing: detaches any old series and attaches any new ones.
  /// </summary>
  /// <param name="d">The dependency object.</param>
  /// <param name="e">The dependency property changed event arguments.</param>
  private static void OnSeriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;

    if (e.OldValue is ObservableCollection<DataSeries> oldSeries) {
      oldSeries.CollectionChanged -= graph.OnSeriesCollectionChanged;
      foreach (var s in oldSeries) s.Detach();
    }

    if (e.NewValue is ObservableCollection<DataSeries> newSeries) {
      foreach (var s in newSeries) s.Attach(graph, graph._historyLength);
      newSeries.CollectionChanged += graph.OnSeriesCollectionChanged;
    }

    graph.RequestRender();
  }

  /// <summary>
  /// Handles the <see cref="Series"/> collection changing: detaches any removed series and attaches any new ones.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
    if (e.OldItems != null)
      foreach (DataSeries s in e.OldItems) s.Detach();

    if (e.NewItems != null)
      foreach (DataSeries s in e.NewItems) s.Attach(this, _historyLength);

    RequestRender();
  }

  /// <summary>
  /// Applies every property the given theme sets, leaving anything it leaves null untouched.
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

  /// <summary>
  /// Number of overlay series (Line mode): the primary (index 0) plus any via <see cref="AddSeries"/>.
  /// </summary>
  internal int SeriesCount => 1 + _extraSeries.Count;

  /// <summary>
  /// Registers an additional line series overlaid on the primary one (Line mode), returning the
  /// index used to feed it via <see cref="AddValue(int, double)"/>. The primary series is index 0.
  /// </summary>
  public int AddSeries(Brush lineBrush, Brush? fillBrush = null, double thickness = 2) {
    _extraSeries.Add(new OverlaySeries(_historyLength, Helpers.CreateFrozenPen(lineBrush, thickness), fillBrush));
    RequestRender();
    return _extraSeries.Count;
  }

  /// <summary>
  /// Appends a new sample to the primary series (index 0). O(1).
  /// </summary>
  public void AddValue(double value) => AddValue(0, value);

  /// <summary>
  /// Appends a new sample to the given series, dropping the oldest once <see cref="Capacity"/> is
  /// exceeded. Series 0 is the primary; 1..N are overlays returned by <see cref="AddSeries"/>. O(1).
  /// </summary>
  public void AddValue(int series, double value) {
    if (!CheckAccess()) {
      Dispatcher.BeginInvoke(() => AddValue(series, value));
      return;
    }
    BufferFor(series).Add(value);
    RequestRender();
  }

  /// <summary>
  /// Discards all buffered samples across every series (primary, overlays, and <see cref="Series"/>).
  /// </summary>
  public void ClearValues() {
    if (!CheckAccess()) {
      Dispatcher.BeginInvoke(ClearValues);
      return;
    }
    _values.Clear();
    foreach (var s in _extraSeries) s.Values.Clear();
    foreach (var s in Series) s.ClearValues();
    RequestRender();
  }

  /// <summary>
  /// Removes every overlay series and <see cref="DataSeries"/>, and clears all buffered
  /// samples, returning the graph to an empty primary-only state — for re-pointing one graph
  /// instance at a different data source. Overlay series must be re-registered afterwards.
  /// </summary>
  public void Reset() {
    if (!CheckAccess()) {
      Dispatcher.BeginInvoke(Reset);
      return;
    }
    _values.Clear();
    _extraSeries.Clear();
    Series.Clear();
    RequestRender();
  }

  /// <summary>
  /// Returns the sample buffer for the given series index: 0 is the primary, 1..N are overlays
  /// </summary>
  /// <param name="series">The series index.</param>
  /// <returns>The sample buffer for the specified series.</returns>
  private CircularBuffer<double> BufferFor(int series) =>
      series == 0 ? _values : _extraSeries[series - 1].Values;

  /// <summary>
  /// Sets the line pen to a frozen version of the given brush and the current thickness, or null if the brush is null.
  /// </summary>
  /// <param name="d">The PerformanceGraph instance.</param>
  /// <param name="e">The dependency property changed event arguments.</param>
  private static void OnLineBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.LinePen = Helpers.CreateFrozenPen((Brush)e.NewValue, graph._graphStyle.LinePen.Thickness);
  }

  /// <summary>
  /// Sets the line pen to a frozen version of the given brush and thickness, or null if the brush is null.
  /// </summary>
  /// <param name="d">The PerformanceGraph instance.</param>
  /// <param name="e">The dependency property changed event arguments.</param>
  private static void OnLineThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.LinePen = Helpers.CreateFrozenPen(graph._graphStyle.LinePen.Brush, (double)e.NewValue);
  }

  /// <summary>
  /// Sets the fill brush to a frozen version of the given brush, or null if the brush is null.
  /// </summary>
  /// <param name="d">The PerformanceGraph instance.</param>
  /// <param name="e">The dependency property changed event arguments.</param>
  private static void OnFillBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((PerformanceGraph)d)._graphStyle.FillBrush = (Brush)e.NewValue;

  /// <summary>
  /// Rebuilds the primary series' sample buffer to the new capacity, keeping the most recent samples that still fit. 
  /// Also resizes every overlay series and <see cref="DataSeries"/> to match. O(N) in the number of samples retained.
  /// </summary>
  /// <param name="d">The PerformanceGraph instance.</param>
  /// <param name="e">The dependency property changed event arguments.</param>
  private static void OnHistoryLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    int newLength = (int)e.NewValue;
    if (newLength == graph._historyLength) return; // e.g. the constructor's own sync set

    graph._historyLength = newLength;
    graph._values = CopyMostRecent(graph._values, newLength);
    foreach (var s in graph._extraSeries)
      s.Values = CopyMostRecent(s.Values, newLength);
    foreach (var s in graph.Series)
      s.Resize(newLength);
  }

  /// <summary>
  /// Returns a new buffer of the given capacity, seeded with the most recent samples from the source.
  /// </summary>
  /// <param name="source">The source buffer to copy from.</param>
  /// <param name="newCapacity">The capacity of the new buffer.</param>
  /// <returns>A new circular buffer with the most recent samples.</returns>
  private static CircularBuffer<double> CopyMostRecent(CircularBuffer<double> source, int newCapacity) {
    var next = new CircularBuffer<double>(newCapacity);
    int start = source.Count > newCapacity ? source.Count - newCapacity : 0;
    for (int i = start; i < source.Count; i++) next.Add(source[i]);
    return next;
  }

  /// <summary>
  /// Sets the graph background brush to the given value. Null is allowed, but the graph will be transparent in that case.
  /// </summary>
  /// <param name="d">d</param>
  /// <param name="e">e</param>
  private static void OnGraphBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((PerformanceGraph)d)._graphStyle.BackgroundBrush = (Brush)e.NewValue;

  /// <summary>
  /// Sets the grid pen to a frozen version of the given brush and the current thickness, or null if the brush is null.
  /// </summary>
  /// <param name="d">d</param>
  /// <param name="e">e</param>
  private static void OnGridBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.GridPen = Helpers.CreateFrozenPen((Brush)e.NewValue, graph._graphStyle.GridPen.Thickness);
  }

  /// <summary>
  /// Sets the grid pen to a frozen version of the given brush and thickness, or null if the brush is null.
  /// </summary>
  /// <param name="d">d</param>
  /// <param name="e">e</param>
  private static void OnGridThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.GridPen = Helpers.CreateFrozenPen(graph._graphStyle.GridPen.Brush, (double)e.NewValue);
  }

  /// <summary>
  /// Sets the border pen to a frozen version of the given brush and the current thickness, or null if the brush is null.
  /// </summary>
  /// <param name="d">d</param>
  /// <param name="e">e</param>
  private static void OnBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.BorderPen = Helpers.CreateFrozenPen((Brush)e.NewValue, graph._graphStyle.BorderPen.Thickness);
  }

  /// <summary>
  /// Sets the border pen to a frozen version of the given brush and thickness, or null if the brush is null.
  /// </summary>
  /// <param name="d">d</param>
  /// <param name="e">e</param>
  private static void OnBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    double thickness = (double)e.NewValue;
    graph._graphStyle.BorderThickness = thickness;
    graph._graphStyle.BorderPen = Helpers.CreateFrozenPen(graph._graphStyle.BorderPen.Brush, thickness);
  }

  /// <summary>
  /// Sets the marker line pen to a dashed version of the given brush, or null if the brush is null.
  /// </summary>
  /// <param name="d">d</param>
  /// <param name="e">e</param>
  private static void OnMarkerBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraph)d;
    graph._graphStyle.MarkerPen = e.NewValue is Brush brush ? Helpers.CreateDashedPen(brush, 2) : null;
  }

  /// <summary>
  /// Returns a frozen solid brush that matches the most-opaque stop of the given brush, 
  /// or the brush itself if it's already solid. Freezes the returned brush if it can be frozen.
  /// </summary>
  /// <param name="source"></param>
  /// <returns></returns>
  private static Brush ResolveSolidBrush(Brush source) {
    if (source is not GradientBrush gradient || gradient.GradientStops.Count == 0) {
      if (source.CanFreeze && !source.IsFrozen) source.Freeze();
      return source;
    }

    GradientStop pick = gradient.GradientStops[0];
    foreach (GradientStop stop in gradient.GradientStops)
      if (stop.Color.A > pick.Color.A) pick = stop;

    var solid = new SolidColorBrush(Color.FromRgb(pick.Color.R, pick.Color.G, pick.Color.B));
    solid.Freeze();
    return solid;
  }

  // Which of the 9 color bands a given row belongs to, by the row's position in the value scale.
  private static int BandForRow(int row, int rows) {
    double rowFraction = (row + 0.5) / rows;
    int band = (int)(rowFraction * BandCount);
    return band < 0 ? 0 : (band >= BandCount ? BandCount - 1 : band);
  }

  protected override Size MeasureOverride(Size availableSize) {
    const double PixelsPerUnit = 12;
    double pitch = CellPitch > 0 ? CellPitch : PixelsPerUnit;
    int rowsFallback = DisplayMode == DisplayMode.Dot ? Rows : GridRows;

    double width = double.IsInfinity(availableSize.Width) ? Capacity * pitch : availableSize.Width;
    double height = double.IsInfinity(availableSize.Height) ? rowsFallback * pitch : availableSize.Height;
    return new Size(width, height);
  }

  protected override Size ArrangeOverride(Size finalSize) {
    return finalSize;
  }

  /// <summary>
  /// Renders the graph in the given <see cref="DrawingContext"/>. 
  /// The render order is: background, grid, data, session-extreme markers, border. 
  /// The grid and border are drawn only when their respective toggles are on.
  /// </summary>
  /// <param name="dc">The drawing context.</param>
  protected override void OnRender(DrawingContext dc) {
    base.OnRender(dc);

    Rect bounds = new(RenderSize);
    if (bounds.Width <= 0 || bounds.Height <= 0) return;

    // Background fills the whole control first, behind everything else.
    _backgroundRender.Draw(dc, bounds, _graphStyle);

    // Grid on top of the background — only when the Grid toggle is on.
    if (Grid) _gridRender.Draw(dc, bounds, _graphStyle);

    double minValue = MinValue;
    double maxValue = MaxValue;

    switch (DisplayMode) {
      case DisplayMode.Dot:
        RenderDots(dc, bounds, minValue, maxValue);
        break;
      case DisplayMode.MultipleLine:
        double seriesPitch = CellPitch;
        foreach (var series in Series) {
          if (series.Buffer == null) continue;
          series.Renderer.Draw(dc, bounds, series.Buffer, _historyLength, minValue, maxValue,
              series.ResolvedLinePen, series.EffectiveFillBrush, cellPitch: seriesPitch);
        }
        break;
      default:
        // Primary series first (fill underneath), then each overlay. Banding is a single-series
        // look, so gate it on there being no overlay series.
        bool banded = BandedLine && _extraSeries.Count == 0;
        double cellPitch = CellPitch;
        (_filledLineRender ??= new FilledLineRenderer()).Draw(dc, bounds, _values, _historyLength, minValue, maxValue,
            _graphStyle.LinePen, _graphStyle.FillBrush,
            banded ? BandLinePens() : null, banded ? BandFills() : null, cellPitch);
        foreach (var s in _extraSeries)
          s.Renderer.Draw(dc, bounds, s.Values, _historyLength, minValue, maxValue,
              s.LinePen, s.FillBrush, cellPitch: cellPitch);
        break;
    }

    // Session-extreme markers over the data but under the border. Only graphs that opt in via
    // MarkerBrush draw these.
    if (_graphStyle.MarkerPen != null) {
      double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
      string? markerFormat = MarkerFormat;
      double lowMarker = LowMarker;
      double highMarker = HighMarker;
      var marker = _markerRender ??= new MarkerRenderer();
      marker.Draw(dc, bounds, _graphStyle, lowMarker, minValue, maxValue,
          FormatMarker(lowMarker, markerFormat), topBiased: false, dpi);
      marker.Draw(dc, bounds, _graphStyle, highMarker, minValue, maxValue,
          FormatMarker(highMarker, markerFormat), topBiased: true, dpi);
    }

    // Border drawn last so its edge stays crisp — only when the Border toggle is on.
    if (Border) _borderRender.Draw(dc, bounds, _graphStyle);
  }

  /// <summary>
  /// Dot-matrix render. 
  /// Renders the dot-matrix in either single-color or banded mode, depending on the <see cref="ColorMode"/> property.
  /// Layout math ported from the former PerformanceGraphLite; the two color paths
  /// are genuinely different (SingleColor never touches BandForRow nor allocates the band geometries).
  /// </summary>
  /// <param name="dc">The drawing context.</param>
  /// <param name="bounds">The bounds of the rendering area.</param>
  /// <param name="minValue">The minimum value of the data range.</param>
  /// <param name="maxValue">The maximum value of the data range.</param>
  private void RenderDots(DrawingContext dc, Rect bounds, double minValue, double maxValue) {
    int count = _values.Count;
    double range = maxValue - minValue;
    if (count == 0 || range <= 0) return;

    int rows;
    int startIndex = 0;
    double slotWidth;
    double rowHeight;
    double pitch = CellPitch;
    if (pitch > 0) {
      int cols = Math.Max(1, (int)Math.Round(bounds.Width / pitch));
      rows = Math.Max(1, (int)Math.Round(bounds.Height / pitch));
      slotWidth = bounds.Width / cols;
      rowHeight = bounds.Height / rows;
      if (count > cols) startIndex = count - cols;
    }
    else {
      rows = Rows;
      if (rows <= 0) return;
      int effectiveCapacity = Capacity > count ? Capacity : count;
      slotWidth = bounds.Width / effectiveCapacity;
      rowHeight = bounds.Height / rows;
    }

    double dotColumnWidth = slotWidth * ColumnWidthRatio;
    double columnInset = (slotWidth - dotColumnWidth) / 2;
    double dotSize = rowHeight * DotSizeRatio;
    if (dotSize > dotColumnWidth) dotSize = dotColumnWidth;
    double rowPadding = (rowHeight - dotSize) / 2;

    var layout = new DotLayout(slotWidth, dotColumnWidth, columnInset, rowHeight, dotSize, rowPadding);
    bool flip = Flip;
    double cornerRadius = CornerRadius;

    if (ColorMode == DotColorMode.SingleColor)
      RenderDotsSingleColor(dc, bounds, count, startIndex, rows, minValue, range, in layout, flip, cornerRadius);
    else
      RenderDotsBanded(dc, bounds, count, startIndex, rows, minValue, range, in layout, flip, cornerRadius);
  }

  /// <summary>
  /// Encapsulates the layout parameters for the dot-matrix rendering, including slot width, dot size, row height, and padding.
  /// </summary>
  private readonly struct DotLayout {
    /// <summary>
    /// Initializes a new instance of the <see cref="DotLayout"/> struct with the specified layout parameters.
    /// </summary>
    /// <param name="slotWidth">The width of each slot.</param>
    /// <param name="dotColumnWidth">The width of the column containing the dots.</param>
    /// <param name="columnInset">The inset for the column.</param>
    /// <param name="rowHeight">The height of each row.</param>
    /// <param name="dotSize">The size of each dot.</param>
    /// <param name="rowPadding">The padding for each row.</param>
    public DotLayout(double slotWidth, double dotColumnWidth, double columnInset,
        double rowHeight, double dotSize, double rowPadding) {
      SlotWidth = slotWidth;
      DotColumnWidth = dotColumnWidth;
      ColumnInset = columnInset;
      RowHeight = rowHeight;
      DotSize = dotSize;
      RowPadding = rowPadding;
    }

    public double SlotWidth { get; }
    public double DotColumnWidth { get; }
    public double ColumnInset { get; }
    public double RowHeight { get; }
    public double DotSize { get; }
    public double RowPadding { get; }
  }

  /// <summary>
  /// Renders the dot-matrix in single-color mode, where all dots are filled with the same color regardless of their value.
  /// </summary>
  /// <param name="dc">The drawing context to use for rendering.</param>
  /// <param name="bounds">The bounding rectangle for the dot matrix.</param>
  /// <param name="count">The number of dots to render.</param>
  /// <param name="startIndex">The index of the first dot to render.</param>
  /// <param name="rows">The number of rows in the dot matrix.</param>
  /// <param name="minValue">The minimum value for scaling.</param>
  /// <param name="range">The range of values for scaling.</param>
  /// <param name="layout">The layout parameters for the dot matrix.</param>
  /// <param name="flip">Indicates whether the dot matrix should be flipped.</param>
  /// <param name="cornerRadius">The radius of the rounded corners.</param>
  private void RenderDotsSingleColor(DrawingContext dc, Rect bounds, int count, int startIndex, int rows, double minValue,
      double range, in DotLayout layout, bool flip, double cornerRadius) {
    StreamGeometry geometry = _singleGeometry ??= new StreamGeometry();

    using (StreamGeometryContext ctx = geometry.Open()) {
      for (int i = startIndex; i < count; i++) {
        double slotRight = bounds.Right - (count - 1 - i) * layout.SlotWidth;
        double left = slotRight - layout.SlotWidth + layout.ColumnInset;
        double cx = left + layout.DotColumnWidth / 2;

        double t = (_values[i] - minValue) / range;
        t = t < 0 ? 0 : (t > 1 ? 1 : t);

        double fillHeight = t * bounds.Height;
        int fullRows = (int)(fillHeight / layout.RowHeight);
        if (fullRows > rows) fullRows = rows;

        double partialFraction = 0;
        if (fullRows < rows) {
          partialFraction = (fillHeight - fullRows * layout.RowHeight) / layout.RowHeight;
          partialFraction = partialFraction < 0 ? 0 : (partialFraction > 1 ? 1 : partialFraction);
        }

        for (int r = 0; r < fullRows; r++) {
          double top = flip
              ? bounds.Top + r * layout.RowHeight + layout.RowPadding
              : bounds.Bottom - (r + 1) * layout.RowHeight + layout.RowPadding;
          AddDotFigure(ctx, cx - layout.DotSize / 2, top, layout.DotSize, layout.DotSize, cornerRadius);
        }

        if (partialFraction > 0 && fullRows < rows) {
          double partialHeight = layout.DotSize * partialFraction;
          double top = flip
              ? bounds.Top + fullRows * layout.RowHeight + layout.RowPadding
              : bounds.Bottom - (fullRows + 1) * layout.RowHeight + layout.RowPadding + (layout.DotSize - partialHeight);
          AddDotFigure(ctx, cx - layout.DotSize / 2, top, layout.DotSize, partialHeight, cornerRadius);
        }
      }
    }

    dc.DrawGeometry(_resolvedDotColor, null, geometry);
  }

  /// <summary>
  /// Renders the dot-matrix in banded mode, where each row is colored according to its value band. 
  /// This method creates separate geometries for each band and draws them with their respective colors.
  /// </summary>
  /// <param name="dc">The drawing context to use for rendering.</param>
  /// <param name="bounds">The bounding rectangle for the dot matrix.</param>
  /// <param name="count">The number of dots to render.</param>
  /// <param name="startIndex">The index of the first dot to render.</param>
  /// <param name="rows">The number of rows in the dot matrix.</param>
  /// <param name="minValue">The minimum value for scaling.</param>
  /// <param name="range">The range of values for scaling.</param>
  /// <param name="layout">The layout parameters for the dot matrix.</param>
  /// <param name="flip">Indicates whether the dot matrix should be flipped.</param>
  /// <param name="cornerRadius">The radius of the rounded corners.</param>
  private void RenderDotsBanded(DrawingContext dc, Rect bounds, int count, int startIndex, int rows, double minValue,
      double range, in DotLayout layout, bool flip, double cornerRadius) {
    StreamGeometry[] geometries = _bandGeometries ??= CreateBandGeometries();
    var contexts = new StreamGeometryContext[BandCount];
    try {
      for (int b = 0; b < BandCount; b++) {
        contexts[b] = geometries[b].Open();
      }

      for (int i = startIndex; i < count; i++) {
        double slotRight = bounds.Right - (count - 1 - i) * layout.SlotWidth;
        double left = slotRight - layout.SlotWidth + layout.ColumnInset;
        double cx = left + layout.DotColumnWidth / 2;

        double t = (_values[i] - minValue) / range;
        t = t < 0 ? 0 : (t > 1 ? 1 : t);

        double fillHeight = t * bounds.Height;
        int fullRows = (int)(fillHeight / layout.RowHeight);
        if (fullRows > rows) fullRows = rows;

        double partialFraction = 0;
        if (fullRows < rows) {
          partialFraction = (fillHeight - fullRows * layout.RowHeight) / layout.RowHeight;
          partialFraction = partialFraction < 0 ? 0 : (partialFraction > 1 ? 1 : partialFraction);
        }

        for (int r = 0; r < fullRows; r++) {
          double top = flip
              ? bounds.Top + r * layout.RowHeight + layout.RowPadding
              : bounds.Bottom - (r + 1) * layout.RowHeight + layout.RowPadding;
          AddDotFigure(contexts[BandForRow(r, rows)], cx - layout.DotSize / 2, top, layout.DotSize, layout.DotSize, cornerRadius);
        }

        if (partialFraction > 0 && fullRows < rows) {
          double partialHeight = layout.DotSize * partialFraction;
          double top = flip
              ? bounds.Top + fullRows * layout.RowHeight + layout.RowPadding
              : bounds.Bottom - (fullRows + 1) * layout.RowHeight + layout.RowPadding + (layout.DotSize - partialHeight);
          AddDotFigure(contexts[BandForRow(fullRows, rows)], cx - layout.DotSize / 2, top, layout.DotSize, partialHeight, cornerRadius);
        }
      }
    }
    finally {
      for (int b = 0; b < BandCount; b++) contexts[b]?.Close();
    }

    for (int b = 0; b < BandCount; b++) dc.DrawGeometry(_resolvedColors[b], null, geometries[b]);
  }

  /// <summary>
  /// Adds a rounded rectangle to the given geometry context, with the given corner radius. If the
  /// corner radius is larger than half the width or height, it will be clamped to the maximum allowed value.
  /// </summary>
  /// <param name="ctx">The geometry context to which the rounded rectangle will be added.</param>
  /// <param name="left">The left coordinate of the rectangle.</param>
  /// <param name="top">The top coordinate of the rectangle.</param>
  /// <param name="width">The width of the rectangle.</param>
  /// <param name="height">The height of the rectangle.</param>
  /// <param name="cornerRadius">The radius of the rounded corners.</param>
  private static void AddDotFigure(StreamGeometryContext ctx, double left, double top, double width, double height, double cornerRadius) {
    double right = left + width;
    double bottom = top + height;

    double radius = cornerRadius;
    double maxRadius = Math.Min(width, height) / 2;
    if (radius > maxRadius) radius = maxRadius;

    if (radius <= 0) {
      ctx.BeginFigure(new Point(left, top), isFilled: true, isClosed: true);
      ctx.LineTo(new Point(right, top), isStroked: false, isSmoothJoin: false);
      ctx.LineTo(new Point(right, bottom), isStroked: false, isSmoothJoin: false);
      ctx.LineTo(new Point(left, bottom), isStroked: false, isSmoothJoin: false);
      return;
    }

    var radii = new Size(radius, radius);
    ctx.BeginFigure(new Point(left + radius, top), isFilled: true, isClosed: true);
    ctx.LineTo(new Point(right - radius, top), isStroked: false, isSmoothJoin: false);
    ctx.ArcTo(new Point(right, top + radius), radii, 0, isLargeArc: false, SweepDirection.Clockwise, isStroked: false, isSmoothJoin: false);
    ctx.LineTo(new Point(right, bottom - radius), isStroked: false, isSmoothJoin: false);
    ctx.ArcTo(new Point(right - radius, bottom), radii, 0, isLargeArc: false, SweepDirection.Clockwise, isStroked: false, isSmoothJoin: false);
    ctx.LineTo(new Point(left + radius, bottom), isStroked: false, isSmoothJoin: false);
    ctx.ArcTo(new Point(left, bottom - radius), radii, 0, isLargeArc: false, SweepDirection.Clockwise, isStroked: false, isSmoothJoin: false);
    ctx.LineTo(new Point(left, top + radius), isStroked: false, isSmoothJoin: false);
    ctx.ArcTo(new Point(left + radius, top), radii, 0, isLargeArc: false, SweepDirection.Clockwise, isStroked: false, isSmoothJoin: false);
  }

  private static string? FormatMarker(double value, string? format) =>
      format is { } f && !double.IsNaN(value)
          ? value.ToString(f, System.Globalization.CultureInfo.InvariantCulture)
          : null;
}
