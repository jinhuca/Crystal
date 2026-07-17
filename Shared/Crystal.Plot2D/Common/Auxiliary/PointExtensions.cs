using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class PointExtensions {
  public static Vector ToVector(this Point pt) => new(x: pt.X, y: pt.Y);

  public static bool IsFinite(this Point pt) => pt.X.IsFinite() && pt.Y.IsFinite();
}
