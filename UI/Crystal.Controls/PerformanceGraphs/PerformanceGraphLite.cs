using Crystal.Controls.PerformanceGraphs.Buffers;
using Crystal.Controls.PerformanceGraphs.Kinds;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// A minimal, dot-only performance graph: a single-series sample buffer rendered as a
/// dot-matrix column chart (the same visual as <see cref="Kinds.GraphKind.Dot"/> on
/// <see cref="PerformanceGraph"/>), with none of the surrounding chrome.
/// </summary>
/// <remarks>
/// <para>
/// This is a deliberately separate, ground-up implementation rather than a thin wrapper around
/// <see cref="PerformanceGraph"/> — mirroring how <see cref="Comet.LivenessIndicatorLite"/> sits
/// alongside <see cref="Comet.LivenessIndicator"/> in this library: a "Lite" variant earns its
/// keep by not carrying the machinery its trimmed-down feature set doesn't need, rather than by
/// hiding that machinery behind zeroed-out properties. Compared to <see cref="PerformanceGraph"/>,
/// this control drops:
/// <list type="bullet">
///   <item>The grid and border renderers, and their pens/geometry caches, entirely — there is no
///     property that turns them on, so no code path, dependency property, or per-frame branch
///     for them exists at all.</item>
///   <item><see cref="Kinds.GraphKind"/> selection, the Line/Bar/SegmentedBar renderers, and the
///     lazy-allocate-per-Kind machinery that supports switching between them at runtime — dot is
///     the only output, so there is nothing to switch and nothing to allocate lazily.</item>
///   <item>Overlay series (<c>AddSeries</c>) and session-extreme markers — one series, no marker
///     pen, no cached <c>FormattedText</c>.</item>
///   <item>Runtime-resizable history (the <c>HistoryLength</c> dependency property and its
///     buffer-rebuild-on-change logic) — <see cref="Capacity"/> is fixed at construction, like a
///     plain <see cref="CircularBuffer{T}"/>, since a dashboard tile's sample window doesn't
///     normally change after it's placed.</item>
/// </list>
/// What's kept, because it's a real per-frame cost rather than decoration: batching every frame's
/// dots into a small, fixed number of <see cref="StreamGeometry"/>-backed draw calls (one per
/// color band actually in use, not one per dot), color brushes resolved to solid-and-frozen once
/// per change rather than per frame, and the same off-screen render suspension
/// <see cref="PerformanceGraph"/> uses (samples still land in the buffer while hidden;
/// <see cref="UIElement.InvalidateVisual"/> is never queued for a tile nobody can see).
/// </para>
/// <para>
/// <b>Value-banded coloring.</b> Each dot's color depends on which row it occupies, not on which
/// sample (column) it belongs to — the classic gauge/meter convention where "how far up the
/// scale" reads as a color regardless of which moment in time produced it. The plotted range
/// [<see cref="MinValue"/>, <see cref="MaxValue"/>] is split into 9 equal-width bands, colored by
/// <see cref="Color1"/> through <see cref="Color9"/> respectively (<see cref="Color1"/> lowest).
/// All nine default to a linear green→red ramp (<see cref="Color1"/> green, <see cref="Color9"/>
/// red), so an unconfigured graph reads as a low-to-high gauge palette out of the box; set any
/// of <see cref="Color1"/>..<see cref="Color9"/> to override an individual band. <see cref="Rows"/>
/// need not be a multiple of 9 — each row maps to whichever band its mid-height falls in.
/// </para>
/// <para>
/// <b>Fractional dot.</b> When a value doesn't land exactly on a row boundary, the fully-lit rows
/// below it are drawn as full-size dots and the one partially-reached row above them is drawn as
/// a single dot proportionally shortened to the fractional remainder (e.g. a value 70% of the way
/// into its row draws a dot 70% as tall) — the same fractional-fill technique
/// <see cref="Kinds.GraphKind.SegmentedBar"/>'s renderer already uses, rather than rounding to the
/// nearest whole dot. Width never shrinks and the dot's color still comes from whichever band its
/// row falls in; only the height changes, and it is still one solid, unstroked, axis-aligned
/// rectangle like every other dot (see <see cref="AddDotFigure"/>) - never a different shape,
/// partial opacity, or a clip.
/// </para>
/// <para>
/// <b>Color mode.</b> <see cref="ColorMode"/> selects between the value-banded coloring described
/// above (<see cref="DotColorMode.Banded"/>, the default - preserves the original behavior) and a
/// single flat <see cref="DotColor"/> (<see cref="DotColorMode.SingleColor"/>). The two modes
/// aren't just a different paint step: <see cref="DotColorMode.SingleColor"/> skips the per-row
/// band lookup entirely and batches every dot into one reused <see cref="StreamGeometry"/> with
/// exactly one <see cref="DrawingContext.DrawGeometry(Brush, Pen, Geometry)"/> call per frame,
/// instead of up to nine geometries/calls - and the nine band geometries are never even allocated
/// for a graph that stays in <see cref="DotColorMode.SingleColor"/>.
/// </para>
/// <para>
/// <b>Corner radius.</b> <see cref="CornerRadius"/> rounds every dot's corners by a single,
/// uniform pixel radius (not the four-value <see cref="System.Windows.CornerRadius"/> struct
/// <see cref="System.Windows.Controls.Border"/> uses - just a plain <see cref="double"/>, since a
/// small square dot has no meaningful use for four independent corners). Defaults to 0 (sharp
/// corners, the original unrounded look). <see cref="AddDotFigure"/> clamps the radius per dot to
/// at most half of that dot's own smaller dimension, so an oversized value can't self-intersect
/// the geometry - the practical ceiling is a fully round dot when the radius reaches half the
/// smaller side, at which point a square dot reads as a circle without a second shape or a
/// separate rendering path to switch between.
/// </para>
/// </remarks>
public sealed class PerformanceGraphLite : FrameworkElement {
  private const int DefaultCapacity = 60;
  private const int DefaultRows = 10;
  private const int BandCount = 9;

  // Fraction of each column slot's width the dot occupies, and of each row's height a
  // *full* dot occupies — the same ratios PerformanceGraph's DotRenderer uses, so a Lite graph
  // dropped next to a full one at the same capacity reads as the same visual language, column
  // for column. A fractional dot keeps this same width and starting height, just shortened.
  private const double ColumnWidthRatio = 0.7;
  private const double DotSizeRatio = 0.55;

  /// <summary>
  /// The Height/Width ratio that makes every rendered dot come out perfectly square for a graph
  /// with the given <paramref name="rows"/>/<paramref name="capacity"/> - not merely square grid
  /// cells, the actual drawn dot after <see cref="ColumnWidthRatio"/>/<see cref="DotSizeRatio"/>
  /// shrink each cell down to the dot inside it. Multiply this by an actual pixel width to get the
  /// exact height that squares every dot at that width, whatever the width turns out to be -
  /// this is the single source of truth for that math (matching exactly what <see cref="OnRender"/>
  /// itself computes), meant for a XAML binding/converter that only has ActualWidth to work with
  /// and needs to derive Height from it without duplicating or guessing these ratio constants.
  /// </summary>
  public static double SquareDotAspectRatio(int rows, int capacity) {
    if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be positive.");
    if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
    return (ColumnWidthRatio / DotSizeRatio) * (rows / (double)capacity);
  }

  // Default green→red gauge ramp (Color1 green … Color9 red). An unconfigured graph reads as a
  // linear low-to-high palette rather than flat gray. Each brush is frozen once and shared by
  // every instance's Color1..9 metadata default and its initial resolved-color slots.
  private static readonly Brush[] DefaultBandColors = CreateDefaultBandColors();

  private static Brush[] CreateDefaultBandColors() {
    (byte R, byte G, byte B)[] ramp = {
      (0x2E, 0xCC, 0x40), // Color1 - green
      (0x5F, 0xCA, 0x34),
      (0x90, 0xC8, 0x28),
      (0xC0, 0xC6, 0x1B),
      (0xF1, 0xC4, 0x0F), // Color5 - yellow
      (0xF0, 0x9A, 0x14),
      (0xEF, 0x70, 0x1A),
      (0xEE, 0x46, 0x1F),
      (0xED, 0x1C, 0x24), // Color9 - red
    };
    var brushes = new Brush[ramp.Length];
    for (int i = 0; i < ramp.Length; i++) {
      var brush = new SolidColorBrush(Color.FromRgb(ramp[i].R, ramp[i].G, ramp[i].B));
      brush.Freeze();
      brushes[i] = brush;
    }
    return brushes;
  }

  /// <summary>Identifies the <see cref="ValuesSource"/> dependency property.</summary>
  public static readonly DependencyProperty ValuesSourceProperty =
      DependencyProperty.Register(nameof(ValuesSource), typeof(ObservableCollection<double>), typeof(PerformanceGraphLite),
          new FrameworkPropertyMetadata(null, OnValuesSourceChanged));

  /// <summary>Identifies the <see cref="GraphBackground"/> dependency property.</summary>
  public static readonly DependencyProperty GraphBackgroundProperty =
      DependencyProperty.Register(nameof(GraphBackground), typeof(Brush), typeof(PerformanceGraphLite),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="MinValue"/> dependency property.</summary>
  public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(PerformanceGraphLite),
          new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="MaxValue"/> dependency property.</summary>
  public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(PerformanceGraphLite),
          new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="Rows"/> dependency property.</summary>
  public static readonly DependencyProperty RowsProperty =
      DependencyProperty.Register(nameof(Rows), typeof(int), typeof(PerformanceGraphLite),
          new FrameworkPropertyMetadata(DefaultRows, FrameworkPropertyMetadataOptions.AffectsRender),
          ValidateRows);

  private static bool ValidateRows(object value) => value is int rows && rows > 0;

  /// <summary>Identifies the <see cref="Flip"/> dependency property.</summary>
  public static readonly DependencyProperty FlipProperty =
      DependencyProperty.Register(nameof(Flip), typeof(bool), typeof(PerformanceGraphLite),
          new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="ColorMode"/> dependency property.</summary>
  public static readonly DependencyProperty ColorModeProperty =
      DependencyProperty.Register(nameof(ColorMode), typeof(DotColorMode), typeof(PerformanceGraphLite),
          new FrameworkPropertyMetadata(DotColorMode.Banded, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="CornerRadius"/> dependency property.</summary>
  public static readonly DependencyProperty CornerRadiusProperty =
      DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(PerformanceGraphLite),
          new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender),
          ValidateCornerRadius);

  private static bool ValidateCornerRadius(object value) => value is double radius && radius >= 0;

  /// <summary>Identifies the <see cref="DotColor"/> dependency property.</summary>
  public static readonly DependencyProperty DotColorProperty =
      DependencyProperty.Register(nameof(DotColor), typeof(Brush), typeof(PerformanceGraphLite),
          new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender,
              (d, e) => ((PerformanceGraphLite)d)._resolvedDotColor = ResolveSolidBrush((Brush)e.NewValue)));

  /// <summary>Identifies the <see cref="Color1"/> dependency property.</summary>
  public static readonly DependencyProperty Color1Property = RegisterBandColor(nameof(Color1), 0);
  /// <summary>Identifies the <see cref="Color2"/> dependency property.</summary>
  public static readonly DependencyProperty Color2Property = RegisterBandColor(nameof(Color2), 1);
  /// <summary>Identifies the <see cref="Color3"/> dependency property.</summary>
  public static readonly DependencyProperty Color3Property = RegisterBandColor(nameof(Color3), 2);
  /// <summary>Identifies the <see cref="Color4"/> dependency property.</summary>
  public static readonly DependencyProperty Color4Property = RegisterBandColor(nameof(Color4), 3);
  /// <summary>Identifies the <see cref="Color5"/> dependency property.</summary>
  public static readonly DependencyProperty Color5Property = RegisterBandColor(nameof(Color5), 4);
  /// <summary>Identifies the <see cref="Color6"/> dependency property.</summary>
  public static readonly DependencyProperty Color6Property = RegisterBandColor(nameof(Color6), 5);
  /// <summary>Identifies the <see cref="Color7"/> dependency property.</summary>
  public static readonly DependencyProperty Color7Property = RegisterBandColor(nameof(Color7), 6);
  /// <summary>Identifies the <see cref="Color8"/> dependency property.</summary>
  public static readonly DependencyProperty Color8Property = RegisterBandColor(nameof(Color8), 7);
  /// <summary>Identifies the <see cref="Color9"/> dependency property.</summary>
  public static readonly DependencyProperty Color9Property = RegisterBandColor(nameof(Color9), 8);

  // One Register call shared by all 9 color DPs - band is captured by the PropertyChangedCallback
  // closure, so each property still resolves independently into its own _resolvedColors slot the
  // moment it changes, not per frame.
  private static DependencyProperty RegisterBandColor(string name, int band) =>
      DependencyProperty.Register(name, typeof(Brush), typeof(PerformanceGraphLite),
          new FrameworkPropertyMetadata(DefaultBandColors[band], FrameworkPropertyMetadataOptions.AffectsRender,
              (d, e) => ((PerformanceGraphLite)d)._resolvedColors[band] = ResolveSolidBrush((Brush)e.NewValue)));

  // One StreamGeometry per color band, each reused every frame by re-opening it - a frame draws
  // at most BandCount DrawGeometry calls total (one per band that has at least one dot this
  // frame), never one per dot and never one per sample.
  private readonly StreamGeometry[] _bandGeometries = { new(), new(), new(), new(), new(), new(), new(), new(), new() };

  // The single reused StreamGeometry for SingleColor mode - lazily created on first render in that
  // mode, for the same reason _bandGeometries above is lazy, just in the other direction: a graph
  // that only ever renders Banded never allocates this one either.
  private StreamGeometry? _singleGeometry;

  private static StreamGeometry[] CreateBandGeometries() {
    var geometries = new StreamGeometry[BandCount];
    for (int i = 0; i < BandCount; i++) geometries[i] = new StreamGeometry();
    return geometries;
  }

  // Resolved once per color change, not per frame - mirrors the default ramp until a Color1..9
  // DP's changed callback above updates the matching slot. Clone so a control can freeze its own
  // (frozen) brush references without touching the shared defaults array.
  private readonly Brush[] _resolvedColors = (Brush[])DefaultBandColors.Clone();

  private readonly CircularBuffer<double> _values;

  /// <summary>Number of samples retained and plotted. Fixed at construction.</summary>
  public int Capacity { get; }

  /// <summary>Identifies the <see cref="Capacity"/> dependency property.</summary>
  public static readonly DependencyProperty CapacityProperty =
      DependencyProperty.Register(nameof(Capacity), typeof(int), typeof(PerformanceGraphLite),
          new FrameworkPropertyMetadata(DefaultCapacity, FrameworkPropertyMetadataOptions.AffectsRender, OnCapacityChanged),
          ValidateCapacity);

  private static bool ValidateCapacity(object value) => value is int capacity && capacity > 0;

  private static void OnCapacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraphLite)d;
    int newCapacity = (int)e.NewValue;
    if (graph._values != null && newCapacity == graph._values.Capacity) return; // e.g. the constructor's own sync set

    // Unlike PerformanceGraph.HistoryLength, this does not carry samples over - see the class
    // remarks for why. This exists so Capacity is reachable from XAML/bindings/styles at all
    // (the (int capacity) constructor overload never was reachable from plain XAML), not to
    // support changing it live under a running feed.
    graph._values = new CircularBuffer<double>(newCapacity);
  }

  /// <summary>Number of samples retained and plotted. Settable, but see the class remarks - this
  /// does not preserve existing samples across a change the way <see cref="PerformanceGraph"/>'s
  /// <c>HistoryLength</c> does.</summary>
  public int Capacity {
    get => (int)GetValue(CapacityProperty);
    set => SetValue(CapacityProperty, value);
  }

  // Off-screen render suspension — identical rationale and mechanics to PerformanceGraph's: a
  // collapsed tile, minimized window, or closed detail window flips IsVisible to false. Samples
  // still land in the buffer so no data gap forms, but queuing a render pass every poll for a
  // tile nobody can see is pure waste; one deferred invalidation is flushed the moment the
  // control becomes visible again.
  private bool _renderSuspended;
  private bool _pendingRender;

  /// <summary>Creates a graph with the default 60-sample history.</summary>
  public PerformanceGraphLite() : this(DefaultCapacity) { }

  /// <summary>Creates a graph whose sample history is fixed at <paramref name="capacity"/> for
  /// the control's lifetime.</summary>
  public PerformanceGraphLite(int capacity) {
    if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

    Capacity = capacity;
    _values = new CircularBuffer<double>(capacity);

    // Keep the Capacity DP in step with the constructor argument (the DP defaults to
    // DefaultCapacity, so a non-default programmatic size would otherwise disagree with it).
    // SetCurrentValue leaves a later Style/Binding/XAML attribute free to override;
    // OnCapacityChanged no-ops here because _values is already this size.
    SetCurrentValue(CapacityProperty, capacity);

    SnapsToDevicePixels = true;
    UseLayoutRounding = true;

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
    if (visible && _pendingRender) {
      _pendingRender = false;
      InvalidateVisual();
    }
  }

  // Queue a repaint, unless the control is off-screen — then just remember one is owed so it can
  // be flushed on the next IsVisible transition.
  private void RequestRender() {
    if (_renderSuspended) {
      _pendingRender = true;
      return;
    }
    InvalidateVisual();
  }

  /// <summary>
  /// Binds the graph's data to an <see cref="ObservableCollection{T}"/> of <see cref="double"/>
  /// instead of driving it imperatively via <see cref="AddValue"/> from code-behind - e.g.
  /// <c>ValuesSource="{Binding UtilizationSamples}"</c> in XAML. Setting this property (assignment
  /// or binding alike) clears the buffer and seeds it with the collection's current contents, then
  /// every subsequent <see cref="INotifyCollectionChanged.CollectionChanged"/> notification that
  /// carries new items appends them through the same <see cref="AddValue"/> path used by the
  /// code-behind API - same O(1) ring-buffer append, same off-screen render-suspension behavior.
  /// A <see cref="NotifyCollectionChangedAction.Reset"/> (e.g. <c>Collection.Clear()</c>) clears
  /// the buffer and re-seeds it from the collection's post-reset contents; Remove/Replace/Move
  /// aren't translated into buffer edits beyond appending any NewItems they carry - there's no
  /// buffer operation that corresponds to "un-plot a sample already drawn," so aging out old
  /// samples is left entirely to the ring buffer's own capacity-driven eviction, not to source
  /// removals.
  /// <para>
  /// <b>Threading:</b> unlike <see cref="AddValue"/>, which accepts calls from a background thread
  /// and hops onto the UI thread itself, <see cref="ObservableCollection{T}"/> is not safe to
  /// mutate from a background thread - only the thread that owns the collection may call
  /// Add/Clear on it. Keep using <see cref="AddValue"/> directly for a sensor-polling thread; use
  /// <see cref="ValuesSource"/> when the data both originates on and is mutated from the UI thread
  /// (e.g. a view-model collection updated from a DispatcherTimer), or when the feed already
  /// marshals its own collection edits onto it.
  /// </para>
  /// </summary>
  public ObservableCollection<double>? ValuesSource {
    get => (ObservableCollection<double>?)GetValue(ValuesSourceProperty);
    set => SetValue(ValuesSourceProperty, value);
  }

  private static void OnValuesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraphLite)d;

    if (e.OldValue is ObservableCollection<double> oldSource)
      oldSource.CollectionChanged -= graph.OnValuesSourceCollectionChanged;

    graph.ClearValues();

    if (e.NewValue is ObservableCollection<double> newSource) {
      foreach (double value in newSource) graph.AddValue(value);
      newSource.CollectionChanged += graph.OnValuesSourceCollectionChanged;
    }
  }

  private void OnValuesSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
    if (e.Action == NotifyCollectionChangedAction.Reset) {
      ClearValues();
      if (sender is ObservableCollection<double> source)
        foreach (double value in source) AddValue(value);
      return;
    }

    // Add, Replace, and Move all surface their new elements via NewItems - appending them covers
    // the live-append scenario this property exists for. There's no corresponding "remove the
    // matching sample" for a plain Remove, so a bare Remove is a no-op here by design.
    if (e.NewItems == null) return;
    foreach (double value in e.NewItems) AddValue(value);
  }

  /// <summary>Solid backdrop painted behind the dots. Null (the default) paints nothing at all,
  /// so a Lite graph placed over an already-themed backdrop (e.g. inside a themed Border) costs
  /// one fewer <c>DrawRectangle</c> call per frame than opting in would.</summary>
  public Brush? GraphBackground {
    get => (Brush?)GetValue(GraphBackgroundProperty);
    set => SetValue(GraphBackgroundProperty, value);
  }

  /// <summary>Values at or below this map to the bottom edge of the plot area (top edge if <see cref="Flip"/> is true).</summary>
  public double MinValue {
    get => (double)GetValue(MinValueProperty);
    set => SetValue(MinValueProperty, value);
  }

  /// <summary>Values at or above this map to the top edge of the plot area (bottom edge if <see cref="Flip"/> is true).</summary>
  public double MaxValue {
    get => (double)GetValue(MaxValueProperty);
    set => SetValue(MaxValueProperty, value);
  }

  /// <summary>Vertical dot resolution — how many rows of dots a fully-lit column draws. Also the
  /// resolution of the 5-band coloring (see remarks); a multiple of 5 bands most cleanly, but any
  /// positive value works.</summary>
  public int Rows {
    get => (int)GetValue(RowsProperty);
    set => SetValue(RowsProperty, value);
  }

  /// <summary>When true, dots stack from the top row downward instead of the bottom row upward -
  /// mirrors <see cref="PerformanceGraph.Flip"/>'s effect on its SegmentedBar kind, the closest
  /// analog to this control's own dot-matrix rendering. Row-to-band color mapping is unaffected -
  /// only where each row is drawn on screen changes, not which value or color it represents.</summary>
  public bool Flip {
    get => (bool)GetValue(FlipProperty);
    set => SetValue(FlipProperty, value);
  }

  /// <summary>Selects whether dots are colored by the <see cref="Color1"/>..<see cref="Color9"/>
  /// value bands (<see cref="DotColorMode.Banded"/>, the default) or by a single flat
  /// <see cref="DotColor"/> (<see cref="DotColorMode.SingleColor"/>). Switching to
  /// <see cref="DotColorMode.SingleColor"/> also switches the render path to exactly one draw call
  /// per frame with no band lookup - see the class remarks.</summary>
  public DotColorMode ColorMode {
    get => (DotColorMode)GetValue(ColorModeProperty);
    set => SetValue(ColorModeProperty, value);
  }

  /// <summary>Uniform corner radius, in pixels, applied to every dot. Not the four-value
  /// <see cref="System.Windows.CornerRadius"/> struct - a plain <see cref="double"/>, since a
  /// small square dot has no use for four independent corners. Defaults to 0 (sharp corners).
  /// <see cref="AddDotFigure"/> clamps this per dot to at most half of that dot's own smaller
  /// dimension, so a value larger than the dot itself can't self-intersect the geometry - set it
  /// to half the dot size (see <see cref="SquareDotAspectRatio"/> for computing a size where width
  /// and height match) for a fully round dot.</summary>
  public double CornerRadius {
    get => (double)GetValue(CornerRadiusProperty);
    set => SetValue(CornerRadiusProperty, value);
  }

  /// <summary>The single color used for every dot when <see cref="ColorMode"/> is
  /// <see cref="DotColorMode.SingleColor"/>. Ignored (and never resolved into the render path) in
  /// <see cref="DotColorMode.Banded"/> mode.</summary>
  public Brush DotColor {
    get => (Brush)GetValue(DotColorProperty);
    set => SetValue(DotColorProperty, value);
  }

  /// <summary>Color for the lowest (1st) of the 9 value bands. Defaults to green.</summary>
  public Brush Color1 { get => (Brush)GetValue(Color1Property); set => SetValue(Color1Property, value); }
  /// <summary>Color for the 2nd-lowest of the 9 value bands.</summary>
  public Brush Color2 { get => (Brush)GetValue(Color2Property); set => SetValue(Color2Property, value); }
  /// <summary>Color for the 3rd of the 9 value bands.</summary>
  public Brush Color3 { get => (Brush)GetValue(Color3Property); set => SetValue(Color3Property, value); }
  /// <summary>Color for the 4th of the 9 value bands.</summary>
  public Brush Color4 { get => (Brush)GetValue(Color4Property); set => SetValue(Color4Property, value); }
  /// <summary>Color for the middle (5th) of the 9 value bands. Defaults to yellow.</summary>
  public Brush Color5 { get => (Brush)GetValue(Color5Property); set => SetValue(Color5Property, value); }
  /// <summary>Color for the 6th of the 9 value bands.</summary>
  public Brush Color6 { get => (Brush)GetValue(Color6Property); set => SetValue(Color6Property, value); }
  /// <summary>Color for the 7th of the 9 value bands.</summary>
  public Brush Color7 { get => (Brush)GetValue(Color7Property); set => SetValue(Color7Property, value); }
  /// <summary>Color for the 8th of the 9 value bands.</summary>
  public Brush Color8 { get => (Brush)GetValue(Color8Property); set => SetValue(Color8Property, value); }
  /// <summary>Color for the highest (9th) of the 9 value bands. Defaults to red.</summary>
  public Brush Color9 { get => (Brush)GetValue(Color9Property); set => SetValue(Color9Property, value); }

  /// <summary>Appends a new sample, dropping the oldest once <see cref="Capacity"/> is exceeded. O(1).</summary>
  public void AddValue(double value) {
    // Sensor streams push from a background thread; InvalidateVisual (and the buffer) require
    // this element's dispatcher, so hop onto it if we're not already there.
    if (!CheckAccess()) {
      Dispatcher.BeginInvoke(() => AddValue(value));
      return;
    }
    _values.Add(value);
    RequestRender();
  }

  /// <summary>Discards all buffered samples.</summary>
  public void ClearValues() {
    if (!CheckAccess()) {
      Dispatcher.BeginInvoke(ClearValues);
      return;
    }
    _values.Clear();
    RequestRender();
  }

  // Same flatten-to-most-opaque-stop-and-freeze approach as PerformanceGraph's internal
  // SolidFillCache, minus the matching Pen it also builds — Lite dots are fill-only, so there is
  // no pen to cache alongside the brush.
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

  // Which of the 5 color bands a given row belongs to, by the row's position in the value scale
  // (row 0 = lowest values) - independent of Flip, which only changes where that row is drawn.
  private static int BandForRow(int row, int rows) {
    double rowFraction = (row + 0.5) / rows;
    int band = (int)(rowFraction * BandCount);
    return band < 0 ? 0 : (band >= BandCount ? BandCount - 1 : band);
  }

  protected override Size MeasureOverride(Size availableSize) {
    double width = double.IsInfinity(availableSize.Width) ? 200 : availableSize.Width;
    double height = double.IsInfinity(availableSize.Height) ? 100 : availableSize.Height;
    return new Size(width, height);
  }

  protected override Size ArrangeOverride(Size finalSize) => finalSize;

  // Bundles the per-frame layout math shared by both render paths into one value passed by `in`
  // (a struct, so this costs nothing beyond a handful of doubles on the stack - no heap allocation
  // either mode's caller wouldn't already have paid for as local variables).
  private readonly struct DotLayout {
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

  protected override void OnRender(DrawingContext dc) {
    base.OnRender(dc);

    Rect bounds = new(RenderSize);
    if (bounds.Width <= 0 || bounds.Height <= 0) return;

    // No grid, no border: this is the entire render pass — an optional flat backdrop, then dots.
    if (GraphBackground != null) dc.DrawRectangle(GraphBackground, null, bounds);

    int count = _values.Count;
    int rows = Rows;
    double range = MaxValue - MinValue;
    if (count == 0 || rows <= 0 || range <= 0) return;

    int effectiveCapacity = Capacity > count ? Capacity : count;
    double slotWidth = bounds.Width / effectiveCapacity;
    double dotColumnWidth = slotWidth * ColumnWidthRatio;
    double columnInset = (slotWidth - dotColumnWidth) / 2;

    double rowHeight = bounds.Height / rows;
    // One dot per column, centred in its slot. Sizing off the row pitch keeps dots the same
    // size regardless of tile width; a column is never split into 2+ dots across.
    double dotSize = rowHeight * DotSizeRatio;
    if (dotSize > dotColumnWidth) dotSize = dotColumnWidth;
    double rowPadding = (rowHeight - dotSize) / 2;

    var layout = new DotLayout(slotWidth, dotColumnWidth, columnInset, rowHeight, dotSize, rowPadding);
    bool flip = Flip;
    double cornerRadius = CornerRadius;

    // The two modes are genuinely different render paths, not a shared loop with a color lookup
    // swapped out - SingleColor never touches BandForRow, never allocates the nine band
    // geometries, and never opens more than one StreamGeometryContext for the whole frame.
    if (ColorMode == DotColorMode.SingleColor)
      RenderSingleColor(dc, bounds, count, rows, minValue, range, in layout, flip, cornerRadius);
    else
      RenderBanded(dc, bounds, count, rows, minValue, range, in layout, flip, cornerRadius);
  }

  // SingleColor path: one geometry, one Open()/Close() pair, one DrawGeometry call - regardless of
  // sample count or Rows. No BandForRow call anywhere in this method.
  private void RenderSingleColor(DrawingContext dc, Rect bounds, int count, int rows, double minValue,
      double range, in DotLayout layout, bool flip, double cornerRadius) {
    StreamGeometry geometry = _singleGeometry ??= new StreamGeometry();

    using (StreamGeometryContext ctx = geometry.Open()) {
      for (int i = 0; i < count; i++) {
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

  // Banded path: unchanged algorithm from before ColorMode existed, just gated behind it and
  // reading layout fields instead of locals. Up to nine geometries, opened/closed once per frame,
  // one DrawGeometry call per band that actually has a dot in it this frame.
  private void RenderBanded(DrawingContext dc, Rect bounds, int count, int rows, double minValue,
      double range, in DotLayout layout, bool flip, double cornerRadius) {
    StreamGeometry[] geometries = _bandGeometries ??= CreateBandGeometries();

    var contexts = new StreamGeometryContext[BandCount];
    try {
      for (int b = 0; b < BandCount; b++) contexts[b] = _bandGeometries[b].Open();

      for (int i = 0; i < count; i++) {
        // Newest sample (last, index count-1) pinned to the right edge; each older sample steps
        // one slot to the left — the same right-aligned layout PerformanceGraph's renderers use,
        // so a Lite graph's columns land where a full graph's would at the same capacity.
        double slotRight = bounds.Right - (count - 1 - i) * slotWidth;
        double left = slotRight - slotWidth + columnInset;
        double cx = left + dotColumnWidth / 2;

        double t = (_values[i] - MinValue) / range;
        t = t < 0 ? 0 : (t > 1 ? 1 : t);

        // Rows fully reached by this value, plus how far (as a 0-1 fraction of one row) it gets
        // into the next one — mirrors GraphKind.SegmentedBar's own fractional-fill math exactly.
        double fillHeight = t * bounds.Height;
        int fullRows = (int)(fillHeight / rowHeight);
        if (fullRows > rows) fullRows = rows;

        double partialFraction = 0;
        if (fullRows < rows) {
          partialFraction = (fillHeight - fullRows * rowHeight) / rowHeight;
          partialFraction = partialFraction < 0 ? 0 : (partialFraction > 1 ? 1 : partialFraction);
        }

        for (int r = 0; r < fullRows; r++) {
          double top = flip
              ? bounds.Top + r * rowHeight + rowPadding
              : bounds.Bottom - (r + 1) * rowHeight + rowPadding;
          AddDotFigure(contexts[BandForRow(r, rows)], cx - dotSize / 2, top, dotSize, dotSize);
        }

        // The one partially-lit row just past the fully-lit ones, if any: full width, height
        // shrunk to the fractional remainder, anchored at the same edge a full dot in that row
        // would share (bottom edge in the normal orientation, top edge when flipped) so it reads
        // as "this row is X% lit" rather than a dot that's merely positioned differently.
        if (partialFraction > 0 && fullRows < rows) {
          double partialHeight = dotSize * partialFraction;
          double top = flip
              ? bounds.Top + fullRows * rowHeight + rowPadding
              : bounds.Bottom - (fullRows + 1) * rowHeight + rowPadding + (dotSize - partialHeight);
          AddDotFigure(contexts[BandForRow(fullRows, rows)], cx - dotSize / 2, top, dotSize, partialHeight);
        }
      }
    } finally {
      for (int b = 0; b < BandCount; b++) contexts[b]?.Close();
    }

    for (int b = 0; b < BandCount; b++) dc.DrawGeometry(_resolvedColors[b], null, _bandGeometries[b]);
  }

  // Traces one closed, filled square (or, for a fractional dot, rectangle) figure directly into
  // an already-open context — no Rect/Point[] intermediate, no separate geometry per dot, no
  // stroke (fill-only: no Pen is ever passed to DrawGeometry for this control, so isStroked here
  // is moot but kept false for clarity). Every dot this control ever draws, full or fractional,
  // goes through this one method — there is no other shape, no clip-based partial fill, and no
  // opacity trick anywhere in this class.
  private static void AddDotFigure(StreamGeometryContext ctx, double left, double top, double width, double height) {
    double right = left + width;
    double bottom = top + height;

    // Clamped per dot, not once per frame: a fractional (partially-lit) row's shortened height
    // must not let the radius exceed half of whichever dimension is smaller here specifically, or
    // the corner arcs below would overlap/self-intersect for that one shortened dot even though
    // full-height dots in the same frame are fine at the same CornerRadius value.
    double radius = cornerRadius;
    double maxRadius = Math.Min(width, height) / 2;
    if (radius > maxRadius) radius = maxRadius;

    if (radius <= 0) {
      // Plain rectangle - unchanged from before CornerRadius existed, and the common case (the
      // property defaults to 0), so it stays the cheapest path rather than routing everything
      // through the arc-based corners below just to draw them at a zero radius.
      ctx.BeginFigure(new Point(left, top), isFilled: true, isClosed: true);
      ctx.LineTo(new Point(right, top), isStroked: false, isSmoothJoin: false);
      ctx.LineTo(new Point(right, bottom), isStroked: false, isSmoothJoin: false);
      ctx.LineTo(new Point(left, bottom), isStroked: false, isSmoothJoin: false);
      return;
    }

    // Rounded rectangle: each corner replaced by a 90-degree ArcTo (always the "small" arc, since
    // 90 degrees is under the 180-degree large/small boundary), traced clockwise around the
    // perimeter starting just right of the top-left corner. The final ArcTo's endpoint is exactly
    // the BeginFigure start point, so the figure is already closed by the segments themselves -
    // isClosed: true here governs the corner join, not an extra implicit closing line.
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
}
