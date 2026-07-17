using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class TriangleMath {
  public static bool TriangleContains(Point a, Point b, Point c, Point m) {
    var a0 = a.X - c.X;
    var a1 = b.X - c.X;
    var a2 = a.Y - c.Y;
    var a3 = b.Y - c.Y;

    if(AreClose(x: a0 * a3, y: a1 * a2)) {
      // determinant is too close to zero => apexes are on one line
      var ab = a - b;
      var ac = a - c;
      var bc = b - c;
      var ax = a - m;
      var bx = b - m;
      var res = AreClose(x: ab.X * ax.Y, y: ab.Y * ax.X) && !AreClose(x: ab.LengthSquared, y: 0) ||
                AreClose(x: ac.X * ax.Y, y: ac.Y * ax.X) && !AreClose(x: ac.LengthSquared, y: 0) ||
                AreClose(x: bc.X * bx.Y, y: bc.Y * bx.X) && !AreClose(x: bc.LengthSquared, y: 0);
      return res;
    }

    var b1 = m.X - c.X;
    var b2 = m.Y - c.Y;

    // alpha, beta and gamma - are baricentric coordinates of v 
    // in triangle with apexes a, b and c
    var beta = (b2 / a2 * a0 - b1) / (a3 / a2 * a0 - a1);
    var alpha = (b1 - a1 * beta) / a0;
    var gamma = 1 - beta - alpha;
    return alpha >= 0 && beta >= 0 && gamma >= 0;
  }

  private const double eps = 0.00001;
  private static bool AreClose(double x, double y) {
    return Math.Abs(value: x - y) < eps;
  }

  public static Vector3D GetBaricentricCoordinates(Point a, Point b, Point c, Point m) {
    var Sac = GetSquare(a: a, b: c, c: m);
    var Sbc = GetSquare(a: b, b: c, c: m);
    var Sab = GetSquare(a: a, b: b, c: m);

    var sum = (Sab + Sac + Sbc) / 3;

    return new Vector3D(x: Sbc / sum, y: Sac / sum, z: Sab / sum);
  }

  public static double GetSquare(Point a, Point b, Point c) {
    var ab = (a - b).Length;
    var ac = (a - c).Length;
    var bc = (b - c).Length;

    var p = 0.5 * (ab + ac + bc); // half of perimeter
    return Math.Sqrt(d: p * (p - ab) * (p - ac) * (p - bc));
  }
}
