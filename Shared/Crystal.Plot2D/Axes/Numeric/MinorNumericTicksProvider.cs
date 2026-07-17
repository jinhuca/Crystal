using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crystal.Plot2D.Axes.Numeric;

public sealed class MinorNumericTicksProvider : ITicksProvider<double> {
  private readonly ITicksProvider<double> parent;
  private Range<double>[] ranges;
  internal void SetRanges(IEnumerable<Range<double>> ranges) {
    this.ranges = ranges.ToArray();
  }

  private double[] coeffs;

  public double[] Coeffs {
    get => coeffs;
    set {
      ArgumentNullException.ThrowIfNull(value);

      coeffs = value;
      Changed.Raise(sender: this);
    }
  }

  internal MinorNumericTicksProvider(ITicksProvider<double> parent) {
    this.parent = parent;
    Coeffs = new[] { 0.3, 0.3, 0.3, 0.3, 0.6, 0.3, 0.3, 0.3, 0.3 };
  }

  #region ITicksProvider<double> Members

  public event EventHandler Changed;

  public ITicksInfo<double> GetTicks(Range<double> range, int ticksCount) {
    if(Coeffs.Length == 0) {
      return new TicksInfo<double>();
    }

    var minorTicks = ranges.Select(selector: r => CreateTicks(range: r)).SelectMany(selector: m => m);
    var res = new TicksInfo<double> {
      TickSizes = minorTicks.Select(selector: m => m.Value).ToArray(),
      Ticks = minorTicks.Select(selector: m => m.Tick).ToArray()
    };

    return res;
  }

  public MinorTickInfo<double>[] CreateTicks(Range<double> range) {
    var step = (range.Max - range.Min) / (Coeffs.Length + 1);

    var res = new MinorTickInfo<double>[Coeffs.Length];
    for(var i = 0; i < Coeffs.Length; i++) {
      res[i] = new MinorTickInfo<double>(value: Coeffs[i], tick: range.Min + step * (i + 1));
    }
    return res;
  }

  public int DecreaseTickCount(int ticksCount) {
    return ticksCount;
  }

  public int IncreaseTickCount(int ticksCount) {
    return ticksCount;
  }

  public ITicksProvider<double> MinorProvider => null;

  public ITicksProvider<double> MajorProvider => parent;

  #endregion
}
