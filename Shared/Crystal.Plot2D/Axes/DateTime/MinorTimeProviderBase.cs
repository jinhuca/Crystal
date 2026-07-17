using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crystal.Plot2D.Axes;

internal abstract class MinorTimeProviderBase<T> : ITicksProvider<T> {
  public event EventHandler Changed;
  protected void RaiseChanged() {
    if(Changed != null) {
      Changed(sender: this, e: EventArgs.Empty);
    }
  }

  private readonly ITicksProvider<T> provider;

  protected MinorTimeProviderBase(ITicksProvider<T> provider) {
    this.provider = provider;
  }

  private T[] majorTicks = { };
  internal void SetTicks(T[] ticks) {
    majorTicks = ticks;
  }

  private const double ticksSize = 0.5;

  public ITicksInfo<T> GetTicks(Range<T> range, int ticksCount) {
    if(majorTicks.Length == 0) {
      return new TicksInfo<T>();
    }

    ticksCount /= majorTicks.Length;
    if(ticksCount == 0) {
      ticksCount = 2;
    }

    var ticks = majorTicks.GetPairs().Select(selector: r => Clip(ticks: provider.GetTicks(range: r, ticksCount: ticksCount), range: r)).
      SelectMany(selector: t => t.Ticks).ToArray();

    var res = new TicksInfo<T> {
      Ticks = ticks,
      TickSizes = ArrayExtensions.CreateArray(length: ticks.Length, defaultValue: ticksSize)
    };
    return res;
  }

  private ITicksInfo<T> Clip(ITicksInfo<T> ticks, Range<T> range) {
    var newTicks = new List<T>(capacity: ticks.Ticks.Length);
    var newSizes = new List<double>(capacity: ticks.TickSizes.Length);

    for(var i = 0; i < ticks.Ticks.Length; i++) {
      var tick = ticks.Ticks[i];
      if(IsInside(value: tick, range: range)) {
        newTicks.Add(item: tick);
        newSizes.Add(item: ticks.TickSizes[i]);
      }
    }

    return new TicksInfo<T> {
      Ticks = newTicks.ToArray(),
      TickSizes = newSizes.ToArray(),
      Info = ticks.Info
    };
  }

  protected abstract bool IsInside(T value, Range<T> range);

  public int DecreaseTickCount(int ticksCount) {
    if(majorTicks.Length > 0) {
      ticksCount /= majorTicks.Length;
    }

    var minorTicksCount = provider.DecreaseTickCount(ticksCount: ticksCount);

    if(majorTicks.Length > 0) {
      minorTicksCount *= majorTicks.Length;
    }

    return minorTicksCount;
  }

  public int IncreaseTickCount(int ticksCount) {
    if(majorTicks.Length > 0) {
      ticksCount /= majorTicks.Length;
    }

    var minorTicksCount = provider.IncreaseTickCount(ticksCount: ticksCount);

    if(majorTicks.Length > 0) {
      minorTicksCount *= majorTicks.Length;
    }

    return minorTicksCount;
  }

  public ITicksProvider<T> MinorProvider => null;

  public ITicksProvider<T> MajorProvider => null;
}
