using Crystal.Controls.RangeBars.Controls;
using Xunit;

namespace Crystal.Controls.UnitTests.RangeBars;

public class RangeBarViewTests {
  [Fact]
  public void Defaults_MatchDocumentedValues() => StaRunner.Run(() => {
    var view = new RangeBarView();

    Assert.Equal(string.Empty, view.Title);
    Assert.Equal(string.Empty, view.Unit);
    Assert.Equal(0.0, view.Value);
    Assert.Equal("{0:0.00}", view.ValueFormat);
    Assert.Equal(0.0, view.MinValue);
    Assert.Equal(100.0, view.MaxValue);
  });

  [Fact]
  public void Bar_IsNullBeforeTemplateApplied() => StaRunner.Run(() => {
    var view = new RangeBarView();

    Assert.Null(view.Bar);
  });

  [Fact]
  public void SettingScaleProperties_BeforeTemplate_DoesNotThrow() => StaRunner.Run(() => {
    var view = new RangeBarView();

    // The DP changed-handlers guard on a null Bar; setting these before the template
    // has produced the inner bar must be safe.
    view.MinValue = 0;
    view.MaxValue = 3;
    view.Value = 1.5;

    Assert.Equal(0, view.MinValue);
    Assert.Equal(3, view.MaxValue);
    Assert.Equal(1.5, view.Value);
  });

  [Fact]
  public void TitleAndUnit_RoundTrip() => StaRunner.Run(() => {
    var view = new RangeBarView { Title = "Voltage", Unit = "V" };

    Assert.Equal("Voltage", view.Title);
    Assert.Equal("V", view.Unit);
  });
}
