using Crystal.Controls.RangeBars.Controls;
using System.Globalization;
using Xunit;

namespace Crystal.Controls.Tests.RangeBars;

public class ValueFormatConverterTests {
  private static readonly BarValueFormatConverter Converter = new();

  [Fact]
  public void Convert_FormatsValueWithSuppliedFormat() {
    object result = Converter.Convert(new object[] { 0.82, "{0:0.00}" }, typeof(string), null!, CultureInfo.InvariantCulture);

    Assert.Equal("0.82", result);
  }

  [Fact]
  public void Convert_MissingFormat_DefaultsToPlainValue() {
    object result = Converter.Convert(new object[] { 42, null! }, typeof(string), null!, CultureInfo.InvariantCulture);

    Assert.Equal("42", result);
  }

  [Fact]
  public void Convert_TooFewValues_ReturnsEmptyString() {
    object result = Converter.Convert(new object[] { 42 }, typeof(string), null!, CultureInfo.InvariantCulture);

    Assert.Equal(string.Empty, result);
  }

  [Fact]
  public void Convert_InvalidFormat_FallsBackToValueToString() {
    object result = Converter.Convert(new object[] { 42, "{0" }, typeof(string), null!, CultureInfo.InvariantCulture);

    Assert.Equal("42", result);
  }

  [Fact]
  public void ConvertBack_Throws() {
    Assert.Throws<NotSupportedException>(() =>
        Converter.ConvertBack("x", new[] { typeof(double) }, null!, CultureInfo.InvariantCulture));
  }
}
