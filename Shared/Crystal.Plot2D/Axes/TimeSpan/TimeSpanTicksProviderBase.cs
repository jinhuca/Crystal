using Crystal.Plot2D.Common;
using System.Collections.Generic;

namespace Crystal.Plot2D.Axes.TimeSpan;

internal abstract class TimeSpanTicksProviderBase : TimePeriodTicksProvider<System.TimeSpan> {
  protected sealed override bool Continue(System.TimeSpan current, System.TimeSpan end) {
    return current < end;
  }

  protected sealed override System.TimeSpan RoundDown(System.TimeSpan start, System.TimeSpan end) {
    return RoundDown(timeSpan: start, diff: Difference);
  }

  protected sealed override System.TimeSpan RoundUp(System.TimeSpan start, System.TimeSpan end) {
    return RoundUp(dateTime: end, diff: Difference);
  }

  protected static System.TimeSpan Shift(System.TimeSpan span, DifferenceIn diff) {
    var res_ = span;

    System.TimeSpan shift_ = new();
    switch(diff) {
      case DifferenceIn.Year:
      case DifferenceIn.Month:
      case DifferenceIn.Day:
        shift_ = System.TimeSpan.FromDays(value: 1);
        break;
      case DifferenceIn.Hour:
        shift_ = System.TimeSpan.FromHours(value: 1);
        break;
      case DifferenceIn.Minute:
        shift_ = System.TimeSpan.FromMinutes(value: 1);
        break;
      case DifferenceIn.Second:
        shift_ = System.TimeSpan.FromSeconds(value: 1);
        break;
      case DifferenceIn.Millisecond:
        shift_ = System.TimeSpan.FromMilliseconds(value: 1);
        break;
    }

    res_ = res_.Add(ts: shift_);
    return res_;
  }

  protected sealed override System.TimeSpan RoundDown(System.TimeSpan timeSpan, DifferenceIn diff) {
    var res_ = timeSpan;

    if(timeSpan.Ticks < 0) {
      res_ = RoundUp(dateTime: timeSpan.Duration(), diff: diff).Negate();
    }
    else {
      switch(diff) {
        case DifferenceIn.Year:
        case DifferenceIn.Month:
        case DifferenceIn.Day:
          res_ = System.TimeSpan.FromDays(value: timeSpan.Days);
          break;
        case DifferenceIn.Hour:
          res_ = System.TimeSpan.FromDays(value: timeSpan.Days).
            Add(ts: System.TimeSpan.FromHours(value: timeSpan.Hours));
          break;
        case DifferenceIn.Minute:
          res_ = System.TimeSpan.FromDays(value: timeSpan.Days).
            Add(ts: System.TimeSpan.FromHours(value: timeSpan.Hours)).
            Add(ts: System.TimeSpan.FromMinutes(value: timeSpan.Minutes));
          break;
        case DifferenceIn.Second:
          res_ = System.TimeSpan.FromDays(value: timeSpan.Days).
            Add(ts: System.TimeSpan.FromHours(value: timeSpan.Hours)).
            Add(ts: System.TimeSpan.FromMinutes(value: timeSpan.Minutes)).
            Add(ts: System.TimeSpan.FromSeconds(value: timeSpan.Seconds));
          break;
        case DifferenceIn.Millisecond:
          res_ = timeSpan;
          break;
      }
    }

    return res_;
  }

  protected sealed override System.TimeSpan RoundUp(System.TimeSpan dateTime, DifferenceIn diff) {
    var res_ = RoundDown(timeSpan: dateTime, diff: diff);
    res_ = Shift(span: res_, diff: diff);

    return res_;
  }

  protected override List<System.TimeSpan> Trim(List<System.TimeSpan> ticks, Range<System.TimeSpan> range) {
    var startIndex_ = 0;
    for(var i_ = 0; i_ < ticks.Count - 1; i_++) {
      if(ticks[index: i_] <= range.Min && range.Min <= ticks[index: i_ + 1]) {
        startIndex_ = i_;
        break;
      }
    }

    var endIndex_ = ticks.Count - 1;
    for(var i_ = ticks.Count - 1; i_ >= 1; i_--) {
      if(ticks[index: i_] >= range.Max && range.Max > ticks[index: i_ - 1]) {
        endIndex_ = i_;
        break;
      }
    }

    List<System.TimeSpan> res_ = new(capacity: endIndex_ - startIndex_ + 1);
    for(var i_ = startIndex_; i_ <= endIndex_; i_++) {
      res_.Add(item: ticks[index: i_]);
    }

    return res_;
  }

  protected sealed override bool IsMinDate(System.TimeSpan dt) {
    return false;
  }
}

internal sealed class DayTimeSpanProvider : TimeSpanTicksProviderBase {
  protected override DifferenceIn GetDifferenceCore() {
    return DifferenceIn.Day;
  }

  protected override int[] GetTickCountsCore() {
    return new int[] { 20, 10, 5, 2, 1 };
  }

  protected override int GetSpecificValue(System.TimeSpan start, System.TimeSpan dt) {
    return (dt - start).Days;
  }

  protected override System.TimeSpan GetStart(System.TimeSpan start, int step) {
    var days_ = start.TotalDays;
    double newDays_ = (int)(days_ / step) * step;
    if(newDays_ > days_) {
      newDays_ -= step;
    }
    return System.TimeSpan.FromDays(value: newDays_);
    //return TimeSpan.FromDays(start.Days);
  }

  protected override System.TimeSpan AddStep(System.TimeSpan dt, int step) {
    return dt.Add(ts: System.TimeSpan.FromDays(value: step));
  }
}

internal sealed class HourTimeSpanProvider : TimeSpanTicksProviderBase {
  protected override DifferenceIn GetDifferenceCore() {
    return DifferenceIn.Hour;
  }

  protected override int[] GetTickCountsCore() {
    return new int[] { 24, 12, 6, 4, 3, 2, 1 };
  }

  protected override int GetSpecificValue(System.TimeSpan start, System.TimeSpan dt) {
    return (int)(dt - start).TotalHours;
  }

  protected override System.TimeSpan GetStart(System.TimeSpan start, int step) {
    var hours_ = start.TotalHours;
    double newHours_ = (int)(hours_ / step) * step;
    if(newHours_ > hours_) {
      newHours_ -= step;
    }
    return System.TimeSpan.FromHours(value: newHours_);
    //return TimeSpan.FromDays(start.Days);
  }

  protected override System.TimeSpan AddStep(System.TimeSpan dt, int step) {
    return dt.Add(ts: System.TimeSpan.FromHours(value: step));
  }
}

internal sealed class MinuteTimeSpanProvider : TimeSpanTicksProviderBase {
  protected override DifferenceIn GetDifferenceCore() {
    return DifferenceIn.Minute;
  }

  protected override int[] GetTickCountsCore() {
    return new int[] { 60, 30, 20, 15, 10, 5, 4, 3, 2 };
  }

  protected override int GetSpecificValue(System.TimeSpan start, System.TimeSpan dt) {
    return (int)(dt - start).TotalMinutes;
  }

  protected override System.TimeSpan GetStart(System.TimeSpan start, int step) {
    var minutes_ = start.TotalMinutes;
    double newMinutes_ = (int)(minutes_ / step) * step;
    if(newMinutes_ > minutes_) {
      newMinutes_ -= step;
    }

    return System.TimeSpan.FromMinutes(value: newMinutes_);
  }

  protected override System.TimeSpan AddStep(System.TimeSpan dt, int step) {
    return dt.Add(ts: System.TimeSpan.FromMinutes(value: step));
  }
}

internal sealed class SecondTimeSpanProvider : TimeSpanTicksProviderBase {
  protected override DifferenceIn GetDifferenceCore() {
    return DifferenceIn.Second;
  }

  protected override int[] GetTickCountsCore() {
    return new int[] { 60, 30, 20, 15, 10, 5, 4, 3, 2 };
  }

  protected override int GetSpecificValue(System.TimeSpan start, System.TimeSpan dt) {
    return (int)(dt - start).TotalSeconds;
  }

  protected override System.TimeSpan GetStart(System.TimeSpan start, int step) {
    var seconds_ = start.TotalSeconds;
    double newSeconds_ = (int)(seconds_ / step) * step;
    if(newSeconds_ > seconds_) {
      newSeconds_ -= step;
    }

    return System.TimeSpan.FromSeconds(value: newSeconds_);
    //return new TimeSpan(start.Days, start.Hours, start.Minutes, 0);
  }

  protected override System.TimeSpan AddStep(System.TimeSpan dt, int step) {
    return dt.Add(ts: System.TimeSpan.FromSeconds(value: step));
  }
}

internal sealed class MillisecondTimeSpanProvider : TimeSpanTicksProviderBase {
  protected override DifferenceIn GetDifferenceCore() {
    return DifferenceIn.Millisecond;
  }

  protected override int[] GetTickCountsCore() {
    return new int[] { 100, 50, 40, 25, 20, 10, 5, 4, 2 };
  }

  protected override int GetSpecificValue(System.TimeSpan start, System.TimeSpan dt) {
    return (int)(dt - start).TotalMilliseconds;
  }

  protected override System.TimeSpan GetStart(System.TimeSpan start, int step) {
    var millis_ = start.TotalMilliseconds;
    double newMillis_ = (int)(millis_ / step) * step;
    if(newMillis_ > millis_) {
      newMillis_ -= step;
    }

    return System.TimeSpan.FromMilliseconds(value: newMillis_);
    //return start;
  }

  protected override System.TimeSpan AddStep(System.TimeSpan dt, int step) {
    return dt.Add(ts: System.TimeSpan.FromMilliseconds(value: step));
  }
}
