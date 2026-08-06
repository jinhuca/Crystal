using System.Windows.Media;
using Crystal.Controls.MeterGauges;
using Xunit;

namespace Crystal.Controls.Tests.MeterGauges;

public class MeterGaugeRenderTests {
  private static readonly Color Active = Color.FromRgb(0x3B, 0xD1, 0x5A);
  private static readonly Color Inactive = Color.FromRgb(0x3A, 0x3A, 0x40);

  [Fact]
  public void FullValue_LightsMoreTicksThanZeroValue() => StaRunner.Run(() => {
    int atZero = ActivePixels(value: 0, min: 0, max: 100);
    int atFull = ActivePixels(value: 100, min: 0, max: 100);

    Assert.True(atFull > atZero,
        $"expected more active pixels at full scale, got zero={atZero} full={atFull}");
  });

  [Fact]
  public void ActivePixelCount_IncreasesMonotonicallyWithValue() => StaRunner.Run(() => {
    int low = ActivePixels(value: 25, min: 0, max: 100);
    int mid = ActivePixels(value: 50, min: 0, max: 100);
    int high = ActivePixels(value: 75, min: 0, max: 100);

    Assert.True(low < mid && mid < high,
        $"expected monotonic growth, got low={low} mid={mid} high={high}");
  });

  [Fact]
  public void ZeroValue_StillLightsFirstTick() => StaRunner.Run(() => {
    // litCount is inclusive of the boundary tick, so even value == min lights one tick.
    Assert.True(ActivePixels(value: 0, min: 0, max: 100) > 0);
  });

  [Fact]
  public void GaugeAlwaysDrawsInactiveTrack() => StaRunner.Run(() => {
    var gauge = new MeterGauge {
      MinValue = 0, MaxValue = 100, Value = 50,
      ActiveBrush = new SolidColorBrush(Active),
      InactiveBrush = new SolidColorBrush(Inactive),
      GaugeBackground = Brushes.Black
    };
    var renderer = new PixelRenderer(gauge, 200, 200);

    Assert.True(renderer.CountColor(Inactive) > 0, "unlit ticks should be visible at 50%");
  });

  [Fact]
  public void ValueClampedAboveMax_DoesNotExceedFullScale() => StaRunner.Run(() => {
    int atFull = ActivePixels(value: 100, min: 0, max: 100);
    int overMax = ActivePixels(value: 500, min: 0, max: 100);

    // Over-range values are clamped, so they light no more than a full-scale reading
    // (allow a small tolerance for anti-aliasing differences).
    Assert.True(overMax <= atFull + 15,
        $"clamped over-range should match full scale, got full={atFull} over={overMax}");
  });

  private static int ActivePixels(double value, double min, double max) {
    var gauge = new MeterGauge {
      MinValue = min, MaxValue = max, Value = value,
      ActiveBrush = new SolidColorBrush(Active),
      InactiveBrush = new SolidColorBrush(Inactive),
      GaugeBackground = Brushes.Black
    };
    return new PixelRenderer(gauge, 200, 200).CountColor(Active);
  }
}
