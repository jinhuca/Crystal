using OSModule.ViewModels;
using Xunit;

namespace OSModule.Tests;

public class OsUptimeFormatTests {
  [Fact]
  public void Formats_days_when_at_least_one_day() {
    var uptime = new TimeSpan(days: 3, hours: 21, minutes: 22, seconds: 12);

    Assert.Equal("3d 21:22:12", OsViewModel.FormatUptime(uptime));
  }

  [Fact]
  public void Omits_days_below_one_day() {
    var uptime = new TimeSpan(hours: 5, minutes: 7, seconds: 9);

    Assert.Equal("05:07:09", OsViewModel.FormatUptime(uptime));
  }

  [Fact]
  public void Pads_hours_minutes_and_seconds() {
    var uptime = new TimeSpan(hours: 1, minutes: 2, seconds: 3);

    Assert.Equal("01:02:03", OsViewModel.FormatUptime(uptime));
  }

  [Fact]
  public void Negative_uptime_clamps_to_zero() {
    // A clock skew could yield now < boot; the label should never render a negative duration.
    Assert.Equal("00:00:00", OsViewModel.FormatUptime(TimeSpan.FromSeconds(-5)));
  }
}
