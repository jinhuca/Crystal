namespace Crystal.Plot2D.Axes.TimeSpan;

/// <summary>
/// Represents axis with values of type TimeSpan.
/// </summary>
public class TimeSpanAxis : AxisBase<System.TimeSpan> {
  /// <summary>
  /// Initializes a new instance of the <see cref="TimeSpanAxis"/> class with default values conversion.
  /// </summary>
  public TimeSpanAxis()
    : base(axisControl: new TimeSpanAxisControl(),
      convertFromDouble: DoubleToTimeSpan, convertToDouble: TimeSpanToDouble) { }

  private static readonly long minTicks = System.TimeSpan.MinValue.Ticks;
  private static readonly long maxTicks = System.TimeSpan.MaxValue.Ticks;
  private static System.TimeSpan DoubleToTimeSpan(double value) {
    var ticks_ = (long)(value * 10000000000L);

    // todo should we throw an exception if number of ticks is too big or small?
    if(ticks_ < minTicks) {
      ticks_ = minTicks;
    }
    else if(ticks_ > maxTicks) {
      ticks_ = maxTicks;
    }

    return new System.TimeSpan(ticks: ticks_);
  }

  private static double TimeSpanToDouble(System.TimeSpan time) {
    return time.Ticks / 10000000000.0;
  }

  /// <summary>
  /// Sets conversions of axis - functions used to convert values of axis type to and from double values of viewport.
  /// Sets both ConvertToDouble and ConvertFromDouble properties.
  /// </summary>
  /// <param name="min">The minimal viewport value.</param>
  /// <param name="minValue">The value of axis type, corresponding to minimal viewport value.</param>
  /// <param name="max">The maximal viewport value.</param>
  /// <param name="maxValue">The value of axis type, corresponding to maximal viewport value.</param>
  public override void SetConversion(double min, System.TimeSpan minValue, double max, System.TimeSpan maxValue) {
    var conversion_ = new TimeSpanToDoubleConversion(min: min, minSpan: minValue, max: max, maxSpan: maxValue);

    ConvertToDouble = conversion_.ToDouble;
    ConvertFromDouble = conversion_.FromDouble;
  }
}
