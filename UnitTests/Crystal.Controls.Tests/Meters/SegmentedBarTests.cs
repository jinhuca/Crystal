using Crystal.Controls.Meters;
using System.ComponentModel;
using System.Windows.Media;
using Xunit;

namespace Crystal.Controls.Tests.Meters;

public class SegmentedBarTests {
  private static readonly Color Fill = Colors.Red;

  private static SegmentedBar NewBar(bool segmented, double value) => new() {
    Minimum = 0,
    Maximum = 100,
    Value = value,
    Fill = new SolidColorBrush(Fill),
    Segmented = segmented,
    // No track/border so the pixel count reflects the fill alone.
    TrackBrush = null,
    Stroke = null,
  };

  [Fact]
  public void Solid_bar_fills_proportionally_to_value() => StaRunner.Run(() => {
    var full = new PixelRenderer(NewBar(segmented: false, value: 100), 100, 6);
    var half = new PixelRenderer(NewBar(segmented: false, value: 50), 100, 6);
    var empty = new PixelRenderer(NewBar(segmented: false, value: 0), 100, 6);

    Assert.Equal(0, empty.CountColor(Fill));
    // ~half the plot is filled at 50%, all of it at 100%.
    Assert.True(half.CountColor(Fill) < full.CountColor(Fill));
    Assert.True(full.CountColor(Fill) > half.CountColor(Fill) * 1.5);
  });

  [Fact]
  public void Segmented_bar_leaves_gaps_so_it_lights_fewer_pixels_than_solid() => StaRunner.Run(() => {
    int solid = new PixelRenderer(NewBar(segmented: false, value: 60), 100, 6).CountColor(Fill);
    int segmented = new PixelRenderer(NewBar(segmented: true, value: 60), 100, 6).CountColor(Fill);

    Assert.True(segmented > 0);
    Assert.True(segmented < solid);
  });

  [Fact]
  public void Value_is_clamped_to_the_range() => StaRunner.Run(() => {
    int over = new PixelRenderer(NewBar(segmented: false, value: 500), 100, 6).CountColor(Fill);
    int full = new PixelRenderer(NewBar(segmented: false, value: 100), 100, 6).CountColor(Fill);

    Assert.Equal(full, over);
  });

  [Fact]
  public void CoreBarAppearance_raises_change_notifications() {
    var appearance = CoreBarAppearance.Current;
    var changed = new List<string?>();
    PropertyChangedEventHandler handler = (_, e) => changed.Add(e.PropertyName);
    appearance.PropertyChanged += handler;
    try {
      bool origSeg = appearance.Segmented, origMono = appearance.Monochrome;
      appearance.Segmented = !origSeg;
      appearance.Monochrome = !origMono;

      Assert.Contains(nameof(CoreBarAppearance.Segmented), changed);
      Assert.Contains(nameof(CoreBarAppearance.Monochrome), changed);

      // Restore so the shared singleton doesn't leak state into other tests.
      appearance.Segmented = origSeg;
      appearance.Monochrome = origMono;
    } finally {
      appearance.PropertyChanged -= handler;
    }
  }
}
