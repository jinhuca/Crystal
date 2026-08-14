using Crystal.Controls.RangeBars;
using Crystal.Controls.RangeBars.Themes;
using System.Windows.Media;
using Xunit;

namespace Crystal.Controls.Tests.RangeBars;

public class RangeBarTests {
  [Fact]
  public void Defaults_MatchDocumentedValues() => StaRunner.Run(() => {
    var bar = new RangeBar();

    Assert.Equal(0.0, bar.Value);
    Assert.Equal(0.0, bar.MinValue);
    Assert.Equal(100.0, bar.MaxValue);
    Assert.Equal(3.0, bar.BorderThickness);
  });

  [Fact]
  public void Properties_RoundTrip() => StaRunner.Run(() => {
    var bar = new RangeBar {
      Value = 1.5, MinValue = 0, MaxValue = 3, BorderThickness = 5
    };

    Assert.Equal(1.5, bar.Value);
    Assert.Equal(0, bar.MinValue);
    Assert.Equal(3, bar.MaxValue);
    Assert.Equal(5, bar.BorderThickness);
  });

  [Fact]
  public void ApplyTheme_SetsBrushes() => StaRunner.Run(() => {
    var bar = new RangeBar();

    bar.ApplyTheme(RangeBarThemes.Rose());

    Assert.Equal(Color.FromRgb(0xE8, 0x2A, 0x7A), ((SolidColorBrush)bar.FillBrush).Color);
  });

  [Fact]
  public void ApplyTheme_Null_DoesNotThrow() => StaRunner.Run(() => {
    var bar = new RangeBar();

    bar.ApplyTheme(null!);
  });
}
