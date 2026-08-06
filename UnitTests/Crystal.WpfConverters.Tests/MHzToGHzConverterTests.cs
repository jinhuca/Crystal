using System.Globalization;
using Crystal.WpfConverters;
using Xunit;

namespace Crystal.WpfConverters.Tests;

public class MHzToGHzConverterTests {
  private static readonly MHzToGHzConverter Converter = new();

  private static object Convert(object value) =>
    Converter.Convert(value, typeof(double), null!, CultureInfo.InvariantCulture);

  [Theory]
  [InlineData(3600.0, 3.6)]
  [InlineData(1000.0, 1.0)]
  [InlineData(0.0, 0.0)]
  [InlineData(4500.0, 4.5)]
  public void Convert_DividesByThousand(double mhz, double expectedGhz) =>
    Assert.Equal(expectedGhz, (double)Convert(mhz));

  [Fact]
  public void Convert_RoundsToTwoDecimals() =>
    // 3546 / 1000 = 3.546 → third decimal is 6, unambiguously rounds up to 3.55.
    Assert.Equal(3.55, (double)Convert(3546.0));

  [Fact]
  public void Convert_AcceptsIntegerInput() =>
    // ConvertToDouble handles boxed int, which is how XAML often supplies the bound value.
    Assert.Equal(2.4, (double)Convert(2400));

  [Fact]
  public void ConvertBack_Throws() =>
    Assert.Throws<NotImplementedException>(() =>
      Converter.ConvertBack(1.0, typeof(double), null!, CultureInfo.InvariantCulture));
}
