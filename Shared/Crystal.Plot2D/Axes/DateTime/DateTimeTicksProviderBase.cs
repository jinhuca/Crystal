using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;

namespace Crystal.Plot2D.Axes;

public abstract class DateTimeTicksProviderBase : ITicksProvider<System.DateTime> {
  public event EventHandler Changed;

  protected void RaiseChanged() {
    if(Changed != null) {
      Changed(sender: this, e: EventArgs.Empty);
    }
  }

  protected static System.DateTime Shift(System.DateTime dateTime, DifferenceIn diff) {
    var res = dateTime;

    switch(diff) {
      case DifferenceIn.Year:
        res = res.AddYears(value: 1);
        break;
      case DifferenceIn.Month:
        res = res.AddMonths(months: 1);
        break;
      case DifferenceIn.Day:
        res = res.AddDays(value: 1);
        break;
      case DifferenceIn.Hour:
        res = res.AddHours(value: 1);
        break;
      case DifferenceIn.Minute:
        res = res.AddMinutes(value: 1);
        break;
      case DifferenceIn.Second:
        res = res.AddSeconds(value: 1);
        break;
      case DifferenceIn.Millisecond:
        res = res.AddMilliseconds(value: 1);
        break;
    }

    return res;
  }

  protected static System.DateTime RoundDown(System.DateTime dateTime, DifferenceIn diff) {
    var res = dateTime;

    switch(diff) {
      case DifferenceIn.Year:
        res = new System.DateTime(year: dateTime.Year, month: 1, day: 1);
        break;
      case DifferenceIn.Month:
        res = new System.DateTime(year: dateTime.Year, month: dateTime.Month, day: 1);
        break;
      case DifferenceIn.Day:
        res = dateTime.Date;
        break;
      case DifferenceIn.Hour:
        res = dateTime.Date.AddHours(value: dateTime.Hour);
        break;
      case DifferenceIn.Minute:
        res = dateTime.Date.AddHours(value: dateTime.Hour).AddMinutes(value: dateTime.Minute);
        break;
      case DifferenceIn.Second:
        res = dateTime.Date.AddHours(value: dateTime.Hour).AddMinutes(value: dateTime.Minute).AddSeconds(value: dateTime.Second);
        break;
      case DifferenceIn.Millisecond:
        res = dateTime.Date.AddHours(value: dateTime.Hour).AddMinutes(value: dateTime.Minute).AddSeconds(value: dateTime.Second).AddMilliseconds(value: dateTime.Millisecond);
        break;
    }

    DebugVerify.Is(condition: res <= dateTime);

    return res;
  }

  protected static System.DateTime RoundUp(System.DateTime dateTime, DifferenceIn diff) {
    var res = RoundDown(dateTime: dateTime, diff: diff);

    switch(diff) {
      case DifferenceIn.Year:
        res = res.AddYears(value: 1);
        break;
      case DifferenceIn.Month:
        res = res.AddMonths(months: 1);
        break;
      case DifferenceIn.Day:
        res = res.AddDays(value: 1);
        break;
      case DifferenceIn.Hour:
        res = res.AddHours(value: 1);
        break;
      case DifferenceIn.Minute:
        res = res.AddMinutes(value: 1);
        break;
      case DifferenceIn.Second:
        res = res.AddSeconds(value: 1);
        break;
      case DifferenceIn.Millisecond:
        res = res.AddMilliseconds(value: 1);
        break;
    }

    return res;
  }

  #region ITicksProvider<DateTime> Members

  public abstract ITicksInfo<System.DateTime> GetTicks(Range<System.DateTime> range, int ticksCount);
  public abstract int DecreaseTickCount(int ticksCount);
  public abstract int IncreaseTickCount(int ticksCount);
  public abstract ITicksProvider<System.DateTime> MinorProvider { get; }
  public abstract ITicksProvider<System.DateTime> MajorProvider { get; }

  #endregion
}
