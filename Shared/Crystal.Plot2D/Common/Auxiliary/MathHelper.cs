using System;
using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class MathHelper {
  public static long Clamp(long value, long min, long max) => Math.Max(val1: min, val2: Math.Min(val1: value, val2: max));

  public static double Clamp(double value, double min, double max) => Math.Max(val1: min, val2: Math.Min(val1: value, val2: max));

  /// <summary>
  ///   Clamps specified value to [0,1].
  /// </summary>
  /// <param name="d">
  ///   Value to clamp.
  /// </param>
  /// <returns>
  ///   Value in range [0,1].
  /// </returns>
  public static double Clamp(double value) => Math.Max(val1: 0, val2: Math.Min(val1: value, val2: 1));

  public static int Clamp(int value, int min, int max) => Math.Max(val1: min, val2: Math.Min(val1: value, val2: max));

  public static Rect CreateRectByPoints(double xMin, double yMin, double xMax, double yMax) => new(point1: new Point(x: xMin, y: yMin), point2: new Point(x: xMax, y: yMax));

  public static double Interpolate(double start, double end, double ratio) => start * (1 - ratio) + end * ratio;

  public static double RadiansToDegrees(this double radians) => radians * 180 / Math.PI;

  public static double DegreesToRadians(this double degrees) => degrees / 180 * Math.PI;

  /// <summary>
  ///   Converts vector into angle.
  /// </summary>
  /// <param name="vector">
  ///   The vector.
  /// </param>
  /// <returns>
  ///   Angle in degrees.
  /// </returns>
  public static double ToAngle(this Vector vector) => Math.Atan2(y: -vector.Y, x: vector.X).RadiansToDegrees();

  public static Point ToPoint(this Vector v) => new(x: v.X, y: v.Y);

  public static bool IsNaN(this double d) => double.IsNaN(d: d);

  public static bool IsNotNaN(this double d) => !double.IsNaN(d: d);

  public static bool IsFinite(this double d) => !double.IsNaN(d: d) && !double.IsInfinity(d: d);

  public static bool IsInfinite(this double d) => double.IsInfinity(d: d);

  public static bool AreClose(double d1, double d2, double diffRatio) => Math.Abs(value: d1 / d2 - 1) < diffRatio;
}