using System.Windows.Media;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class ColorExtensions {
  private static Color MakeTransparent(this Color color, int alpha) {
    color.A = (byte)alpha;
    return color;
  }

  public static Color MakeTransparent(this Color color, double opacity) {
    return MakeTransparent(color: color, alpha: (int)(255 * opacity));
  }
}
