using Crystal.Plot2D.Charts;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.DataSources.OneDimensional;
using Crystal.Plot2D.Transforms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Graphs;

/// <summary>
/// Class represents a series of points connected by one polyline.
/// </summary>
public sealed class LineGraph : PointsGraphBase {
  static LineGraph() {
    var thisType = typeof(LineGraph);
  }

  /// <summary>Provides access to filters collection</summary>
  public FilterCollection Filters { get; } = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="LineGraph"/> class.
  /// </summary>
  public LineGraph() {
    ManualTranslate = true;
    Filters.CollectionChanged += filters_CollectionChanged;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="LineGraph"/> class.
  /// </summary>
  /// <param name="pointSource">The point source.</param>
  public LineGraph(IPointDataSource pointSource) : this() {
    DataSource = pointSource;
  }

  private void filters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    FilteredPoints = null;
    Update();
  }
    
  #region OutlinePen

  /*
  /// <summary>
  ///   Gets or sets the brush, using which polyline is plotted.
  /// </summary>
  /// <value>
  ///   The line brush.
  /// </value>
  public Brush Stroke
  {
    get => LinePen.Brush;
    set
    {
      if (LinePen.Brush != value)
      {
        if (!LinePen.IsSealed)
        {
          LinePen.Brush = value;
          InvalidateVisual();
        }
        else
        {
          OutlinePen pen = LinePen.Clone();
          pen.Brush = value;
          LinePen = pen;
        }
        RaisePropertyChanged();
      }
    }
  }
  */

  /*
  /// <summary>
  ///   Gets or sets the line thickness.
  /// </summary>
  /// <value>
  ///   The line thickness.
  /// </value>
  public double StrokeThickness
  {
    get => LinePen.Thickness;
    set
    {
      if (LinePen.Thickness != value)
      {
        if (!LinePen.IsSealed)
        {
          LinePen.Thickness = value; InvalidateVisual();
        }
        else
        {
          OutlinePen pen = LinePen.Clone();
          pen.Thickness = value;
          LinePen = pen;
        }

        RaisePropertyChanged(propertyName: nameof(StrokeThickness));
      }
    }
  }
  */

  /// <summary>
  ///   Gets or sets the line pen.
  /// </summary>
  /// <value>
  ///   The line pen.
  /// </value>
  [NotNull]
  public Pen LinePen {
    get => (Pen)GetValue(dp: LinePenProperty);
    set => SetValue(dp: LinePenProperty, value: value);
  }

  public static readonly DependencyProperty LinePenProperty = DependencyProperty.Register(
    name: nameof(LinePen),
    propertyType: typeof(Pen),
    ownerType: typeof(LineGraph),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Pen(brush: Brushes.Blue, thickness: 1), flags: FrameworkPropertyMetadataOptions.AffectsRender), validateValueCallback: OnValidatePen);

  private static bool OnValidatePen(object value) => value != null;

  #endregion

  protected override void OnOutputChanged(Rect newRect, Rect oldRect) {
    FilteredPoints = null;
    base.OnOutputChanged(newRect: newRect, oldRect: oldRect);
  }

  protected override void OnDataChanged() {
    FilteredPoints = null;
    base.OnDataChanged();
  }

  protected override void OnDataSourceChanged(DependencyPropertyChangedEventArgs args) {
    FilteredPoints = null;
    base.OnDataSourceChanged(args: args);
  }

  protected override void OnVisibleChanged(DataRect newRect, DataRect oldRect) {
    if(newRect.Size != oldRect.Size) {
      FilteredPoints = null;
    }
    base.OnVisibleChanged(newRect: newRect, oldRect: oldRect);
  }

  private FakePointList FilteredPoints { get; set; }

  protected override void UpdateCore() {
    if(Plotter == null || DataSource == null) {
      return;
    }
    var output = Viewport.Output;
    var transform = GetTransform();

    if(FilteredPoints == null || !(transform.DataTransform is IdentityTransform)) {
      var points = GetPoints();

      var bounds = BoundsHelper.GetViewportBounds(dataPoints: points, transform: transform.DataTransform);
      Viewport2D.SetContentBounds(obj: this, value: bounds);

      // getting new value of transform as it could change after calculating and setting content bounds.
      transform = GetTransform();
      var transformedPoints = transform.DataToScreenAsList(dataPoints: points);

      // Analysis and filtering of unnecessary points
      FilteredPoints = new FakePointList(points: FilterPoints(points: transformedPoints), left: output.Left, right: output.Right);

      if(ProvideVisiblePoints) {
        List<Point> viewportPointsList = new(capacity: transformedPoints.Count);
        if(transform.DataTransform is IdentityTransform) {
          viewportPointsList.AddRange(collection: points);
        }
        else {
          var viewportPoints = points.DataToViewport(transform: transform.DataTransform);
          viewportPointsList.AddRange(collection: viewportPoints);
        }

        SetVisiblePoints(obj: this, value: new ReadOnlyCollection<Point>(list: viewportPointsList));
      }

      Offset = new Vector();
    }
    else {
      var left = output.Left;
      var right = output.Right;
      var shift = Offset.X;
      left -= shift;
      right -= shift;
      FilteredPoints.SetXBorders(left: left, right: right);
    }
  }

  private readonly StreamGeometry _streamGeometry = new();
  protected override void OnRenderCore(DrawingContext dc, RenderState state) {
    if(DataSource == null) {
      return;
    }
    if(FilteredPoints.HasPoints) {
      using(var context = _streamGeometry.Open()) {
        context.BeginFigure(startPoint: FilteredPoints.StartPoint, isFilled: false, isClosed: false);
        context.PolyLineTo(points: FilteredPoints, isStroked: true, isSmoothJoin: _smoothLinesJoin);
      }

      Brush brush = null;
      var pen = LinePen;

      var isTranslated = IsTranslated;
      if(isTranslated) {
        dc.PushTransform(transform: new TranslateTransform(offsetX: Offset.X, offsetY: Offset.Y));
      }
      dc.DrawGeometry(brush: brush, pen: pen, geometry: _streamGeometry);
      if(isTranslated) {
        dc.Pop();
      }
    }
  }

  private bool _filteringEnabled = true;
  public bool FilteringEnabled {
    get => _filteringEnabled;
    set {
      if(_filteringEnabled != value) {
        _filteringEnabled = value;
        FilteredPoints = null;
        Update();
      }
    }
  }

  private bool _smoothLinesJoin = true;
  public bool SmoothLinesJoin {
    get => _smoothLinesJoin;
    set {
      _smoothLinesJoin = value;
      Update();
    }
  }

  private List<Point> FilterPoints(List<Point> points) {
    if(!_filteringEnabled) {
      return points;
    }
    var filteredPoints = Filters.Filter(points: points, screenRect: Viewport.Output);
    return filteredPoints;
  }
}