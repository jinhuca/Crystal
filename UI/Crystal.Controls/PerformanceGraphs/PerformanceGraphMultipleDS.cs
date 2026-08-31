using Crystal.Controls.PerformanceGraphs.Renders;
using Crystal.Controls.PerformanceGraphs.Styles;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// A Task-Manager-style performance graph that plots MULTIPLE independent lines
/// (<see cref="DataSeries"/> items) on one shared set of axes - each with its own color,
/// thickness, and optional fill, each fed independently (imperatively via
/// <see cref="DataSeries.AddValue"/>, or via <see cref="DataSeries.ValuesSource"/> - see that
/// property's own remarks for its XAML-binding caveat).
/// </summary>
/// <remarks>
/// Unlike <see cref="PerformanceGraph"/>'s own multi-series support (<c>AddSeries</c>/
/// <c>AddValue(int, double)</c> - imperative, code-behind-only, and index-addressed), series here
/// are first-class, declarative content:
/// <code>
/// &lt;graphs:PerformanceGraphMultipleDS MinValue="0" MaxValue="100" Capacity="60"&gt;
///   &lt;graphs:PerformanceGraphMultipleDS.Series&gt;
///     &lt;graphs:DataSeries x:Name="CpuSeries" Name="CPU" LineBrush="#E82A7A"/&gt;
///     &lt;graphs:DataSeries x:Name="GpuSeries" Name="GPU" LineBrush="#3E9BE8"/&gt;
///   &lt;/graphs:PerformanceGraphMultipleDS.Series&gt;
/// &lt;/graphs:PerformanceGraphMultipleDS&gt;
/// </code>
/// <see cref="Series"/> defaults to a live, empty <see cref="ObservableCollection{T}"/> (set in the
/// constructor) so plain XAML population, as above, works with no <c>{Binding}</c> needed - the
/// same idiom <see cref="System.Windows.Controls.ItemsControl.Items"/> uses. Assigning the WHOLE
/// property to a different collection (from code-behind, or <c>{Binding}</c> on the graph itself,
/// which - unlike binding <see cref="DataSeries.ValuesSource"/> - works normally since the graph
/// is a real <see cref="FrameworkElement"/>) also works, for building the series list dynamically.
/// <para>
/// <b>Capacity</b> is shared by every series and is live-resizable - like <see cref="PerformanceGraph"/>'s
/// own <c>HistoryLength</c> (and unlike <see cref="PerformanceGraphLite"/>'s simpler,
/// non-preserving version), changing it rebuilds every series' buffer while carrying over each
/// one's most recent samples.
/// </para>
/// <para>
/// <b>What's deliberately not here (yet).</b> No <see cref="Kinds.GraphKind"/> switch - every
/// series always draws as a line, since "multiple lines" is this control's entire reason to exist.
/// No session-extreme markers (<see cref="PerformanceGraph.LowMarker"/>/<c>HighMarker</c>'s
/// equivalent) and no built-in legend - <see cref="DataSeries.Name"/> exists for a
/// consumer-built legend to read, not one this control draws itself.
/// </para>
/// </remarks>
public sealed class PerformanceGraphMultipleDS : FrameworkElement, IPerformanceGraph {
  private const int DefaultCapacity = 60;
  private const int DefaultGridColumns = 60;
  private const int DefaultGridRows = 4;

  private readonly BackgroundRenderer _backgroundRender = new();
  private readonly BorderRenderer _borderRender = new();
  private readonly GraphStyle _graphStyle = new();
  private GridRenderer _gridRender = new(DefaultGridRows, DefaultGridColumns);

  // Kept in sync with CapacityProperty - mirrors PerformanceGraph's own _historyLength field
  // exactly, including the SetCurrentValue-in-constructor sync pattern below.
  private int _capacity = DefaultCapacity;

  public PerformanceGraphMultipleDS() {
    SnapsToDevicePixels = true;
    UseLayoutRounding = true;

    // Suspend rendering whenever the control leaves the screen - identical mechanism to
    // PerformanceGraph/PerformanceGraphLite; see ApplyVisibility's own comment below.
    IsVisibleChanged += (_, e) => ApplyVisibility((bool)e.NewValue);
    ApplyVisibility(IsVisible);

    // A live, empty collection by default so plain XAML population works without {Binding} -
    // SetCurrentValue leaves a later Style/Binding/XAML attribute free to override it with a
    // different collection entirely.
    SetCurrentValue(SeriesProperty, new ObservableCollection<DataSeries>());
  }

  private bool _renderSuspended;
  private bool _pendingRender;

  /// <summary>True while rendering is suspended because the control is off-screen.</summary>
  internal bool RenderSuspended => _renderSuspended;

  /// <summary>True when a sample arrived while suspended, so a repaint is owed on the next show.</summary>
  internal bool HasPendingRender => _pendingRender;

  // Core of the visibility gate, split out from the IsVisibleChanged hook so it can be driven
  // deterministically in tests - identical to PerformanceGraph's own copy of this mechanism.
  internal void ApplyVisibility(bool visible) {
    _renderSuspended = !visible;
    if (visible && _pendingRender) {
      _pendingRender = false;
      InvalidateVisual();
    }
  }

  // Queue a repaint, unless the control is off-screen - then just remember one is owed. Called by
  // this class's own property-changed callbacks AND by DataSeries (internal, so a series can
  // request a repaint from its own AddValue/property changes without this graph having to poll
  // every series every frame to notice a change happened).
  internal void RequestRender() {
    if (_renderSuspended) {
      _pendingRender = true;
      return;
    }
    InvalidateVisual();
  }

  /// <summary>Identifies the <see cref="Series"/> dependency property.</summary>
  public static readonly DependencyProperty SeriesProperty =
      DependencyProperty.Register(nameof(Series), typeof(ObservableCollection<DataSeries>), typeof(PerformanceGraphMultipleDS),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnSeriesChanged));

  /// <summary>The lines this graph plots. See the class remarks for XAML/binding usage.</summary>
  public ObservableCollection<DataSeries> Series {
    get => (ObservableCollection<DataSeries>)GetValue(SeriesProperty);
    set => SetValue(SeriesProperty, value);
  }

  private static void OnSeriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraphMultipleDS)d;

    if (e.OldValue is ObservableCollection<DataSeries> oldSeries) {
      oldSeries.CollectionChanged -= graph.OnSeriesCollectionChanged;
      foreach (var s in oldSeries) s.Detach();
    }

    if (e.NewValue is ObservableCollection<DataSeries> newSeries) {
      foreach (var s in newSeries) s.Attach(graph, graph._capacity);
      newSeries.CollectionChanged += graph.OnSeriesCollectionChanged;
    }

    graph.RequestRender();
  }

  private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
    if (e.OldItems != null)
      foreach (DataSeries s in e.OldItems) s.Detach();

    if (e.NewItems != null)
      foreach (DataSeries s in e.NewItems) s.Attach(this, _capacity);

    // A bare Reset (e.g. Series.Clear()) carries no OldItems/NewItems - every series that was
    // attached already got Detach()'d individually as each was removed, so there's nothing this
    // branch needs to do beyond the repaint below.
    RequestRender();
  }

  /// <summary>Identifies the <see cref="MinValue"/> dependency property.</summary>
  public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(PerformanceGraphMultipleDS),
          new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Value mapped to the bottom edge, shared by every series on this graph.</summary>
  public double MinValue {
    get => (double)GetValue(MinValueProperty);
    set => SetValue(MinValueProperty, value);
  }

  /// <summary>Identifies the <see cref="MaxValue"/> dependency property.</summary>
  public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(PerformanceGraphMultipleDS),
          new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Value mapped to the top edge, shared by every series on this graph.</summary>
  public double MaxValue {
    get => (double)GetValue(MaxValueProperty);
    set => SetValue(MaxValueProperty, value);
  }

  /// <summary>Identifies the <see cref="Capacity"/> dependency property.</summary>
  public static readonly DependencyProperty CapacityProperty =
      DependencyProperty.Register(nameof(Capacity), typeof(int), typeof(PerformanceGraphMultipleDS),
          new FrameworkPropertyMetadata(DefaultCapacity, FrameworkPropertyMetadataOptions.AffectsRender, OnCapacityChanged),
          ValidateCapacity);

  private static bool ValidateCapacity(object value) => value is int c && c > 0;

  /// <summary>Number of samples retained/plotted, shared by every series on this graph. Resizing
  /// preserves each series' most recent samples - see the class remarks.</summary>
  public int Capacity {
    get => (int)GetValue(CapacityProperty);
    set => SetValue(CapacityProperty, value);
  }

  private static void OnCapacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraphMultipleDS)d;
    int newCapacity = (int)e.NewValue;
    if (newCapacity == graph._capacity) return; // e.g. the constructor's own sync set

    graph._capacity = newCapacity;
    foreach (var s in graph.Series) s.Resize(newCapacity);
  }

  /// <summary>Identifies the <see cref="GridColumns"/> dependency property.</summary>
  public static readonly DependencyProperty GridColumnsProperty =
      DependencyProperty.Register(nameof(GridColumns), typeof(int), typeof(PerformanceGraphMultipleDS),
          new FrameworkPropertyMetadata(DefaultGridColumns, FrameworkPropertyMetadataOptions.AffectsRender, OnGridChanged),
          value => value is int c && c > 0);

  /// <summary>Number of vertical grid lines drawn - purely cosmetic, independent of <see cref="Capacity"/>
  /// (the same deliberate split <see cref="PerformanceGraph.GridColumns"/> already makes).</summary>
  public int GridColumns {
    get => (int)GetValue(GridColumnsProperty);
    set => SetValue(GridColumnsProperty, value);
  }

  /// <summary>Identifies the <see cref="GridRows"/> dependency property.</summary>
  public static readonly DependencyProperty GridRowsProperty =
      DependencyProperty.Register(nameof(GridRows), typeof(int), typeof(PerformanceGraphMultipleDS),
          new FrameworkPropertyMetadata(DefaultGridRows, FrameworkPropertyMetadataOptions.AffectsRender, OnGridChanged),
          value => value is int r && r > 0);

  /// <summary>Number of horizontal grid lines drawn - purely cosmetic.</summary>
  public int GridRows {
    get => (int)GetValue(GridRowsProperty);
    set => SetValue(GridRowsProperty, value);
  }

  private static void OnGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    // GridRenderer's row/column count is constructor-only, so a change to either DP just builds a
    // fresh instance rather than mutating one in place - cheap, since GridRenderer itself only
    // holds a lazily-rebuilt cached geometry, not per-frame state worth preserving across the swap.
    var graph = (PerformanceGraphMultipleDS)d;
    graph._gridRender = new GridRenderer(graph.GridRows, graph.GridColumns);
  }

  /// <summary>
  /// The Height/Width ratio that makes every grid cell come out a perfect square for a graph with
  /// the given <paramref name="gridRows"/>/<paramref name="gridColumns"/> - matching exactly what
  /// <see cref="GridRenderer"/> itself computes internally
  /// (<c>cellWidth = bounds.Width / columns</c>, <c>cellHeight = bounds.Height / rows</c>), so
  /// there's a single source of truth for this math rather than a XAML binding/converter
  /// duplicating or guessing it. Multiply this by an actual pixel width to get the exact height
  /// that squares every cell at that width, whatever the width turns out to be. Identical to
  /// <see cref="PerformanceGraph.SquareGridAspectRatio"/> - this graph draws the same grid.
  /// </summary>
  public static double SquareGridAspectRatio(int gridRows, int gridColumns) {
    if (gridRows <= 0) throw new ArgumentOutOfRangeException(nameof(gridRows), "Grid row count must be positive.");
    if (gridColumns <= 0) throw new ArgumentOutOfRangeException(nameof(gridColumns), "Grid column count must be positive.");
    return gridRows / (double)gridColumns;
  }

  /// <summary>Identifies the <see cref="GraphBackground"/> dependency property.</summary>
  public static readonly DependencyProperty GraphBackgroundProperty =
      DependencyProperty.Register(nameof(GraphBackground), typeof(Brush), typeof(PerformanceGraphMultipleDS),
          new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnGraphBackgroundChanged));

  public Brush GraphBackground {
    get => (Brush)GetValue(GraphBackgroundProperty);
    set => SetValue(GraphBackgroundProperty, value);
  }

  private static void OnGraphBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((PerformanceGraphMultipleDS)d)._graphStyle.BackgroundBrush = (Brush)e.NewValue;

  /// <summary>Identifies the <see cref="GridBrush"/> dependency property.</summary>
  public static readonly DependencyProperty GridBrushProperty =
      DependencyProperty.Register(nameof(GridBrush), typeof(Brush), typeof(PerformanceGraphMultipleDS),
          new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender, OnGridBrushChanged));

  public Brush GridBrush {
    get => (Brush)GetValue(GridBrushProperty);
    set => SetValue(GridBrushProperty, value);
  }

  private static void OnGridBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraphMultipleDS)d;
    graph._graphStyle.GridPen = Helpers.CreateFrozenPen((Brush)e.NewValue, graph._graphStyle.GridPen.Thickness);
  }

  /// <summary>Identifies the <see cref="GridThickness"/> dependency property.</summary>
  public static readonly DependencyProperty GridThicknessProperty =
      DependencyProperty.Register(nameof(GridThickness), typeof(double), typeof(PerformanceGraphMultipleDS),
          new FrameworkPropertyMetadata(0.6, FrameworkPropertyMetadataOptions.AffectsRender, OnGridThicknessChanged));

  public double GridThickness {
    get => (double)GetValue(GridThicknessProperty);
    set => SetValue(GridThicknessProperty, value);
  }

  private static void OnGridThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraphMultipleDS)d;
    graph._graphStyle.GridPen = Helpers.CreateFrozenPen(graph._graphStyle.GridPen.Brush, (double)e.NewValue);
  }

  /// <summary>Identifies the <see cref="BorderBrush"/> dependency property.</summary>
  public static readonly DependencyProperty BorderBrushProperty =
      DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(PerformanceGraphMultipleDS),
          new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnBorderBrushChanged));

  public Brush BorderBrush {
    get => (Brush)GetValue(BorderBrushProperty);
    set => SetValue(BorderBrushProperty, value);
  }

  private static void OnBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraphMultipleDS)d;
    graph._graphStyle.BorderPen = Helpers.CreateFrozenPen((Brush)e.NewValue, graph._graphStyle.BorderPen.Thickness);
  }

  /// <summary>Identifies the <see cref="BorderThickness"/> dependency property.</summary>
  public static readonly DependencyProperty BorderThicknessProperty =
      DependencyProperty.Register(nameof(BorderThickness), typeof(double), typeof(PerformanceGraphMultipleDS),
          new FrameworkPropertyMetadata(0.8, FrameworkPropertyMetadataOptions.AffectsRender, OnBorderThicknessChanged));

  public double BorderThickness {
    get => (double)GetValue(BorderThicknessProperty);
    set => SetValue(BorderThicknessProperty, value);
  }

  private static void OnBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var graph = (PerformanceGraphMultipleDS)d;
    graph._graphStyle.BorderPen = Helpers.CreateFrozenPen(graph._graphStyle.BorderPen.Brush, (double)e.NewValue);
  }

  protected override Size MeasureOverride(Size availableSize) {
    // Same reasoning as PerformanceGraph/PerformanceGraphLite's own MeasureOverride: an infinite
    // dimension falls back to a size derived from this instance's own configuration (Capacity
    // columns / GridRows rows) instead of a flat magic number oblivious to either. Same
    // PixelsPerUnit=12 pitch already established for both of those elsewhere.
    const double PixelsPerUnit = 12;
    double width = double.IsInfinity(availableSize.Width) ? Capacity * PixelsPerUnit : availableSize.Width;
    double height = double.IsInfinity(availableSize.Height) ? GridRows * PixelsPerUnit : availableSize.Height;
    return new Size(width, height);
  }

  protected override Size ArrangeOverride(Size finalSize) => finalSize;

  protected override void OnRender(DrawingContext dc) {
    base.OnRender(dc);

    Rect bounds = new(RenderSize);

    // Background fills the whole control first, behind everything else.
    _backgroundRender.Draw(dc, bounds, _graphStyle);

    // Grid on top of the background — GridColumns purely cosmetic, unrelated to Capacity.
    _gridRender.Draw(dc, bounds, _graphStyle);

    // Read once, not once per series below - neither changes mid-frame, and each
    // DependencyProperty read is a property-store lookup, not a free field access.
    double minValue = MinValue;
    double maxValue = MaxValue;

    // Each series draws through its own renderer/reused StreamGeometry, so one series' geometry
    // isn't re-Open()'d by another within this same pass (which would render both with the last
    // geometry written) - the same reasoning PerformanceGraph's own overlay series established.
    foreach (var series in Series) {
      if (series.Buffer == null) continue; // Not attached - shouldn't normally happen, defensive.
      series.Renderer.Draw(dc, bounds, series.Buffer, _capacity, minValue, maxValue,
          series.ResolvedLinePen, series.FillBrush);
    }

    // Border drawn last so its edge stays crisp over the fill/grid instead of being covered.
    _borderRender.Draw(dc, bounds, _graphStyle);
  }
}
