using System.Globalization;
using System.Windows.Media;
using CpuModule.Views;
using Xunit;

namespace CpuModule.Tests;

public class LoadToBrushConverterTests {
  private static readonly LoadToBrushConverter Converter = new();

  private static SolidColorBrush Convert(object value) =>
      (SolidColorBrush)Converter.Convert(value, typeof(Brush), null!, CultureInfo.InvariantCulture);

  [Theory]
  [InlineData(0.0)]
  [InlineData(59.9)]
  public void Low_load_maps_to_green(double load) =>
      Assert.Equal(Color.FromRgb(0x3B, 0xD1, 0x5A), Convert(load).Color);

  [Theory]
  [InlineData(60.0)]
  [InlineData(84.9)]
  public void Mid_load_maps_to_amber(double load) =>
      Assert.Equal(Color.FromRgb(0xE8, 0xB3, 0x2A), Convert(load).Color);

  [Theory]
  [InlineData(85.0)]
  [InlineData(100.0)]
  public void High_load_maps_to_red(double load) =>
      Assert.Equal(Color.FromRgb(0xE8, 0x4A, 0x3B), Convert(load).Color);

  [Fact]
  public void Non_double_value_is_treated_as_zero_load() =>
      Assert.Equal(Color.FromRgb(0x3B, 0xD1, 0x5A), Convert("not a number").Color);

  [Fact]
  public void Returns_a_shared_frozen_brush_per_band() {
    var a = Convert(10.0);
    var b = Convert(20.0);

    // Both low-band reads must hand back the same frozen instance, not a fresh allocation.
    Assert.True(a.IsFrozen);
    Assert.Same(a, b);
  }

  [Fact]
  public void ConvertBack_is_not_supported() =>
      Assert.Throws<NotSupportedException>(
          () => Converter.ConvertBack(Brushes.Red, typeof(double), null!, CultureInfo.InvariantCulture));
}
