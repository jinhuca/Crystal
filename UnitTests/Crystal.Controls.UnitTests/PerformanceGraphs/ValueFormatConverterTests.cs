using System.Globalization;
using Crystal.Controls.PerformanceGraphs.Controls;
using Xunit;

namespace Crystal.Controls.UnitTests.PerformanceGraphs;

public class ValueFormatConverterTests {
  private static readonly ValueFormatConverter Converter = new();

  [Fact]
  public void Convert_AppliesPercentFormat() {
    object result = Converter.Convert(new object[] { 100.0, "{0}%" }, typeof(string), null!, CultureInfo.InvariantCulture);

    Assert.Equal("100%", result);
  }

  [Fact]
  public void Convert_AppliesVoltFormat() {
    object result = Converter.Convert(new object[] { 3.0, "{0}V" }, typeof(string), null!, CultureInfo.InvariantCulture);

    Assert.Equal("3V", result);
  }

  [Fact]
  public void Convert_MissingFormat_DefaultsToPlainValue() {
    object result = Converter.Convert(new object[] { 60, null! }, typeof(string), null!, CultureInfo.InvariantCulture);

    Assert.Equal("60", result);
  }

  [Fact]
  public void Convert_TooFewValues_ReturnsEmptyString() {
    object result = Converter.Convert(new object[] { 1 }, typeof(string), null!, CultureInfo.InvariantCulture);

    Assert.Equal(string.Empty, result);
  }

  [Fact]
  public void Convert_NullValues_ReturnsEmptyString() {
    object result = Converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);

    Assert.Equal(string.Empty, result);
  }

  [Fact]
  public void Convert_InvalidFormat_FallsBackToValueToString() {
    object result = Converter.Convert(new object[] { 7, "{0" }, typeof(string), null!, CultureInfo.InvariantCulture);

    Assert.Equal("7", result);
  }

  [Fact]
  public void ConvertBack_Throws() {
    Assert.Throws<NotSupportedException>(() =>
        Converter.ConvertBack("x", new[] { typeof(double) }, null!, CultureInfo.InvariantCulture));
  }
}
