using System;

namespace Crystal.Plot2D.Axes;

/// <summary>
/// Contains default axis value conversions.
/// </summary>
public static class DefaultAxisConversions {
  #region double

  private static readonly Func<double, double> doubleToDouble = d => d;
  public static Func<double, double> DoubleToDouble => doubleToDouble;

  private static readonly Func<double, double> doubleFromDouble = d => d;
  public static Func<double, double> DoubleFromDouble => doubleFromDouble;

  #endregion

  #region DateTime

  private static readonly long minDateTimeTicks = DateTime.MinValue.Ticks;
  private static readonly long maxDateTimeTicks = DateTime.MaxValue.Ticks;
  private static readonly Func<double, System.DateTime> dateTimeFromDouble = d => {
    var ticks = (long)(d * 10000000000L);

    // todo should we throw an exception if number of ticks is too big or small?
    if(ticks < minDateTimeTicks) {
      ticks = minDateTimeTicks;
    }
    else if(ticks > maxDateTimeTicks) {
      ticks = maxDateTimeTicks;
    }

    return new System.DateTime(ticks: ticks);
  };
  public static Func<double, System.DateTime> DateTimeFromDouble => dateTimeFromDouble;

  private static readonly Func<System.DateTime, double> dateTimeToDouble = dt => dt.Ticks / 10000000000.0;
  public static Func<System.DateTime, double> DateTimeToDouble => dateTimeToDouble;

  #endregion

  #region TimeSpan

  private static readonly long minTimeSpanTicks = System.TimeSpan.MinValue.Ticks;
  private static readonly long maxTimeSpanTicks = System.TimeSpan.MaxValue.Ticks;

  private static readonly Func<double, System.TimeSpan> timeSpanFromDouble = d => {
    var ticks = (long)(d * 10000000000L);

    // todo should we throw an exception if number of ticks is too big or small?
    if(ticks < minTimeSpanTicks) {
      ticks = minTimeSpanTicks;
    }
    else if(ticks > maxTimeSpanTicks) {
      ticks = maxTimeSpanTicks;
    }

    return new System.TimeSpan(ticks: ticks);
  };

  public static Func<double, System.TimeSpan> TimeSpanFromDouble => timeSpanFromDouble;

  private static readonly Func<System.TimeSpan, double> timeSpanToDouble = timeSpan => {
    return timeSpan.Ticks / 10000000000.0;
  };

  public static Func<System.TimeSpan, double> TimeSpanToDouble => timeSpanToDouble;

  #endregion

  #region integer

  private static readonly Func<double, int> intFromDouble = d => (int)d;
  public static Func<double, int> IntFromDouble => intFromDouble;

  private static readonly Func<int, double> intToDouble = i => i;
  public static Func<int, double> IntToDouble => intToDouble;

  #endregion
}
