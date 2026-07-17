using System.Windows.Media;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class BrushHelper {
  /// <summary>
  ///   Creates a SolidColorBrush with random hue of its color.
  /// </summary>
  /// <returns>
  ///   A SolidColorBrush with random hue of its color.
  /// </returns>
  public static SolidColorBrush CreateBrushWithRandomHue() {
    return new SolidColorBrush { Color = ColorHelper.CreateColorWithRandomHue() };
  }

  /// <summary>
  ///   Makes SolidColorBrush transparent.
  /// </summary>
  /// <param name="brush">
  ///   The brush.
  /// </param>
  /// <param name="alpha">
  ///   The alpha, [0..255]
  /// </param>
  /// <returns></returns>
  public static SolidColorBrush MakeTransparent(this SolidColorBrush brush, int alpha) {
    var color = brush.Color;
    color.A = (byte)alpha;
    return new SolidColorBrush(color: color);
  }

  /// <summary>
  ///   Makes SolidColorBrush transparent.
  /// </summary>
  /// <param name="brush">
  ///   The brush.
  /// </param>
  /// <param name="alpha">
  ///   The alpha, [0.0 .. 1.0].
  /// </param>
  /// <returns></returns>
  public static SolidColorBrush MakeTransparent(this SolidColorBrush brush, double opacity) {
    return MakeTransparent(brush: brush, alpha: (int)(opacity * 255));
  }

  public static SolidColorBrush ChangeLightness(this SolidColorBrush brush, double lightnessFactor) {
    var color = brush.Color;
    var hsbColor = HsbColor.FromArgbColor(color: color);
    hsbColor.Brightness *= lightnessFactor;

    if(hsbColor.Brightness > 1.0) {
      hsbColor.Brightness = 1.0;
    }

    var result = new SolidColorBrush(color: hsbColor.ToArgbColor());
    return result;
  }

  public static SolidColorBrush ChangeSaturation(this SolidColorBrush brush, double saturationFactor) {
    var color = brush.Color;
    var hsbColor = HsbColor.FromArgbColor(color: color);
    hsbColor.Saturation *= saturationFactor;

    if(hsbColor.Saturation > 1.0) {
      hsbColor.Saturation = 1.0;
    }

    var result = new SolidColorBrush(color: hsbColor.ToArgbColor());
    return result;
  }
}