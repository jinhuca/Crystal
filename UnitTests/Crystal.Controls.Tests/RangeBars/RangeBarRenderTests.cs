using Crystal.Controls.RangeBars;
using System.Windows.Media;
using Xunit;

namespace Crystal.Controls.Tests.RangeBars;

public class RangeBarRenderTests {
  private static readonly Color Fill = Color.FromRgb(0x20, 0xFF, 0x20);
  private static readonly Color Track = Color.FromRgb(0x40, 0x40, 0x48);

  [Fact]
  public void FullValue_FillsMoreThanZeroValue() => StaRunner.Run(() => {
    int atZero = FillPixels(value: 0, min: 0, max: 100);
    int atFull = FillPixels(value: 100, min: 0, max: 100);

    Assert.True(atFull > atZero,
        $"expected more fill pixels at full scale, got zero={atZero} full={atFull}");
  });

  [Fact]
  public void ZeroValue_DrawsNoFill() => StaRunner.Run(() => {
    Assert.Equal(0, FillPixels(value: 0, min: 0, max: 100));
  });

  [Fact]
  public void FillPixelCount_IncreasesMonotonicallyWithValue() => StaRunner.Run(() => {
    int low = FillPixels(value: 25, min: 0, max: 100);
    int mid = FillPixels(value: 50, min: 0, max: 100);
    int high = FillPixels(value: 75, min: 0, max: 100);

    Assert.True(low < mid && mid < high,
        $"expected monotonic growth, got low={low} mid={mid} high={high}");
  });

  [Fact]
  public void HalfValue_FillsRoughlyHalfTheTrack() => StaRunner.Run(() => {
    int half = FillPixels(value: 50, min: 0, max: 100);
    int full = FillPixels(value: 100, min: 0, max: 100);

    double ratio = (double)half / full;
    Assert.InRange(ratio, 0.4, 0.6);
  });

  [Fact]
  public void ValueAboveMax_IsClampedToFullScale() => StaRunner.Run(() => {
    int full = FillPixels(value: 100, min: 0, max: 100);
    int over = FillPixels(value: 500, min: 0, max: 100);

    Assert.True(over <= full + 15,
        $"clamped over-range should match full scale, got full={full} over={over}");
  });

  [Fact]
  public void ValueBelowMin_DrawsNoFill() => StaRunner.Run(() => {
    Assert.Equal(0, FillPixels(value: -50, min: 0, max: 100));
  });

  [Fact]
  public void UnfilledTrack_IsVisibleAtPartialValue() => StaRunner.Run(() => {
    var bar = NewBar(value: 50, min: 0, max: 100);
    var renderer = new PixelRenderer(bar, 300, 40);

    Assert.True(renderer.CountColor(Track) > 0, "unfilled track should show at 50%");
  });

  [Fact]
  public void SizeSmallerThanBorderThickness_DoesNotThrow() => StaRunner.Run(() => {
    // Default BorderThickness is 3; a 2x2 control makes width/height minus the border negative,
    // which the Rect constructor rejects. Rendering must degrade gracefully, not crash.
    var bar = NewBar(value: 50, min: 0, max: 100);
    _ = new PixelRenderer(bar, 2, 2);
  });

  private static int FillPixels(double value, double min, double max) =>
      new PixelRenderer(NewBar(value, min, max), 300, 40).CountColor(Fill);

  private static RangeBar NewBar(double value, double min, double max) => new() {
    MinValue = min, MaxValue = max, Value = value,
    FillBrush = new SolidColorBrush(Fill),
    TrackBrush = new SolidColorBrush(Track),
    BarBackground = Brushes.Black,
    BorderBrush = Brushes.Black
  };
}
