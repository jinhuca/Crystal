using Crystal.Plot2D.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Shapes;

/// <summary>
/// Represents rectangle with corners bound to viewport coordinates.
/// </summary>
[TemplatePart(Name = "PART_LinesPath", Type = typeof(Path))]
[TemplatePart(Name = "PART_RectPath", Type = typeof(Path))]
public abstract class RangeHighlight : ContentControl, IPlotterElement {
  /// <summary>
  /// Initializes a new instance of the <see cref="RangeHighlight"/> class.
  /// </summary>
  protected RangeHighlight() {
    Resources = new ResourceDictionary { Source = new Uri(uriString: Constants.Constants.ShapeResourceUri, uriKind: UriKind.Relative) };
    Style = (Style)FindResource(resourceKey: typeof(RangeHighlight));
    ApplyTemplate();
  }

  private bool partsLoaded;
  protected bool PartsLoaded => partsLoaded;

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();

    linesPath = (Path)Template.FindName(name: "PART_LinesPath", templatedParent: this);
    GeometryGroup linesGroup = new();
    linesGroup.Children.Add(value: lineGeometry1);
    linesGroup.Children.Add(value: lineGeometry2);
    linesPath.Data = linesGroup;

    rectPath = (Path)Template.FindName(name: "PART_RectPath", templatedParent: this);
    rectPath.Data = rectGeometry;

    partsLoaded = true;
  }

  #region Presentation DPs

  public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
    name: nameof(Fill),
    propertyType: typeof(Brush),
    ownerType: typeof(RangeHighlight),
    typeMetadata: new PropertyMetadata(defaultValue: Shape.FillProperty.DefaultMetadata.DefaultValue));

  public Brush Fill {
    get => (Brush)GetValue(dp: FillProperty);
    set => SetValue(dp: FillProperty, value: value);
  }

  public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
    name: nameof(Stroke),
    propertyType: typeof(Brush),
    ownerType: typeof(RangeHighlight),
    typeMetadata: new PropertyMetadata(defaultValue: Shape.StrokeProperty.DefaultMetadata.DefaultValue));

  public Brush Stroke {
    get => (Brush)GetValue(dp: StrokeProperty);
    set => SetValue(dp: StrokeProperty, value: value);
  }

  public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
    name: nameof(StrokeThickness),
    propertyType: typeof(double),
    ownerType: typeof(RangeHighlight),
    typeMetadata: new PropertyMetadata(defaultValue: Shape.StrokeThicknessProperty.DefaultMetadata.DefaultValue));

  public double StrokeThickness {
    get => (double)GetValue(dp: StrokeThicknessProperty);
    set => SetValue(dp: StrokeThicknessProperty, value: value);
  }

  public static readonly DependencyProperty StrokeStartLineCapProperty = DependencyProperty.Register(
    name: nameof(StrokeStartLineCap),
    propertyType: typeof(PenLineCap),
    ownerType: typeof(RangeHighlight),
    typeMetadata: new PropertyMetadata(defaultValue: Shape.StrokeStartLineCapProperty.DefaultMetadata.DefaultValue));

  public PenLineCap StrokeStartLineCap {
    get => (PenLineCap)GetValue(dp: StrokeStartLineCapProperty);
    set => SetValue(dp: StrokeStartLineCapProperty, value: value);
  }

  public static readonly DependencyProperty StrokeEndLineCapProperty = DependencyProperty.Register(
    name: nameof(StrokeEndLineCap),
    propertyType: typeof(PenLineCap),
    ownerType: typeof(RangeHighlight),
    typeMetadata: new PropertyMetadata(defaultValue: Shape.StrokeEndLineCapProperty.DefaultMetadata.DefaultValue));

  public PenLineCap StrokeEndLineCap {
    get => (PenLineCap)GetValue(dp: StrokeEndLineCapProperty);
    set => SetValue(dp: StrokeEndLineCapProperty, value: value);
  }

  public static readonly DependencyProperty StrokeDashCapProperty = DependencyProperty.Register(
    name: nameof(StrokeDashCap),
    propertyType: typeof(PenLineCap),
    ownerType: typeof(RangeHighlight),
    typeMetadata: new PropertyMetadata(defaultValue: Shape.StrokeDashCapProperty.DefaultMetadata.DefaultValue));

  public PenLineCap StrokeDashCap {
    get => (PenLineCap)GetValue(dp: StrokeDashCapProperty);
    set => SetValue(dp: StrokeDashCapProperty, value: value);
  }

  public static readonly DependencyProperty StrokeLineJoinProperty = DependencyProperty.Register(
    name: nameof(StrokeLineJoin),
    propertyType: typeof(PenLineJoin),
    ownerType: typeof(RangeHighlight),
    typeMetadata: new PropertyMetadata(defaultValue: Shape.StrokeLineJoinProperty.DefaultMetadata.DefaultValue));

  public PenLineJoin StrokeLineJoin {
    get => (PenLineJoin)GetValue(dp: StrokeLineJoinProperty);
    set => SetValue(dp: StrokeLineJoinProperty, value: value);
  }

  public static readonly DependencyProperty StrokeMiterLimitProperty = DependencyProperty.Register(
    name: nameof(StrokeMiterLimit),
    propertyType: typeof(double),
    ownerType: typeof(RangeHighlight),
    typeMetadata: new PropertyMetadata(defaultValue: Shape.StrokeMiterLimitProperty.DefaultMetadata.DefaultValue));

  public double StrokeMiterLimit {
    get => (double)GetValue(dp: StrokeMiterLimitProperty);
    set => SetValue(dp: StrokeMiterLimitProperty, value: value);
  }

  public static readonly DependencyProperty StrokeDashOffsetProperty = DependencyProperty.Register(
    name: nameof(StrokeDashOffset),
    propertyType: typeof(double),
    ownerType: typeof(RangeHighlight),
    typeMetadata: new PropertyMetadata(defaultValue: Shape.StrokeDashOffsetProperty.DefaultMetadata.DefaultValue));

  public double StrokeDashOffset {
    get => (double)GetValue(dp: StrokeDashOffsetProperty);
    set => SetValue(dp: StrokeDashOffsetProperty, value: value);
  }

  public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register(
    name: nameof(StrokeDashArray),
    propertyType: typeof(DoubleCollection),
    ownerType: typeof(RangeHighlight),
    typeMetadata: new PropertyMetadata(defaultValue: Shape.StrokeDashArrayProperty.DefaultMetadata.DefaultValue));

  [SuppressMessage(category: "Microsoft.Usage", checkId: "CA2227:CollectionPropertiesShouldBeReadOnly")]
  public DoubleCollection StrokeDashArray {
    get => (DoubleCollection)GetValue(dp: StrokeDashArrayProperty);
    set => SetValue(dp: StrokeDashArrayProperty, value: value);
  }

  #endregion

  #region Values dependency properties

  /// <summary>
  /// Gets or sets the first value determining position of rectangle in viewport coordinates.
  /// </summary>
  /// <value>The StartValue.</value>
  public double StartValue {
    get => (double)GetValue(dp: StartValueProperty);
    set => SetValue(dp: StartValueProperty, value: value);
  }

  public static readonly DependencyProperty StartValueProperty =
    DependencyProperty.Register(
      name: nameof(StartValue),
      propertyType: typeof(double),
      ownerType: typeof(RangeHighlight),
      typeMetadata: new FrameworkPropertyMetadata(defaultValue: 0.0, propertyChangedCallback: OnValueChanged));

  private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var r = (RangeHighlight)d;
    r.OnValueChanged(e: e);
  }

  /// <summary>
  /// Gets or sets the second value determining position of rectangle in viewport coordinates.
  /// </summary>
  /// <value>The EndValue.</value>
  public double EndValue {
    get => (double)GetValue(dp: EndValueProperty);
    set => SetValue(dp: EndValueProperty, value: value);
  }

  public static readonly DependencyProperty EndValueProperty =
    DependencyProperty.Register(
      name: nameof(EndValue),
      propertyType: typeof(double),
      ownerType: typeof(RangeHighlight),
      typeMetadata: new FrameworkPropertyMetadata(defaultValue: 0.0, propertyChangedCallback: OnValueChanged));

  private void OnValueChanged(DependencyPropertyChangedEventArgs e) {
    UpdateUIRepresentation();
  }

  #endregion

  #region Geometry

  private Path rectPath;
  private Path linesPath;

  private readonly RectangleGeometry rectGeometry = new();
  protected RectangleGeometry RectGeometry => rectGeometry;

  private readonly LineGeometry lineGeometry1 = new();
  protected LineGeometry LineGeometry1 => lineGeometry1;

  private readonly LineGeometry lineGeometry2 = new();
  protected LineGeometry LineGeometry2 => lineGeometry2;

  #endregion

  #region IPlotterElement Members

  private PlotterBase plotter;

  void IPlotterElement.OnPlotterAttached(PlotterBase plotter) {
    plotter.CentralGrid.Children.Add(element: this);

    var plotter2d = (PlotterBase)plotter;
    this.plotter = plotter2d;
    plotter2d.Viewport.PropertyChanged += Viewport_PropertyChanged;

    UpdateUIRepresentation();
  }

  private void UpdateUIRepresentation() {
    if(Plotter == null) {
      return;
    }

    if(partsLoaded) {
      UpdateUIRepresentationCore();
    }
  }

  protected virtual void UpdateUIRepresentationCore() {
  }

  private void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e) {
    UpdateUIRepresentation();
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter) {
    var plotter2d = (PlotterBase)plotter;
    plotter2d.Viewport.PropertyChanged -= Viewport_PropertyChanged;
    plotter.CentralGrid.Children.Remove(element: this);

    this.plotter = null;
  }

  public PlotterBase Plotter => plotter;

  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion
}
