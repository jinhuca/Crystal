using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Axes.TimeSpan;

internal sealed class MinorTimeSpanTicksProvider : MinorTimeProviderBase<System.TimeSpan> {
  public MinorTimeSpanTicksProvider(ITicksProvider<System.TimeSpan> owner) : base(provider: owner) { }

  protected override bool IsInside(System.TimeSpan value, Range<System.TimeSpan> range) {
    return range.Min < value && value < range.Max;
  }
}
