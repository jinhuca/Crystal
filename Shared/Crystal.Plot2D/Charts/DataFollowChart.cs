using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Common.Auxiliary.DataSearch;
using Crystal.Plot2D.Graphs;
using Crystal.Plot2D.Transforms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Charts;

/// <summary>
///   Represents a marker with position.X bound to mouse cursor's position and position.
///   Y is determined by interpolation of <see cref="MarkerPointsGraph"/>'s points.
/// </summary>
[ContentProperty(name: "MarkerTemplate")]
public class DataFollowChart : ViewportHostPanel, INotifyPropertyChanged {
  /// <summary>
  ///   Initializes a new instance of the <see cref="DataFollowChart"/> class.
  /// </summary>
  public DataFollowChart() {
    Marker = CreateDefaultMarker();
    SetX(obj: marker, value: 0);
    SetY(obj: marker, value: 0);
    throw new InvalidOperationException();
  }

  private static Ellipse CreateDefaultMarker() {
    return new Ellipse {
      Width = 10,
      Height = 10,
      Stroke = Brushes.Green,
      StrokeThickness = 1,
      Fill = Brushes.LightGreen,
      Visibility = Visibility.Hidden
    };
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="DataFollowChart"/> class, bound to specified <see cref="PointsGraphBase"/>.
  /// </summary>
  /// <param name="pointSource">
  ///   The point source.
  /// </param>
  public DataFollowChart(PointsGraphBase pointSource) : this() {
    PointSource = pointSource;
  }

  #region MarkerTemplate property

  /// <summary>
  ///   Gets or sets the template, used to create a marker. This is a dependency property.
  /// </summary>
  /// <value>
  ///   The marker template.
  /// </value>
  public DataTemplate MarkerTemplate {
    get => (DataTemplate)GetValue(dp: MarkerTemplateProperty);
    set => SetValue(dp: MarkerTemplateProperty, value: value);
  }

  /// <summary>
  /// Identifies the <see cref="MarkerTemplate"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty MarkerTemplateProperty = DependencyProperty.Register(
    name: nameof(MarkerTemplate),
    propertyType: typeof(DataTemplate),
    ownerType: typeof(DataFollowChart),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnMarkerTemplateChanged));

  private static void OnMarkerTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var chart_ = (DataFollowChart)d;
    var template_ = (DataTemplate)e.NewValue;

    FrameworkElement marker_;
    if(template_ != null) {
      marker_ = (FrameworkElement)template_.LoadContent();
    }
    else {
      marker_ = CreateDefaultMarker();
    }

    chart_.Children.Remove(element: chart_.marker);
    chart_.Marker = marker_;
    chart_.Children.Add(element: marker_);
  }

  #endregion

  #region Point sources

  /// <summary>
  ///   Gets or sets the source of points. Can be null.
  /// </summary>
  /// <value>
  ///   The point source.
  /// </value>
  public PointsGraphBase PointSource {
    get => (PointsGraphBase)GetValue(dp: PointSourceProperty);
    set => SetValue(dp: PointSourceProperty, value: value);
  }

  /// <summary>
  ///   Identifies the <see cref="PointSource"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty PointSourceProperty = DependencyProperty.Register(
    name: nameof(PointSource),
    propertyType: typeof(PointsGraphBase),
    ownerType: typeof(DataFollowChart),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnPointSourceChanged));

  private static void OnPointSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var chart_ = (DataFollowChart)d;
    if(e.OldValue is PointsGraphBase previous_) {
      previous_.VisiblePointsChanged -= chart_.Source_VisiblePointsChanged;
    }

    if(e.NewValue is PointsGraphBase current_) {
      current_.ProvideVisiblePoints = true;
      current_.VisiblePointsChanged += chart_.Source_VisiblePointsChanged;
      if(current_.VisiblePoints != null) {
        chart_.searcher = new SortedXSearcher1d(_collection: current_.VisiblePoints);
      }
    }

    chart_.UpdateUIRepresentation();
  }

  private SearchResult1d searchResult = SearchResult1d.Empty;
  private SortedXSearcher1d searcher;
  private FrameworkElement marker;

  [NotNull]
  public FrameworkElement Marker {
    get => marker;
    protected set {
      marker = value;
      marker.DataContext = followDataContext;
      PropertyChanged.Raise(sender: this, propertyName: "Marker");
    }
  }

  private readonly FollowDataContext followDataContext = new();
  public FollowDataContext FollowDataContext => followDataContext;

  private void UpdateUIRepresentation() {
    if(Plotter == null) {
      return;
    }

    var source_ = PointSource;
    if(source_ is null or { VisiblePoints: null }) {
      SetValue(key: markerPositionPropertyKey, value: new Point(x: double.NaN, y: double.NaN));
      marker.Visibility = Visibility.Hidden;
      return;
    }

    var mousePos_ = Mouse.GetPosition(relativeTo: Plotter.CentralGrid);
    var transform_ = Plotter.Transform;
    var viewportPos_ = mousePos_.ScreenToViewport(transform: transform_);
    var x_ = viewportPos_.X;
    searchResult = searcher.SearchXBetween(_x: x_, _result: searchResult);
    SetValue(key: closestPointIndexPropertyKey, value: searchResult.Index);
    if(!searchResult.IsEmpty) {
      marker.Visibility = Visibility.Visible;
      IList<Point> points_ = source_.VisiblePoints;
      var ptBefore_ = points_[index: searchResult.Index];
      var ptAfter_ = points_[index: searchResult.Index + 1];

      var ratio_ = (x_ - ptBefore_.X) / (ptAfter_.X - ptBefore_.X);
      var y_ = ptBefore_.Y + (ptAfter_.Y - ptBefore_.Y) * ratio_;
      Point temp_ = new(x: x_, y: y_);
      SetX(obj: marker, value: temp_.X);
      SetY(obj: marker, value: temp_.Y);

      var markerPosition_ = temp_;
      followDataContext.Position = markerPosition_;
      SetValue(key: markerPositionPropertyKey, value: markerPosition_);
    }
    else {
      SetValue(key: markerPositionPropertyKey, value: new Point(x: double.NaN, y: double.NaN));
      marker.Visibility = Visibility.Hidden;
    }
  }

  #region ClosestPointIndex property

  private static readonly DependencyPropertyKey closestPointIndexPropertyKey = DependencyProperty.RegisterReadOnly(
    name: "ClosestPointIndex",
    propertyType: typeof(int),
    ownerType: typeof(DataFollowChart),
    typeMetadata: new PropertyMetadata(defaultValue: -1));

  public static readonly DependencyProperty ClosestPointIndexProperty = closestPointIndexPropertyKey.DependencyProperty;

  public int ClosestPointIndex => (int)GetValue(dp: ClosestPointIndexProperty);

  #endregion

  #region MarkerPositionProperty

  private static readonly DependencyPropertyKey markerPositionPropertyKey = DependencyProperty.RegisterReadOnly(
    name: "MarkerPosition",
    propertyType: typeof(Point),
    ownerType: typeof(DataFollowChart),
    typeMetadata: new PropertyMetadata(defaultValue: new Point(), propertyChangedCallback: OnMarkerPositionChanged));

  private static void OnMarkerPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var chart_ = (DataFollowChart)d;
    chart_.MarkerPositionChanged.Raise(sender: chart_);
  }

  /// <summary>
  ///   Identifies the <see cref="MarkerPosition"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty MarkerPositionProperty = markerPositionPropertyKey.DependencyProperty;

  /// <summary>
  ///   Gets the marker position.
  /// </summary>
  /// <value>
  ///   The marker position.
  /// </value>
  public Point MarkerPosition => (Point)GetValue(dp: MarkerPositionProperty);

  /// <summary>
  ///   Occurs when marker position changes.
  /// </summary>
  public event EventHandler MarkerPositionChanged;

  #endregion

  public override void OnPlotterAttached(PlotterBase plotter) {
    base.OnPlotterAttached(plotter: plotter);
    plotter.MainGrid.MouseMove += MainGrid_MouseMove;
  }

  private void MainGrid_MouseMove(object sender, MouseEventArgs e) {
    UpdateUIRepresentation();
  }

  public override void OnPlotterDetaching(PlotterBase plotter) {
    plotter.MainGrid.MouseMove -= MainGrid_MouseMove;
    base.OnPlotterDetaching(plotter: plotter);
  }

  protected override void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e) {
    base.Viewport_PropertyChanged(sender: sender, e: e);
    UpdateUIRepresentation();
  }

  private void Source_VisiblePointsChanged(object sender, EventArgs e) {
    var source_ = (PointsGraphBase)sender;
    if(source_.VisiblePoints != null) {
      searcher = new SortedXSearcher1d(_collection: source_.VisiblePoints);
    }
    UpdateUIRepresentation();
  }

  #endregion

  #region INotifyPropertyChanged Members

  /// <summary>
  ///   Occurs when a property value changes.
  /// </summary>
  public event PropertyChangedEventHandler PropertyChanged;

  #endregion
}

/// <summary>
///   Represents a special data context, which encapsulates marker's position and custom data.
///   Used in <see cref="DataFollowChart"/>.
/// </summary>
public sealed class FollowDataContext : INotifyPropertyChanged {
  private Point position;

  /// <summary>
  ///   Gets or sets the position of marker.
  /// </summary>
  /// <value>
  ///   The position.
  /// </value>
  public Point Position {
    get => position;
    set {
      position = value;
      PropertyChanged.Raise(sender: this, propertyName: "Position");
    }
  }

  private object data;

  /// <summary>
  ///   Gets or sets the additional custom data.
  /// </summary>
  /// <value>
  ///   The data.
  /// </value>
  public object Data {
    get => data;
    set {
      data = value;
      PropertyChanged.Raise(sender: this, propertyName: "Data");
    }
  }

  #region INotifyPropertyChanged Members

  /// <summary>
  ///   Occurs when a property value changes.
  /// </summary>
  public event PropertyChangedEventHandler PropertyChanged;

  #endregion
}