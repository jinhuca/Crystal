using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Axes.TimeSpan;

public abstract class TimeTicksProviderBase<T> : ITicksProvider<T> {
  public event EventHandler Changed;

  private void RaiseChanged() {
    if(Changed != null) {
      Changed(sender: this, e: EventArgs.Empty);
    }
  }

  private static readonly Dictionary<DifferenceIn, ITicksProvider<T>> providers = new();

  protected static Dictionary<DifferenceIn, ITicksProvider<T>> Providers => providers;

  private static readonly Dictionary<DifferenceIn, ITicksProvider<T>> minorProviders = new();

  protected static Dictionary<DifferenceIn, ITicksProvider<T>> MinorProviders => minorProviders;

  protected abstract System.TimeSpan GetDifference(T start, T end);

  #region ITicksProvider<T> Members

  private IDateTimeTicksStrategy strategy = new DefaultDateTimeTicksStrategy();

  public IDateTimeTicksStrategy Strategy {
    get => strategy;
    set {
      if(strategy != value) {
        strategy = value;
        RaiseChanged();
      }
    }
  }

  private ITicksInfo<T> result;
  private DifferenceIn diff;

  public ITicksInfo<T> GetTicks(Range<T> range, int ticksCount) {
    Verify.IsTrue(condition: ticksCount > 0);

    var start_ = range.Min;
    var end_ = range.Max;
    var length_ = GetDifference(start: start_, end: end_);

    diff = strategy.GetDifference(span: length_);

    TicksInfo<T> result_ = new() { Info = diff };
    if(providers.TryGetValue(key: diff, value: out var provider_)) {
      var innerResult_ = provider_.GetTicks(range: range, ticksCount: ticksCount);
      var ticks_ = ModifyTicksGuard(ticks: innerResult_.Ticks);

      result_.Ticks = ticks_;
      result = result_;
      return result_;
    }

    throw new InvalidOperationException(message: Strings.Exceptions.UnsupportedRangeInAxis);
  }

  private T[] ModifyTicksGuard(T[] ticks) {
    var result_ = ModifyTicks(ticks: ticks);
    if(result_ == null) {
      throw new ArgumentNullException(paramName: nameof(ticks));
    }

    return result_;
  }

  private T[] ModifyTicks(T[] ticks) {
    return ticks;
  }

  /// <summary>
  /// Decreases the tick count.
  /// </summary>
  /// <param name="tickCount">The tick count.</param>
  /// <returns></returns>
  public int DecreaseTickCount(int ticksCount) {
    if(providers.TryGetValue(diff, out var provider_)) {
      return provider_.DecreaseTickCount(ticksCount: ticksCount);
    }

    var res_ = ticksCount / 2;
    if(res_ < 2) {
      res_ = 2;
    }

    return res_;
  }

  /// <summary>
  /// Increases the tick count.
  /// </summary>
  /// <param name="ticksCount">The tick count.</param>
  /// <returns></returns>
  public int IncreaseTickCount(int ticksCount) {
    DebugVerify.Is(condition: ticksCount < 2000);

    if(providers.TryGetValue(diff, out var provider_)) {
      return provider_.IncreaseTickCount(ticksCount: ticksCount);
    }

    return ticksCount * 2;
  }

  public ITicksProvider<T> MinorProvider {
    get {
      if(strategy.TryGetLowerDiff(diff: diff, lowerDiff: out var smallerDiff_) && minorProviders.TryGetValue(smallerDiff_, out var provider_)) {
        var minorProvider_ = (MinorTimeProviderBase<T>)provider_;
        minorProvider_.SetTicks(ticks: result.Ticks);
        return minorProvider_;
      }

      return null;
      // todo What to do if this already is the smallest provider?
    }
  }

  public ITicksProvider<T> MajorProvider =>
    strategy.TryGetBiggerDiff(diff: diff, biggerDiff: out var biggerDiff_) ? providers[key: biggerDiff_] : null;
  // todo What to do if this already is the biggest provider?

  #endregion
}
