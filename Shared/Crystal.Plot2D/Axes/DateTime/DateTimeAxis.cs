using Crystal.Plot2D.Common;
using Crystal.Plot2D.ViewportConstraints;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystal.Plot2D.Axes;

/// <summary>
/// Represents an axis with ticks of <see cref="System.DateTime"/> type.
/// </summary>
public class DateTimeAxis : AxisBase<System.DateTime> {
  /// <summary>
  /// Initializes a new instance of the <see cref="DateTimeAxis"/> class.
  /// </summary>
  public DateTimeAxis()
      : base(axisControl: new DateTimeAxisControl(), convertFromDouble: DoubleToDate,
          convertToDouble: dt => dt.Ticks / 10000000000.0) {
    AxisControl.SetBinding(dp: MajorLabelBackgroundBrushProperty, binding: new Binding(path: "MajorLabelBackgroundBrush") { Source = this });
    AxisControl.SetBinding(dp: MajorLabelRectangleBorderPropertyProperty, binding: new Binding(path: "MajorLabelRectangleBorderProperty") { Source = this });
  }

  #region VisualProperties

  /// <summary>
  /// Gets or sets the major tick labels' background brush. This is a DependencyProperty.
  /// </summary>
  /// <value>The major label background brush.</value>
  public Brush MajorLabelBackgroundBrush {
    get => (Brush)GetValue(dp: MajorLabelBackgroundBrushProperty);
    set => SetValue(dp: MajorLabelBackgroundBrushProperty, value: value);
  }

  public static readonly DependencyProperty MajorLabelBackgroundBrushProperty = DependencyProperty.Register(
    name: nameof(MajorLabelBackgroundBrush),
    propertyType: typeof(Brush),
    ownerType: typeof(DateTimeAxis),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Beige));


  public Brush MajorLabelRectangleBorderProperty {
    get => (Brush)GetValue(dp: MajorLabelRectangleBorderPropertyProperty);
    set => SetValue(dp: MajorLabelRectangleBorderPropertyProperty, value: value);
  }

  public static readonly DependencyProperty MajorLabelRectangleBorderPropertyProperty = DependencyProperty.Register(
    name: nameof(MajorLabelRectangleBorderProperty),
    propertyType: typeof(Brush),
    ownerType: typeof(DateTimeAxis),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Peru));

  #endregion // end of VisualProperties

  private ViewportConstraint constraint = new DateTimeHorizontalAxisConstraint();
  protected ViewportConstraint Constraint {
    get => constraint;
    set => constraint = value;
  }

  protected override void OnPlotterAttached(PlotterBase thePlotter) {
    base.OnPlotterAttached(thePlotter: thePlotter);

    thePlotter.Viewport.Constraints.Add(item: constraint);
  }

  protected override void OnPlotterDetaching(PlotterBase thePlotter) {
    thePlotter.Viewport.Constraints.Remove(item: constraint);

    base.OnPlotterDetaching(thePlotter: thePlotter);
  }

  private static readonly long minTicks = System.DateTime.MinValue.Ticks;
  private static readonly long maxTicks = System.DateTime.MaxValue.Ticks;
  private static System.DateTime DoubleToDate(double d) {
    var ticks = (long)(d * 10000000000L);

    // todo should we throw an exception if number of ticks is too big or small?
    if(ticks < minTicks) {
      ticks = minTicks;
    }
    else if(ticks > maxTicks) {
      ticks = maxTicks;
    }

    return new System.DateTime(ticks: ticks);
  }

  /// <summary>
  /// Sets conversions of axis - functions used to convert values of axis type to and from double values of viewport.
  /// Sets both ConvertToDouble and ConvertFromDouble properties.
  /// </summary>
  /// <param name="min">The minimal viewport value.</param>
  /// <param name="minValue">The value of axis type, corresponding to minimal viewport value.</param>
  /// <param name="max">The maximal viewport value.</param>
  /// <param name="maxValue">The value of axis type, corresponding to maximal viewport value.</param>
  public override void SetConversion(double min, System.DateTime minValue, double max, System.DateTime maxValue) {
    var conversion = new DateTimeToDoubleConversion(min: min, minDate: minValue, max: max, maxDate: maxValue);

    ConvertToDouble = conversion.ToDouble;
    ConvertFromDouble = conversion.FromDouble;
  }
}
