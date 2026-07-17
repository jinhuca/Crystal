using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Transforms;

public static class DataDomains {
  public static DataRect XPositive { get; } = DataRect.FromPoints(x1: double.Epsilon, y1: double.MinValue / 2, x2: double.MaxValue, y2: double.MaxValue / 2);
  public static DataRect YPositive { get; } = DataRect.FromPoints(x1: double.MinValue / 2, y1: double.Epsilon, x2: double.MaxValue / 2, y2: double.MaxValue);
  public static DataRect XYPositive { get; } = DataRect.FromPoints(x1: double.Epsilon, y1: double.Epsilon, x2: double.MaxValue, y2: double.MaxValue);
}
