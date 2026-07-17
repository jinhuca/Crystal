using Crystal.Plot2D;
using Crystal.Plot2D.Axes.Numeric;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.DataSources.OneDimensional;
using Crystal.Plot2D.Graphs;
using Crystal.Plot2D.Transforms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ResourceModule.Controls.Plotter;

public partial class PlotterControl : UserControl {
  private readonly ObservableCollection<Point> _data = new();
  private int x = 0;
  private readonly Random rnd = new();
  private LineGraph? xLineGraph;
  private Path areaPath;
  private const double Baseline = 0.0;

  private readonly List<Path> _areaPaths = new();
private readonly List<LineGraph?> _seriesList = new();

  public PlotterControl() {
    InitializeComponent();

    this.Loaded += (_, __) => {
      //DisablePlotterNavigation();

      // ✅ Guard the first insert too
      if (!plotter.CentralGrid.Children.Contains(areaPath))
        plotter.CentralGrid.Children.Insert(0, areaPath);

      // ✅ Only initialize once — re-navigation reuses the same instance
      if (xLineGraph == null)
        InitializePlot();

      plotter.Background = PlotterBackground ?? plotter.Background;
      ApplyPlotStroke(PlotStroke);

      if (DataSource != null) HandleDataSource(DataSource);

      // second insert is now redundant — remove it
    };

    areaPath = new Path {
    Fill = RangeAreaBrush ?? new SolidColorBrush(Color.FromArgb(120, 30, 144, 255)),
      Stroke = null,
      IsHitTestVisible = false,
      Visibility = Visibility.Collapsed
    };
    Panel.SetZIndex(areaPath, -1000);

    plotter.Background = PlotterBackground ?? new SolidColorBrush(Color.FromRgb(18, 18, 18));
    plotter.AxisGrid.GridPath.Stroke = AxisGridStroke;
    plotter.AxisGrid.GridPath.StrokeDashArray = [1];
    
    plotter.MainHorizontalAxisVisibility = Visibility.Collapsed;
    plotter.MainVerticalAxisVisibility = Visibility.Collapsed;
  }

  #region Dependency Properties (expose plotter Background and stroke)

  public Brush AxisGridStroke {
    get => (Brush)GetValue(AxisGridStrokeProperty);
    set => SetValue(AxisGridStrokeProperty, value);
  }

  public static readonly DependencyProperty AxisGridStrokeProperty =
    DependencyProperty.Register(
      name: nameof(AxisGridStroke),
      propertyType: typeof(Brush),
      ownerType: typeof(PlotterControl),
      typeMetadata: new FrameworkPropertyMetadata(
        new SolidColorBrush(Color.FromArgb(90, 100, 100, 100)), 
        FrameworkPropertyMetadataOptions.None, OnAxisGridStrokeChanged));

  private static void OnAxisGridStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var control = (PlotterControl)d;
    var brush = (Brush)e.NewValue;
    if (control.plotter != null) control.plotter.AxisGrid.GridPath.Stroke = brush;
  }

  public Brush PlotterBackground {
    get => (Brush)GetValue(PlotterBackgroundProperty);
    set => SetValue(PlotterBackgroundProperty, value);
  }

  public static readonly DependencyProperty PlotterBackgroundProperty =
    DependencyProperty.Register(
      name: nameof(PlotterBackground),
      propertyType: typeof(Brush),
      ownerType: typeof(PlotterControl),
      typeMetadata: new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.None, OnPlotterBackgroundChanged));

  private static void OnPlotterBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var control = (PlotterControl)d;
    var brush = (Brush)e.NewValue;
    if(control.plotter != null) control.plotter.Background = brush;
  }

  public Brush PlotStroke {
    get => (Brush)GetValue(PlotStrokeProperty);
    set => SetValue(PlotStrokeProperty, value);
  }

  public static readonly DependencyProperty PlotStrokeProperty =
    DependencyProperty.Register(
      name: nameof(PlotStroke),
      propertyType: typeof(Brush),
      ownerType: typeof(PlotterControl),
      typeMetadata: new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.None, OnPlotStrokeChanged));

  private static void OnPlotStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var control = (PlotterControl)d;
    var brush = (Brush)e.NewValue;
    control.ApplyPlotStroke(brush);
  }

  private void ApplyPlotStroke(Brush brush) {
    if(xLineGraph != null) {
      var thickness = xLineGraph.LinePen?.Thickness ?? 1.0;
      xLineGraph.LinePen = new Pen(brush ?? Brushes.Black, thickness);
    }
  }

  #endregion

  // New flexible DP: accepts object and converts supported types to internal data
  public object? DataSource {
    get => GetValue(DataSourceProperty);
    set => SetValue(DataSourceProperty, value);
  }

  public static readonly DependencyProperty DataSourceProperty =
    DependencyProperty.Register(
      name: nameof(DataSource),
      propertyType: typeof(object),
      ownerType: typeof(PlotterControl),
      typeMetadata: new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.None, OnDataSourceChanged));

  private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var control = (PlotterControl)d;
    control.Dispatcher.Invoke(() => control.HandleDataSource(e.NewValue));
  }

  // central handler converts supported input types into points or appends numeric values
  private void HandleDataSource(object? value) {
    if(value == null) return;

    // direct points
    if(value is IEnumerable<Point> pointsEnumerable) {
      SetData(pointsEnumerable);
      return;
    }

    // enumerable of numeric types
    if(value is IEnumerable<double> doubles) {
      var list = doubles.Select((v, i) => new Point(x + i, v)).ToList();
      SetData(list);
      return;
    }
    if(value is IEnumerable<float> floats) {
      var list = floats.Select((v, i) => new Point(x + i, v)).ToList();
      SetData(list);
      return;
    }
    if(value is IEnumerable<int> ints) {
      var list = ints.Select((v, i) => new Point(x + i, v)).ToList();
      SetData(list);
      return;
    }

    // generic enumerable (attempt conversion of elements)
    if(value is System.Collections.IEnumerable enumObj) {
      var ptsList = new List<Point>();
      int idx = 0;
      foreach(var item in enumObj) {
        if(item == null) { idx++; continue; }
        if(item is Point p) { ptsList.Add(p); }
        else if(item is double dv) ptsList.Add(new Point(x + idx, dv));
        else if(item is float fv) ptsList.Add(new Point(x + idx, fv));
        else if(item is int iv) ptsList.Add(new Point(x + idx, iv));
        else if(double.TryParse(item.ToString(), out var parsed)) ptsList.Add(new Point(x + idx, parsed));
        idx++;
      }
      if(ptsList.Count > 0) {
        SetData(ptsList);
        return;
      }
    }

    // single numeric values -> append point
    if(value is double dVal) { AddPoint(dVal); return; }
    if(value is float fVal) { AddPoint(fVal); return; }
    if(value is int iVal) { AddPoint(iVal); return; }
    if(value is string sVal && double.TryParse(sVal, out var parsedVal)) { AddPoint(parsedVal); return; }

    // unsupported types are ignored silently
  }

  #region Enabled Title and Footer Text DPs

  public Visibility IsTitleEnabled {
    get => (Visibility)GetValue(IsTitleEnabledProperty);
    set => SetValue(IsTitleEnabledProperty, value);
  }

  public static readonly DependencyProperty IsTitleEnabledProperty =
    DependencyProperty.Register(
      name: nameof(IsTitleEnabled),
      propertyType: typeof(Visibility),
      ownerType: typeof(PlotterControl),
      typeMetadata: new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.None));

  #endregion Enabled Title and Footer Text DPs

  #region Title Text Dependency Property

  public string TitleText {
    get => (string)GetValue(TitleTextProperty);
    set => SetValue(TitleTextProperty, value);
  }

  public static readonly DependencyProperty TitleTextProperty =
    DependencyProperty.Register(
      name: nameof(TitleText),
      propertyType: typeof(string),
      ownerType: typeof(PlotterControl),
      typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None));

  #endregion Title Text Dependency Property

  #region Value Text Dependency Property

  public string ValueText {
    get => (string)GetValue(ValueTextProperty);
    set => SetValue(ValueTextProperty, value);
  }

  public static readonly DependencyProperty ValueTextProperty =
    DependencyProperty.Register(
      name: nameof(ValueText),
      propertyType: typeof(string),
      ownerType: typeof(PlotterControl),
      typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None));

  #endregion Value Text Dependency Property

  // Global Y range properties (user can set YMin as baseline and YMax)
  public double YMin {
    get => (double)GetValue(YMinProperty);
    set => SetValue(YMinProperty, value);
  }

  public static readonly DependencyProperty YMinProperty =
    DependencyProperty.Register(
      name: nameof(YMin),
      propertyType: typeof(double),
      ownerType: typeof(PlotterControl),
      typeMetadata: new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.None, OnYRangeChanged));

  public double YMax {
    get => (double)GetValue(YMaxProperty);
    set => SetValue(YMaxProperty, value);
  }

  public static readonly DependencyProperty YMaxProperty =
    DependencyProperty.Register(
      name: nameof(YMax),
      propertyType: typeof(double),
      ownerType: typeof(PlotterControl),
      typeMetadata: new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.None, OnYRangeChanged));

  private static void OnYRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var control = (PlotterControl)d;
    control.Dispatcher.BeginInvoke((Action)(() => {
      control.UpdateArea();
      control.UpdateMultiAreas();
    }), DispatcherPriority.Render);
  }

  // Return effective Y range: if YMin/YMax are NaN use defaults (Baseline/100)
  private (double ymin, double ymax) EffectiveYRange() {
    double ymin = double.IsNaN(YMin) ? Baseline : YMin;
    double ymax = double.IsNaN(YMax) ? 100.0 : YMax;
    return (ymin, ymax);
  }


  // Range area brush dependency property (used when series.Fill is not set)
  public Brush RangeAreaBrush {
    get => (Brush)GetValue(RangeAreaBrushProperty);
    set => SetValue(RangeAreaBrushProperty, value);
  }

  public static readonly DependencyProperty RangeAreaBrushProperty =
    DependencyProperty.Register(
      name: nameof(RangeAreaBrush),
      propertyType: typeof(Brush),
      ownerType: typeof(PlotterControl),
      typeMetadata: new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(120, 30, 144, 255)), FrameworkPropertyMetadataOptions.None, OnRangeAreaBrushChanged));

  private static void OnRangeAreaBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
  var control = (PlotterControl)d;
  var brush = (Brush)e.NewValue;
  control.Dispatcher.BeginInvoke((Action)(() => {
    if(control.areaPath != null) control.areaPath.Fill = brush;
    for(int i = 0; i < control._areaPaths.Count; i++) {
      var p = control._areaPaths[i];
      if(p == null) continue;
      var series = i < control._seriesList.Count ? control._seriesList[i] : null;

      // only overwrite fills for series that don't have explicit Fill set
      bool hasExplicitFill = false;
      if(series != null) {
        var prop = series.GetType().GetProperty("Fill");
        if(prop != null) {
          var val = prop.GetValue(series);
          hasExplicitFill = val != null;
        }
      }

      if(!hasExplicitFill) p.Fill = brush;
    }
  }), DispatcherPriority.Render);
}

  // Hook for potential multi-series area updates (kept simple here)
  private void UpdateMultiAreas() {
    // If multi-series areas are present this would update them.
    // For now ensure the single area is refreshed.
    UpdateArea();
  }

  private bool _initialized = false;

  private void InitializePlot() {
    if (_initialized) return;
    _initialized = true;

    xLineGraph = plotter.AddLineGraph(_data.AsDataSource());
    var initialStroke = PlotStroke ?? new SolidColorBrush(Color.FromRgb(173, 216, 230));
    xLineGraph.LinePen = new Pen(initialStroke, 1);
    xLineGraph.Visibility = Visibility.Collapsed;

    bool HasNonBaselineData() => _data.Any(p => !DoubleEquals(p.Y, Baseline));

    _data.CollectionChanged += (s, e) => {
      if (Dispatcher.CheckAccess()) {
        var has = HasNonBaselineData();
        xLineGraph.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
        areaPath.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
        UpdateArea();
      }
      else {
        Dispatcher.BeginInvoke((Action)(() => {
          var has = HasNonBaselineData();
          xLineGraph.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
          areaPath.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
          UpdateArea();
        }), DispatcherPriority.Render);
      }
    };

    plotter.Viewport.PropertyChanged += (s, e) =>
        Dispatcher.BeginInvoke((Action)UpdateArea, DispatcherPriority.Render);

    plotter.SizeChanged += (s, e) =>
        Dispatcher.BeginInvoke((Action)UpdateArea, DispatcherPriority.Render);

    for (int i = 0; i < 60; i++) Add60Point();

    UpdateArea();
  }

  private void UpdateArea() {
    if(areaPath == null || _data.Count < 2 || plotter == null) return;

    // Determine baseline: prefer global YMin if set, otherwise fall back to default Baseline const
    double baseline = double.IsNaN(YMin) ? Baseline : YMin;

    if(_data.All(p => DoubleEquals(p.Y, baseline))) {
      areaPath.Data = null;
      return;
    }

    var transform = plotter.Viewport.Transform;
    var geom = new StreamGeometry();
    using(var ctx = geom.Open()) {
      var firstScreen = new Point(_data[0].X, _data[0].Y).DataToScreen(transform: transform);
      ctx.BeginFigure(firstScreen, isFilled: true, isClosed: true);
      for(int i = 1; i < _data.Count; i++) {
        var screen = new Point(_data[i].X, _data[i].Y).DataToScreen(transform: transform);
        ctx.LineTo(screen, isStroked: false, isSmoothJoin: false);
      }
      var lastBaseline = new Point(_data[^1].X, baseline).DataToScreen(transform: transform);
      ctx.LineTo(lastBaseline, isStroked: false, isSmoothJoin: false);
      var firstBaseline = new Point(_data[0].X, baseline).DataToScreen(transform: transform);
      ctx.LineTo(firstBaseline, isStroked: false, isSmoothJoin: false);
    }
    geom.Freeze();
    areaPath.Data = geom;
  }

  private void Add60Point() {
    var p = new Point { X = x++, Y = 0 };
    _data.Add(p);
  }

  private static bool DoubleEquals(double a, double b, double eps = 1e-9) => Math.Abs(a - b) <= eps;

  public void AddPoint(double y) {
    Dispatcher.Invoke(() => {
      var p = new Point { X = x++, Y = y };
      _data.Add(p);
      if (_data.Count > 60) _data.RemoveAt(0);
      if (_data.Count > 0) {
        double xMax = _data.Last().X;
        double xMin = Math.Max(0, xMax - 59);
        // Use effective Y range (global YMin/YMax) when updating viewport
        var (ymin, ymax) = EffectiveYRange();
        plotter.Viewport.Visible = new DataRect(new Rect(xMin, ymin, xMax - xMin, ymax - ymin));
        // update area immediately so it stays in sync with viewport & line graph
        UpdateArea();
      }
    });
  }

  public void AddPoint(Point p) {
    Dispatcher.Invoke(() => {
      _data.Add(p);
      if(_data.Count > 60) _data.RemoveAt(0);
      UpdateArea();
    });
  }

  public void SetData(IEnumerable<Point> points) {
    Dispatcher.Invoke(() => {
      _data.Clear();
      foreach(var p in points) _data.Add(p);
      x = _data.Any() ? (int)(_data.Max(pt => pt.X) + 1) : x;
      UpdateArea();
    });
  }

  public void ClearData() {
    Dispatcher.Invoke(() => {
      _data.Clear();
      areaPath.Data = null;
      xLineGraph?.Visibility = Visibility.Collapsed;
      areaPath.Visibility = Visibility.Collapsed;
    });
  }

  public void Refresh() {
    Dispatcher.BeginInvoke((Action)UpdateArea, DispatcherPriority.Render);
  }

  //private void DisablePlotterNavigation() {
  //  if(plotter == null) return;
  //  RemoveNavigationChildren();
  //  plotter.Children.CollectionChanged += (s, e) => {
  //    if(e.NewItems == null) return;
  //    var toRemove = e.NewItems.OfType<IPlotterElement>()
  //      .Where(it => it is NavigationBase || it is KeyboardNavigation || it is TouchpadScroll)
  //      .ToList();
  //    if(toRemove.Count == 0) return;
  //    Dispatcher.BeginInvoke((Action)(() => {
  //      foreach(var item in toRemove) {
  //        if(plotter.Children.Contains(item)) plotter.Children.Remove(item);
  //      }
  //    }), DispatcherPriority.Normal);
  //  };
  //}

  //private void RemoveNavigationChildren() {
  //  var navs = plotter.Children
  //    .OfType<IPlotterElement>()
  //    .Where(it => it is NavigationBase || it is KeyboardNavigation || it is TouchpadScroll)
  //    .ToList();

  //  foreach(var nav in navs) {
  //    if(plotter.Children.Contains(nav)) plotter.Children.Remove(nav);
  //  }
  //}
}