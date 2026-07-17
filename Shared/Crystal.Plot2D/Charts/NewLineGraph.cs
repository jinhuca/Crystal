using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.DataSources.OneDimensional;
using Crystal.Plot2D.Graphs;
using Crystal.Plot2D.Transforms;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Crystal.Plot2D.Charts;

/// <summary>
///   Represents a series of points connected by one polyline.
/// </summary>
public class NewLineGraph : Canvas, IPlotterElement {
  static NewLineGraph() {
    var thisType_ = typeof(NewLineGraph);
  }


  private readonly FilterCollection filters = new();

  /// <summary>
  ///   Initializes a new instance of the <see cref="LineGraph"/> class.
  /// </summary>
  public NewLineGraph() {
    filters.CollectionChanged += filters_CollectionChanged;
    RenderTransform = layoutTransform;
    Background = Brushes.Green.MakeTransparent(opacity: 0.3);
  }

  private void filters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    // todo
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="LineGraph"/> class.
  /// </summary>
  /// <param name="pointSource">
  ///   The point source.
  /// </param>
  public NewLineGraph(IPointDataSource pointSource) : this() {
    DataSource = pointSource;
  }

  /// <summary>
  ///   Provides access to filters collection.
  /// </summary>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Content)]
  public FilterCollection Filters => filters;

  #region Properties

  #region DataSource property

  public IPointDataSource DataSource {
    get => (IPointDataSource)GetValue(dp: DataSourceProperty);
    set => SetValue(dp: DataSourceProperty, value: value);
  }

  public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
    name: nameof(DataSource),
    propertyType: typeof(IPointDataSource),
    ownerType: typeof(NewLineGraph),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnDataSourceReplaced));

  private static void OnDataSourceReplaced(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var owner_ = (NewLineGraph)d;
    owner_.OnDataSourceReplaced(prevDataSource: (IPointDataSource)e.OldValue, currDataSource: (IPointDataSource)e.NewValue);
  }

  private void OnDataSourceReplaced(IPointDataSource prevDataSource, IPointDataSource currDataSource) {
    if(prevDataSource != null) {
      prevDataSource.DataChanged -= OnDataChanged;
    }

    if(currDataSource != null) {
      currDataSource.DataChanged += OnDataChanged;
    }
    Update();
  }

  private void OnDataChanged(object sender, EventArgs e) {
    Update();
  }

  #endregion // end of DataSource property

  #region Stroke property

  public Brush Stroke {
    get => (Brush)GetValue(dp: StrokeProperty);
    set => SetValue(dp: StrokeProperty, value: value);
  }

  public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
    name: nameof(Stroke),
    propertyType: typeof(Brush),
    ownerType: typeof(NewLineGraph),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Blue));

  #endregion // end of Stroke property

  #region StrokeThickness property

  public double StrokeThickness {
    get => (double)GetValue(dp: StrokeThicknessProperty);
    set => SetValue(dp: StrokeThicknessProperty, value: value);
  }

  public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
    name: nameof(StrokeThickness),
    propertyType: typeof(double),
    ownerType: typeof(NewLineGraph),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: 1.0));

  #endregion // end of StrokeThickness property

  #endregion // end of Properties

  private bool smoothLinesJoin = true;
  public bool SmoothLinesJoin {
    get => smoothLinesJoin;
    set {
      smoothLinesJoin = value;
      Update();
    }
  }

  private List<Point> FilterPoints(List<Point> points) {
    var filteredPoints_ = filters.Filter(points: points, screenRect: plotter.Viewport.Output);

    return filteredPoints_;
  }

  private void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e) {
    if(e.PropertyName == "Visible") {
      var prevVisible_ = (DataRect)e.OldValue;
      var currVisible_ = (DataRect)e.NewValue;

      if(currVisible_.Size != prevVisible_.Size) {
        Update();
      }
      else {
        UpdateTransform();
      }
    }
    else {
      Update();
    }
  }

  private readonly TranslateTransform layoutTransform = new();
  private void UpdateTransform() {
    var currentTransform_ = plotter.Transform;

    var shift_ = transformWhenCreated.ViewportRect.Location.ViewportToScreen(transform: currentTransform_)
      - currentTransform_.ViewportRect.Location.ViewportToScreen(transform: currentTransform_);

    layoutTransform.X = shift_.X;
    layoutTransform.Y = shift_.Y;

    Debug.WriteLine(message: "X=" + shift_.X);
    Debug.WriteLine(message: "Y=" + shift_.Y);
  }

  private CoordinateTransform transformWhenCreated;
  private readonly ResourcePool<Polyline> polylinePool = new();
  private const int PointCount = 500;

  private void Update() {
    if(Plotter == null) {
      return;
    }

    if(DataSource == null) {
      return;
    }

    layoutTransform.X = 0;
    layoutTransform.Y = 0;

    var dataSource_ = DataSource;
    var dataPoints_ = dataSource_.GetPoints();

    transformWhenCreated = plotter.Transform;

    var contentBounds_ = dataPoints_.GetBounds();
    Viewport2D.SetContentBounds(obj: this, value: contentBounds_);

    foreach(Polyline polyline_ in Children) {
      polylinePool.Put(item: polyline_);
    }

    Children.Clear();

    PointCollection pointCollection_ = new();
    foreach(var screenPoint_ in dataPoints_.DataToScreen(transform: plotter.Transform)) {
      if(pointCollection_.Count < PointCount) {
        pointCollection_.Add(value: screenPoint_);
      }
      else {
        var polyline_ = polylinePool.GetOrCreate();
        polyline_.Points = pointCollection_;

        SetPolylineBindings(polyline: polyline_);

        Children.Add(element: polyline_);
        Dispatcher.Invoke(callback: () => { }, priority: DispatcherPriority.ApplicationIdle);
        pointCollection_ = new PointCollection();
      }
    }
  }

  private void SetPolylineBindings(Polyline polyline) {
    polyline.SetBinding(dp: Shape.StrokeProperty, binding: new Binding { Source = this, Path = new PropertyPath(path: "Stroke") });
    polyline.SetBinding(dp: Shape.StrokeThicknessProperty, binding: new Binding { Source = this, Path = new PropertyPath(path: "StrokeThickness") });
  }

  #region IPlotterElement Members

  private PlotterBase plotter;
  public void OnPlotterAttached(PlotterBase plotter) {
    this.plotter = (PlotterBase)plotter;
    this.plotter.Viewport.PropertyChanged += Viewport_PropertyChanged;

    plotter.CentralGrid.Children.Add(element: this);

    Update();
  }

  public void OnPlotterDetaching(PlotterBase plotter) {
    plotter.CentralGrid.Children.Remove(element: this);
    this.plotter.Viewport.PropertyChanged -= Viewport_PropertyChanged;
    this.plotter = null;
  }

  public PlotterBase Plotter => plotter;

  #endregion
}
