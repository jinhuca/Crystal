using System.Windows;
using System.Windows.Media.Media3D;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class VectorExtensions {
  public static Point ToPoint(this Vector vector) {
    return new Point(x: vector.X, y: vector.Y);
  }

  public static Vector DecreaseLength(this Vector vector, double width, double height) {
    vector.X /= width;
    vector.Y /= height;

    return vector;
  }

  public static Vector Perpendicular(this Vector v) {
    var result = Vector3D.CrossProduct(vector1: new Vector3D(x: v.X, y: v.Y, z: 0), vector2: new Vector3D(x: 0, y: 0, z: 1));
    return new Vector(x: result.X, y: result.Y);
  }
}
