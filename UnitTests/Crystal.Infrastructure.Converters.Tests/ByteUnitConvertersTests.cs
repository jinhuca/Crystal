using System.Globalization;
using Crystal.Infrastructure.Converters;
using Xunit;

namespace Crystal.Infrastructure.Converters.Tests;

// Formatting/parsing here goes through double.ToString("F2") / double.Parse, which are
// culture-sensitive. Pin the invariant culture so a machine set to, e.g., de-DE (comma decimal
// separator) doesn't flip "1.50 KB" to "1,50 KB" and break the asserts.
public class ByteUnitConvertersTests : IDisposable {
  private readonly CultureInfo _original = CultureInfo.CurrentCulture;

  public ByteUnitConvertersTests() =>
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

  public void Dispose() => CultureInfo.CurrentCulture = _original;

  [Theory]
  [InlineData(0UL, "0 B")]
  [InlineData(512UL, "512 B")]
  [InlineData(1023UL, "1023 B")]
  public void ConvertBytesToReadableUnit_BelowOneKilobyte_ReportsRawBytes(ulong bytes, string expected) =>
    Assert.Equal(expected, ByteUnitConverters.ConvertBytesToReadableUnit(bytes));

  [Theory]
  [InlineData(1024UL, "1.00 KB")]
  [InlineData(1536UL, "1.50 KB")]
  [InlineData(1048576UL, "1.00 MB")]
  [InlineData(1073741824UL, "1.00 GB")]
  [InlineData(1099511627776UL, "1.00 TB")]
  public void ConvertBytesToReadableUnit_AtEachThreshold_ScalesToExpectedUnit(ulong bytes, string expected) =>
    Assert.Equal(expected, ByteUnitConverters.ConvertBytesToReadableUnit(bytes));

  [Fact]
  public void ConvertBytesToReadableUnit_BeyondTerabytes_CapsAtTb() =>
    // Units table stops at TB, so a petabyte-scale value stays in TB rather than rolling over.
    Assert.Equal("1024.00 TB", ByteUnitConverters.ConvertBytesToReadableUnit(1125899906842624UL));

  [Theory]
  [InlineData("512", 512L)]
  [InlineData("1 KB", 1024L)]
  [InlineData("1.5 KB", 1536L)]
  [InlineData("2 MB", 2097152L)]
  [InlineData("3 GB", 3221225472L)]
  [InlineData("1 TB", 1099511627776L)]
  [InlineData("1 PB", 1125899906842624L)]
  public void ConvertReadableUnitToBytes_MapsUnitToPowerOf1024(string input, long expected) =>
    Assert.Equal(expected, ByteUnitConverters.ConvertReadableUnitToBytes(input));

  [Fact]
  public void ConvertReadableUnitToBytes_IsCaseInsensitiveOnUnit() =>
    Assert.Equal(1024L, ByteUnitConverters.ConvertReadableUnitToBytes("1 kb"));

  [Fact]
  public void ConvertReadableUnitToBytes_UnknownUnit_TreatedAsBytes() =>
    // Unrecognized unit falls through the switch to power 0 (multiplier 1).
    Assert.Equal(5L, ByteUnitConverters.ConvertReadableUnitToBytes("5 XB"));

  [Fact]
  public void ConvertReadableUnitToBytes_TrimsSurroundingWhitespace() =>
    Assert.Equal(1024L, ByteUnitConverters.ConvertReadableUnitToBytes("  1 KB  "));

  [Fact]
  public void RoundTrip_KilobyteValue_Preserved() {
    string formatted = ByteUnitConverters.ConvertBytesToReadableUnit(2048UL);
    Assert.Equal(2048L, ByteUnitConverters.ConvertReadableUnitToBytes(formatted));
  }
}
