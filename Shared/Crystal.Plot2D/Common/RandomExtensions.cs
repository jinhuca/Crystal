using System;
using System.Windows;

namespace Crystal.Plot2D.Common;

internal static class RandomExtensions {
  public static Point NextPoint(this Random rnd) => new(x: rnd.NextDouble(), y: rnd.NextDouble());

  public static Point NextPoint(this Random rnd, double xMin, double xMax, double yMin, double yMax) {
    var x = rnd.NextDouble() * (xMax - xMin) + xMin;
    var y = rnd.NextDouble() * (yMax - yMin) + yMin;
    return new Point(x: x, y: y);
  }

  public static Vector NextVector(this Random rnd) => new(x: rnd.NextDouble(), y: rnd.NextDouble());
}
