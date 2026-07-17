using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class RangeExtensions {
  public static double GetLength(this Range<Point> range) {
    var p1 = range.Min;
    var p2 = range.Max;
    return (p1 - p2).Length;
  }

  public static double GetLength(this Range<double> range) => range.Max - range.Min;
}
