using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class PointCollectionExtensions {
  public static DataRect GetBounds(this IEnumerable<Point> points) {
    return BoundsHelper.GetViewportBounds(viewportPoints: points);
  }

  public static IEnumerable<Point> Skip(this IList<Point> points, int skipCount) {
    for(var i_ = skipCount; i_ < points.Count; i_++) {
      yield return points[index: i_];
    }
  }
}
