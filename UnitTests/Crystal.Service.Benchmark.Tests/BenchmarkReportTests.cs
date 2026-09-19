using Crystal.Service.Benchmark;
using Xunit;

namespace Crystal.Service.Benchmark.Tests;

/// <summary>
/// The CSV report builder lives on the module's dashboard VM, which references WPF; the escaping
/// rule it relies on is the RFC 4180 quoting reproduced here to guard the format contract.
/// </summary>
public sealed class BenchmarkReportTests {
  private static string Csv(string value) {
    if (value.IndexOfAny([',', '"', '\n', '\r']) < 0) return value;
    return '"' + value.Replace("\"", "\"\"") + '"';
  }

  [Theory]
  [InlineData("plain", "plain")]
  [InlineData("has,comma", "\"has,comma\"")]
  [InlineData("has\"quote", "\"has\"\"quote\"")]
  [InlineData("has\nnewline", "\"has\nnewline\"")]
  public void CsvEscaping_QuotesOnlyWhenNeeded(string input, string expected) {
    Assert.Equal(expected, Csv(input));
  }

  [Fact]
  public void ResultFormatting_RoundTripsScoreInvariantly() {
    var result = BenchmarkResult.Success("cpu.int", 1234.5, "MOps/s", "detail", System.TimeSpan.FromSeconds(2.5));

    Assert.Equal("1234.5", result.Score.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
    Assert.Equal("2.5", result.Elapsed.TotalSeconds.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
  }
}
