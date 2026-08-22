using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.Meters;

/// <summary>
/// A single-value horizontal meter bar. Renders <see cref="Value"/> (clamped to
/// <see cref="Minimum"/>..<see cref="Maximum"/>) either as a solid fill or, 
/// when <see cref="Segmented"/> is set, as a row of discrete LED-meter blocks — matching 
/// the look of the PerformanceGraph SegmentedBar kind but for a horizontal, one-shot reading 
/// rather than a time series. Used by the CPU core strip in place of a plain <c>ProgressBar</c> 
/// so the load/clock/temp bars can adopt the meter aesthetic and follow the shared 
/// <see cref="CoreBarAppearance"/> selection.
/// </summary>
public sealed class SegmentedBar : FrameworkElement {
  /// <summary>
  /// LED-block geometry (device-independent px): each lit block plus the gap to the next. 
  /// </summary>
  private const double SegmentWidth = 4;

  /// <summary>
  /// Gap between LED blocks (device-independent px): each lit block plus the gap to the next.
  /// </summary>
  private const double SegmentGap = 2;

  /// <summary>
  /// The minimum value of the meter. The <see cref="Value"/> is clamped to the range 
  /// <see cref="Minimum"/>..<see cref="Maximum"/>.
  /// </summary>
  public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
    nameof(Minimum),
    typeof(double),
    typeof(SegmentedBar),
    new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// The maximum value of the meter. The <see cref="Value"/> is clamped to the range
  /// </summary>
  public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
    nameof(Maximum),
    typeof(double),
    typeof(SegmentedBar),
    new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// The current value of the meter. The <see cref="Value"/> is clamped to the range
  /// </summary>
  public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
    nameof(Value),
    typeof(double),
    typeof(SegmentedBar),
    new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// The brush used to fill the meter's value. When <see cref="Segmented"/> is true, 
  /// this is the color of the lit LED blocks.
  /// </summary>
  public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
    nameof(Fill),
    typeof(Brush),
    typeof(SegmentedBar),
    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// The brush used to fill the meter's background track. When <see cref="Segmented"/> is true,
  /// </summary>
  public static readonly DependencyProperty TrackBrushProperty = DependencyProperty.Register(
    nameof(TrackBrush),
    typeof(Brush),
    typeof(SegmentedBar),
    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// The brush used to draw a 1px border around the meter. 
  /// The border is inset half a pixel so it stays inside the element's bounds instead of straddling them.
  /// </summary>
  public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
    nameof(Stroke),
    typeof(Brush),
    typeof(SegmentedBar),
    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// True to render the meter as a discrete LED-meter (segmented); false for a solid fill.
  /// </summary>
  public static readonly DependencyProperty SegmentedProperty = DependencyProperty.Register(
    nameof(Segmented),
    typeof(bool),
    typeof(SegmentedBar),
    new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// The minimum value of the meter. The <see cref="Value"/> is clamped to the range
  /// </summary>
  public double Minimum {
    get => (double)GetValue(MinimumProperty);
    set => SetValue(MinimumProperty, value);
  }

  /// <summary>
  /// The maximum value of the meter. The <see cref="Value"/> is clamped to the range
  /// </summary>
  public double Maximum {
    get => (double)GetValue(MaximumProperty);
    set => SetValue(MaximumProperty, value);
  }
  public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
  public Brush? Fill { get => (Brush?)GetValue(FillProperty); set => SetValue(FillProperty, value); }
  public Brush? TrackBrush { get => (Brush?)GetValue(TrackBrushProperty); set => SetValue(TrackBrushProperty, value); }
  public Brush? Stroke { get => (Brush?)GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
  public bool Segmented { get => (bool)GetValue(SegmentedProperty); set => SetValue(SegmentedProperty, value); }

  protected override void OnRender(DrawingContext dc) {
    double w = ActualWidth, h = ActualHeight;
    if (w <= 0 || h <= 0) return;

    if (TrackBrush is { } track) dc.DrawRectangle(track, null, new Rect(0, 0, w, h));

    double range = Maximum - Minimum;
    if (range > 0 && Fill is { } fill) {
      double t = (Value - Minimum) / range;
      t = t < 0 ? 0 : (t > 1 ? 1 : t);
      double filled = t * w;

      if (filled > 0) {
        if (Segmented) {
          // LED blocks left→right; the block straddling the fill edge is clipped so the meter reads
          // as "this much lit" rather than snapping to the next whole block.
          for (double x = 0; x < filled; x += SegmentWidth + SegmentGap) {
            double blockRight = x + SegmentWidth;
            double drawWidth = (blockRight > filled ? filled : blockRight) - x;
            if (drawWidth > 0) dc.DrawRectangle(fill, null, new Rect(x, 0, drawWidth, h));
          }
        }
        else {
          dc.DrawRectangle(fill, null, new Rect(0, 0, filled, h));
        }
      }
    }

    // 1px border inset half a pixel so it stays inside the element's bounds instead of straddling them.
    if (Stroke is { } stroke) {
      var pen = new Pen(stroke, 1);
      pen.Freeze();
      dc.DrawRectangle(null, pen, new Rect(0.5, 0.5, w - 1, h - 1));
    }
  }
}
