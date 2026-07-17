using System;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class ColorHelper {
  private static readonly Random random = new();

  /// <summary>
  ///   Creates color from HSB color space with random hue and saturation and brightness equal to 1.
  /// </summary>
  /// <returns></returns>
  public static Color CreateColorWithRandomHue() {
    var hue_ = random.NextDouble() * 360;
    HsbColor hsbColor_ = new(hue: hue_, saturation: 1, brightness: 1);
    return hsbColor_.ToArgbColor();
  }

  public static Color[] CreateRandomColors(int colorNum) {
    var startHue_ = random.NextDouble() * 360;

    var res_ = new Color[colorNum];
    var hueStep_ = 360.0 / colorNum;
    for(var i_ = 0; i_ < res_.Length; i_++) {
      var hue_ = startHue_ + i_ * hueStep_;
      res_[i_] = new HsbColor(hue: hue_, saturation: 1, brightness: 1).ToArgbColor();
    }

    return res_;
  }

  /// <summary>
  ///   Creates color with fully random hue and slightly random saturation and brightness.
  /// </summary>
  /// <returns></returns>
  public static Color CreateRandomHsbColor() {
    var h_ = random.NextDouble() * 360;
    var s_ = random.NextDouble() * 0.5 + 0.5;
    var b_ = random.NextDouble() * 0.25 + 0.75;
    return new HsbColor(hue: h_, saturation: s_, brightness: b_).ToArgbColor();
  }

  /// <summary>
  ///   Creates color with random hue.
  /// </summary>
  /// <param name="saturation">
  ///   The saturation, [0..1].
  /// </param>
  /// <param name="brightness">
  ///   The brightness, [0..1].
  /// </param>
  /// <returns></returns>
  public static Color CreateColorWithRandomHue(double saturation, double brightness) {
    var h_ = random.NextDouble() * 360;
    return new HsbColor(hue: h_, saturation: saturation, brightness: brightness).ToArgbColor();
  }

  /// <summary>
  ///   Creates brush with random hue.
  /// </summary>
  /// <param name="saturation">
  ///   The saturation, [0..1].
  /// </param>
  /// <param name="brightness">
  ///   The brightness, [0..1].
  /// </param>
  /// <returns></returns>
  public static Brush CreateBrushWithRandomHue(double saturation, double brightness) {
    var color_ = CreateColorWithRandomHue(saturation: saturation, brightness: brightness);
    return new SolidColorBrush(color: color_);
  }

  /// <summary>
  ///   Gets the random color (this property is created to use it from Xaml).
  /// </summary>
  /// <value>
  ///   The random color.
  /// </value>
  public static Color RandomColor => CreateRandomHsbColor();

  /// <summary>
  ///   Gets the random brush.
  /// </summary>
  /// <value>
  ///   The random brush.
  /// </value>
  public static SolidColorBrush RandomBrush => new(color: CreateRandomHsbColor());

  public static int ToArgb(this Color color) => color.A << 24 | color.R << 16 | color.G << 8 | color.B;
}
