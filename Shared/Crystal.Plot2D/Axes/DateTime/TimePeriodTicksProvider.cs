using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Axes;

internal abstract class TimePeriodTicksProvider<T> : ITicksProvider<T> {
  public event EventHandler Changed;

  protected void RaiseChanged() {
    Changed?.Invoke(sender: this, e: EventArgs.Empty);
  }

  protected abstract T RoundUp(T time, DifferenceIn diff);
  protected abstract T RoundDown(T time, DifferenceIn diff);

  private bool _differenceInitialized;
  private DifferenceIn difference;

  protected DifferenceIn Difference {
    get {
      if(_differenceInitialized) return difference;
      difference = GetDifferenceCore();
      _differenceInitialized = true;

      return difference;
    }
  }

  protected abstract DifferenceIn GetDifferenceCore();

  private int[] tickCounts;
  protected int[] TickCounts => tickCounts ??= GetTickCountsCore();

  protected abstract int[] GetTickCountsCore();

  public int DecreaseTickCount(int ticksCount) {
    if(ticksCount > TickCounts[0]) {
      return TickCounts[0];
    }

    foreach(var t_ in TickCounts) {
      if(ticksCount > t_) {
        return t_;
      }
    }

    return TickCounts.Last();
  }

  public int IncreaseTickCount(int ticksCount) {
    if(ticksCount >= TickCounts[0]) {
      return TickCounts[0];
    }

    for(var i_ = TickCounts.Length - 1; i_ >= 0; i_--) {
      if(ticksCount < TickCounts[i_]) {
        return TickCounts[i_];
      }
    }

    return TickCounts.Last();
  }

  protected abstract int GetSpecificValue(T start, T dt);
  protected abstract T GetStart(T start, int step);
  protected abstract bool IsMinDate(T dt);
  protected abstract T AddStep(T dt, int step);

  public ITicksInfo<T> GetTicks(Range<T> range, int ticksCount) {
    var start_ = range.Min;
    var end_ = range.Max;
    var diff_ = Difference;
    start_ = RoundDown(start: start_, end: end_);
    end_ = RoundUp(start: start_, end: end_);

    var bounds_ = RoundingHelper.CreateRoundedRange(
      min: GetSpecificValue(start: start_, dt: start_),
      max: GetSpecificValue(start: start_, dt: end_));

    var delta_ = (int)(bounds_.Max - bounds_.Min);
    if(delta_ == 0) {
      return new TicksInfo<T> { Ticks = new[] { start_ } };
    }

    var step_ = delta_ / ticksCount;

    if(step_ == 0) {
      step_ = 1;
    }

    var tick_ = GetStart(start: start_, step: step_);
    var isMinDateTime_ = IsMinDate(dt: tick_) && step_ != 1;
    if(isMinDateTime_) {
      step_--;
    }

    List<T> ticks_ = new();
    var finishTick_ = AddStep(dt: range.Max, step: step_);
    while(Continue(current: tick_, end: finishTick_)) {
      ticks_.Add(item: tick_);
      tick_ = AddStep(dt: tick_, step: step_);
      if(isMinDateTime_) {
        isMinDateTime_ = false;
        step_++;
      }
    }

    ticks_ = Trim(ticks: ticks_, range: range);

    TicksInfo<T> res_ = new() { Ticks = ticks_.ToArray(), Info = diff_ };
    return res_;
  }

  protected abstract bool Continue(T current, T end);

  protected abstract T RoundUp(T start, T end);

  protected abstract T RoundDown(T start, T end);

  protected abstract List<T> Trim(List<T> ticks, Range<T> range);

  public ITicksProvider<T> MinorProvider => throw new NotSupportedException();

  public ITicksProvider<T> MajorProvider => throw new NotSupportedException();
}

internal abstract class DatePeriodTicksProvider : TimePeriodTicksProvider<System.DateTime> {
  protected sealed override bool Continue(System.DateTime current, System.DateTime end) {
    return current < end;
  }

  protected sealed override List<System.DateTime> Trim(List<System.DateTime> ticks, Range<System.DateTime> range) {
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

    List<System.DateTime> res_ = new(capacity: endIndex_ - startIndex_ + 1);
    for(var i_ = startIndex_; i_ <= endIndex_; i_++) {
      res_.Add(item: ticks[index: i_]);
    }

    return res_;
  }

  protected sealed override System.DateTime RoundUp(System.DateTime start, System.DateTime end) {
    var isPositive_ = (end - start).Ticks > 0;
    return isPositive_ ? SafelyRoundUp(dt: end) : RoundDown(time: end, diff: Difference);
  }

  private System.DateTime SafelyRoundUp(System.DateTime dt) {
    if(AddStep(dt: dt, step: 1) == System.DateTime.MaxValue) {
      return System.DateTime.MaxValue;
    }

    return RoundUp(dateTime: dt, diff: Difference);
  }

  protected sealed override System.DateTime RoundDown(System.DateTime start, System.DateTime end) {
    var isPositive_ = (end - start).Ticks > 0;
    return isPositive_ ? RoundDown(time: start, diff: Difference) : SafelyRoundUp(dt: start);
  }

  protected sealed override System.DateTime RoundDown(System.DateTime time, DifferenceIn diff) {
    var res_ = time;

    switch(diff) {
      case DifferenceIn.Year:
        res_ = new System.DateTime(year: time.Year, month: 1, day: 1);
        break;
      case DifferenceIn.Month:
        res_ = new System.DateTime(year: time.Year, month: time.Month, day: 1);
        break;
      case DifferenceIn.Day:
        res_ = time.Date;
        break;
      case DifferenceIn.Hour:
        res_ = time.Date.AddHours(value: time.Hour);
        break;
      case DifferenceIn.Minute:
        res_ = time.Date.AddHours(value: time.Hour).AddMinutes(value: time.Minute);
        break;
      case DifferenceIn.Second:
        res_ = time.Date.AddHours(value: time.Hour).AddMinutes(value: time.Minute).AddSeconds(value: time.Second);
        break;
      case DifferenceIn.Millisecond:
        res_ = time.Date.AddHours(value: time.Hour).AddMinutes(value: time.Minute).AddSeconds(value: time.Second).AddMilliseconds(value: time.Millisecond);
        break;
    }

    DebugVerify.Is(condition: res_ <= time);

    return res_;
  }

  protected override System.DateTime RoundUp(System.DateTime dateTime, DifferenceIn diff) {
    var res_ = RoundDown(time: dateTime, diff: diff);

    switch(diff) {
      case DifferenceIn.Year:
        res_ = res_.AddYears(value: 1);
        break;
      case DifferenceIn.Month:
        res_ = res_.AddMonths(months: 1);
        break;
      case DifferenceIn.Day:
        res_ = res_.AddDays(value: 1);
        break;
      case DifferenceIn.Hour:
        res_ = res_.AddHours(value: 1);
        break;
      case DifferenceIn.Minute:
        res_ = res_.AddMinutes(value: 1);
        break;
      case DifferenceIn.Second:
        res_ = res_.AddSeconds(value: 1);
        break;
      case DifferenceIn.Millisecond:
        res_ = res_.AddMilliseconds(value: 1);
        break;
    }

    return res_;
  }
}
