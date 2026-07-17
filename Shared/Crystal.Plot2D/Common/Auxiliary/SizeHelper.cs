using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class SizeHelper {
  public static Size CreateInfiniteSize() {
    return new Size(width: double.PositiveInfinity, height: double.PositiveInfinity);
  }
}
