using System.Windows.Media;
using Crystal.Controls.MeterGauges;
using Crystal.Controls.MeterGauges.Themes;
using Xunit;

namespace Crystal.Controls.UnitTests.MeterGauges;

public class MeterGaugeTests {
  [Fact]
  public void Defaults_MatchDocumentedValues() => StaRunner.Run(() => {
    var gauge = new MeterGauge();

    Assert.Equal(0.0, gauge.Value);
    Assert.Equal(0.0, gauge.MinValue);
    Assert.Equal(100.0, gauge.MaxValue);
    Assert.Equal(60, gauge.TickCount);
    Assert.Equal(135.0, gauge.StartAngle);
    Assert.Equal(270.0, gauge.SweepAngle);
  });

  [Fact]
  public void ApplyTheme_UpdatesBrushes() => StaRunner.Run(() => {
    var gauge = new MeterGauge();
    var theme = GaugeThemes.FromAccent(Color.FromRgb(0x11, 0x22, 0x33));

    gauge.ApplyTheme(theme);

    Assert.Same(theme.ActiveBrush, gauge.ActiveBrush);
    Assert.Same(theme.InactiveBrush, gauge.InactiveBrush);
    Assert.Same(theme.GaugeBackground, gauge.GaugeBackground);
  });

  [Fact]
  public void ApplyTheme_Null_IsNoOp() => StaRunner.Run(() => {
    var gauge = new MeterGauge();
    Brush before = gauge.ActiveBrush;

    gauge.ApplyTheme(null!);

    Assert.Same(before, gauge.ActiveBrush);
  });

  [Fact]
  public void ApplyTheme_LeavesUnsetPropertiesUntouched() => StaRunner.Run(() => {
    var gauge = new MeterGauge();
    Brush originalBackground = gauge.GaugeBackground;
    var partialTheme = new GaugeTheme { ActiveBrush = Brushes.Red };

    gauge.ApplyTheme(partialTheme);

    Assert.Same(Brushes.Red, gauge.ActiveBrush);
    Assert.Same(originalBackground, gauge.GaugeBackground);
  });

  [Fact]
  public void Value_IsSettable() => StaRunner.Run(() => {
    var gauge = new MeterGauge { MinValue = 0, MaxValue = 3, Value = 1.5 };

    Assert.Equal(1.5, gauge.Value);
  });
}
