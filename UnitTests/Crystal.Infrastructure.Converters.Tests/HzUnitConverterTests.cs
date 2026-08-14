using Crystal.Infrastructure.Converters;
using System.Globalization;
using Xunit;

namespace Crystal.Infrastructure.Converters.Tests;

public class HzUnitConverterTests : IDisposable {
  private readonly CultureInfo _original = CultureInfo.CurrentCulture;

  public HzUnitConverterTests() =>
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

  public void Dispose() => CultureInfo.CurrentCulture = _original;

  [Theory]
  [InlineData(0.0, "0.00 MHz")]
  [InlineData(999.99, "999.99 MHz")]
  public void ConvertMHzToReadableUnit_BelowOneThousand_StaysInMHz(double mhz, string expected) =>
    Assert.Equal(expected, HzUnitConverter.ConvertMHzToReadableUnit(mhz));

  [Theory]
  [InlineData(1000.0, "1.00 GHz")]
  [InlineData(3600.0, "3.60 GHz")]
  [InlineData(4500.0, "4.50 GHz")]
  public void ConvertMHzToReadableUnit_AtOrAboveOneThousand_ConvertsToGHz(double mhz, string expected) =>
    Assert.Equal(expected, HzUnitConverter.ConvertMHzToReadableUnit(mhz));

  [Fact]
  public void ConvertMHzToReadableUnit_Negative_Throws() =>
    Assert.Throws<ArgumentOutOfRangeException>(() => HzUnitConverter.ConvertMHzToReadableUnit(-1.0));
}
