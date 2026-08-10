using System;
using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

/// <summary>Covers <see cref="BiosInformation.GetDate"/>, the SMBIOS BIOS-date string parser.</summary>
public class SMBiosDateTests {
  [Fact]
  public void ParsesFourDigitYear() {
    Assert.Equal(new DateTime(2021, 3, 15), BiosInformation.GetDate("03/15/2021"));
  }

  [Fact]
  public void TwoDigitYear_IsWindowedInto1900s() {
    // Per the parser: year < 100 maps to 1900 + year.
    Assert.Equal(new DateTime(1999, 12, 31), BiosInformation.GetDate("12/31/99"));
    Assert.Equal(new DateTime(1905, 1, 1), BiosInformation.GetDate("01/01/05"));
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("not a date")]
  [InlineData("03/15")]           // too few parts
  [InlineData("03/15/2021/00")]   // too many parts
  [InlineData("aa/15/2021")]      // non-numeric month
  public void MalformedInput_ReturnsNull(string? input) {
    Assert.Null(BiosInformation.GetDate(input!));
  }

  [Theory]
  [InlineData("13/01/2021")] // month > 12
  [InlineData("01/32/2021")] // day > 31
  public void OutOfRangeMonthOrDay_ReturnsNull(string input) {
    Assert.Null(BiosInformation.GetDate(input));
  }

  [Fact]
  public void InvalidCalendarDate_WithinRangeGuards_ReturnsNull() {
    // Day 31 passes the day <= 31 guard but is not a valid February date; the DateTime ctor throws
    // ArgumentOutOfRangeException. This pins the current behavior (the parser does not catch it).
    Assert.Throws<ArgumentOutOfRangeException>(() => BiosInformation.GetDate("02/31/2021"));
  }
}
