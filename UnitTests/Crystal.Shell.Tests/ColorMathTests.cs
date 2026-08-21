using Crystal.Shell.Views;
using System.Windows.Media;
using Xunit;

namespace Crystal.Shell.Tests;

public class ColorMathTests {
  [Theory]
  [InlineData(0xFF, 0x00, 0x00, 0, 1, 1)]     // red
  [InlineData(0x00, 0xFF, 0x00, 120, 1, 1)]   // green
  [InlineData(0x00, 0x00, 0xFF, 240, 1, 1)]   // blue
  [InlineData(0xFF, 0xFF, 0x00, 60, 1, 1)]    // yellow
  [InlineData(0x00, 0xFF, 0xFF, 180, 1, 1)]   // cyan
  [InlineData(0xFF, 0x00, 0xFF, 300, 1, 1)]   // magenta
  public void RgbToHsv_maps_primary_and_secondary_hues(
      byte r, byte g, byte b, double h, double s, double v) {
    var (hue, sat, val) = ColorMath.RgbToHsv(Color.FromRgb(r, g, b));

    Assert.Equal(h, hue, precision: 3);
    Assert.Equal(s, sat, precision: 3);
    Assert.Equal(v, val, precision: 3);
  }

  [Fact]
  public void RgbToHsv_black_is_zero_saturation_and_value() {
    var (h, s, v) = ColorMath.RgbToHsv(Colors.Black);

    Assert.Equal(0, h);
    Assert.Equal(0, s);
    Assert.Equal(0, v);
  }

  [Fact]
  public void RgbToHsv_white_is_zero_saturation_full_value() {
    var (_, s, v) = ColorMath.RgbToHsv(Colors.White);

    Assert.Equal(0, s);
    Assert.Equal(1, v);
  }

  [Fact]
  public void RgbToHsv_gray_has_zero_saturation() {
    var (_, s, v) = ColorMath.RgbToHsv(Color.FromRgb(0x80, 0x80, 0x80));

    Assert.Equal(0, s);
    Assert.Equal(0x80 / 255.0, v, precision: 3);
  }

  [Theory]
  [InlineData(0, 1, 1, 0xFF, 0x00, 0x00)]
  [InlineData(120, 1, 1, 0x00, 0xFF, 0x00)]
  [InlineData(240, 1, 1, 0x00, 0x00, 0xFF)]
  [InlineData(0, 0, 0, 0x00, 0x00, 0x00)]     // black
  [InlineData(0, 0, 1, 0xFF, 0xFF, 0xFF)]     // white
  public void HsvToRgb_maps_known_points(
      double h, double s, double v, byte r, byte g, byte b) {
    Assert.Equal(Color.FromRgb(r, g, b), ColorMath.HsvToRgb(h, s, v));
  }

  [Theory]
  [InlineData(0x00, 0x00, 0x00)]
  [InlineData(0xFF, 0xFF, 0xFF)]
  [InlineData(0x12, 0x34, 0x56)]
  [InlineData(0x3E, 0x9B, 0xE8)]  // the app's Sky accent
  [InlineData(0xE8, 0x2A, 0x7A)]  // Rose
  [InlineData(0x7F, 0x40, 0x20)]
  [InlineData(0x01, 0xFE, 0x80)]
  [InlineData(0x80, 0x80, 0x80)]
  public void RoundTrip_preserves_the_color(byte r, byte g, byte b) {
    var original = Color.FromRgb(r, g, b);

    var (h, s, v) = ColorMath.RgbToHsv(original);
    var result = ColorMath.HsvToRgb(h, s, v);

    Assert.Equal(original, result);
  }
}
