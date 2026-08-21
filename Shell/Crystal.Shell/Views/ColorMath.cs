using System.Windows.Media;

namespace Crystal.Shell.Views;

/// <summary>
/// RGB↔HSV conversions backing <see cref="ColorPickerDialog"/>. Hue is in degrees (0–360),
/// saturation and value in 0–1. Kept as a standalone helper so the maths can be unit-tested without
/// instantiating the WPF window.
/// </summary>
internal static class ColorMath {
  public static (double h, double s, double v) RgbToHsv(Color c) {
    double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
    double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
    double d = max - min;
    double h = 0;
    if (d != 0) {
      if (max == r) h = 60 * (((g - b) / d) % 6);
      else if (max == g) h = 60 * (((b - r) / d) + 2);
      else h = 60 * (((r - g) / d) + 4);
    }
    if (h < 0) h += 360;
    double s = max == 0 ? 0 : d / max;
    return (h, s, max);
  }

  public static Color HsvToRgb(double h, double s, double v) {
    double c = v * s;
    double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
    double m = v - c;
    double r = 0, g = 0, b = 0;
    if (h < 60) { r = c; g = x; }
    else if (h < 120) { r = x; g = c; }
    else if (h < 180) { g = c; b = x; }
    else if (h < 240) { g = x; b = c; }
    else if (h < 300) { r = x; b = c; }
    else { r = c; b = x; }
    return Color.FromRgb(
        (byte)Math.Round((r + m) * 255),
        (byte)Math.Round((g + m) * 255),
        (byte)Math.Round((b + m) * 255));
  }
}
