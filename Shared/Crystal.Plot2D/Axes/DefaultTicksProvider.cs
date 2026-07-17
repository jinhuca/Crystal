using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Axes;

internal static class DefaultTicksProvider {
  internal const int DefaultTicksCount = 10;

  internal static ITicksInfo<T> GetTicks<T>(this ITicksProvider<T> provider, Range<T> range) {
    return provider.GetTicks(range: range, ticksCount: DefaultTicksCount);
  }
}
