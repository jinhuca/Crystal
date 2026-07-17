using Crystal.Plot2D.Transforms;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class BoundsHelper {
  /// <summary>
  ///   Computes bounding rectangle for sequence of points.
  /// </summary>
  /// <param name="points">
  ///   Points sequence.
  /// </param>
  /// <returns>
  ///   Minimal axis-aligned bounding rectangle.
  /// </returns>
  public static DataRect GetViewportBounds(IEnumerable<Point> viewportPoints) {
    var bounds = DataRect.Empty;
    var xMin = double.PositiveInfinity;
    var xMax = double.NegativeInfinity;
    var yMin = double.PositiveInfinity;
    var yMax = double.NegativeInfinity;

    foreach(var p in viewportPoints) {
      xMin = Math.Min(val1: xMin, val2: p.X);
      xMax = Math.Max(val1: xMax, val2: p.X);

      yMin = Math.Min(val1: yMin, val2: p.Y);
      yMax = Math.Max(val1: yMax, val2: p.Y);
    }

    // were some points in collection
    if(!double.IsInfinity(d: xMin)) {
      bounds = DataRect.Create(xMin: xMin, yMin: yMin, xMax: xMax, yMax: yMax);
    }

    return bounds;
  }

  public static DataRect GetViewportBounds(IEnumerable<Point> dataPoints, DataTransform transform) {
    return GetViewportBounds(viewportPoints: dataPoints.DataToViewport(transform: transform));
  }
}
