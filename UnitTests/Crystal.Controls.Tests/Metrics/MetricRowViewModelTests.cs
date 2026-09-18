using Crystal.Controls.Metrics;
using Xunit;

namespace Crystal.Controls.Tests.Metrics;

public class MetricRowViewModelTests {
  [Fact]
  public void Update_TracksValueAndSelfExtremes() {
    var row = new MetricRowViewModel("Load");

    row.Update(10);
    row.Update(30);
    row.Update(20);

    Assert.Equal(20, row.Value);
    Assert.Equal(10, row.Min);
    Assert.Equal(30, row.Max);
    Assert.Equal(20, row.Avg);
  }

  [Theory]
  [InlineData(double.NaN)]
  [InlineData(double.PositiveInfinity)]
  [InlineData(double.NegativeInfinity)]
  public void Update_DropsNonFiniteValue_LeavingStatsIntact(double bad) {
    // A single non-finite reading would otherwise poison the row for the rest of the session:
    // _sum += NaN pins Avg at NaN forever and Math.Min/Max stick Min/Max at NaN (the "NaN%" bug).
    var row = new MetricRowViewModel("Load");
    row.Update(40);
    row.Update(60);

    row.Update(bad);

    Assert.Equal(60, row.Value);
    Assert.Equal(40, row.Min);
    Assert.Equal(60, row.Max);
    Assert.Equal(50, row.Avg);
  }

  [Fact]
  public void Update_IgnoresNonFiniteSuppliedExtremes_FallingBackToSelfTracking() {
    var row = new MetricRowViewModel("Load");

    row.Update(25, double.NaN, double.PositiveInfinity);

    Assert.Equal(25, row.Value);
    Assert.Equal(25, row.Min);
    Assert.Equal(25, row.Max);
  }
}
