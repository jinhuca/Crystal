using Crystal.Controls.RangeBars.Renders;
using Crystal.Controls.RangeBars.Styles;
using Crystal.Controls.RangeBars.Themes;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars;

/// <summary>
/// A horizontal range bar: an outlined track filled from the left edge up to the point
/// representing <see cref="Value"/> on the <see cref="MinValue"/>..<see cref="MaxValue"/> scale.
/// The filled portion uses <see cref="FillBrush"/>, the remainder <see cref="TrackBrush"/>.
/// <para>
/// This is the low-level drawing primitive (analogous to <c>MeterGauge</c> and
/// <c>PerformanceGraph</c>); the labeled title/unit/value chrome lives in
/// <see cref="Controls.RangeBarView"/>.
/// </para>
/// </summary>
public class RangeBar : FrameworkElement {
  /// <summary>Identifies the <see cref="Value"/> dependency property.</summary>
  public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value), typeof(double), typeof(RangeBar),
          new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="MinValue"/> dependency property.</summary>
  public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(RangeBar),
          new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="MaxValue"/> dependency property.</summary>
  public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(RangeBar),
          new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="FillBrush"/> dependency property.</summary>
  public static readonly DependencyProperty FillBrushProperty =
      DependencyProperty.Register(nameof(FillBrush), typeof(Brush), typeof(RangeBar),
          new FrameworkPropertyMetadata(
              new SolidColorBrush(Color.FromRgb(0x3B, 0xD1, 0x5A)),
              FrameworkPropertyMetadataOptions.AffectsRender, OnFillBrushChanged));

  /// <summary>Identifies the <see cref="TrackBrush"/> dependency property.</summary>
  public static readonly DependencyProperty TrackBrushProperty =
      DependencyProperty.Register(nameof(TrackBrush), typeof(Brush), typeof(RangeBar),
          new FrameworkPropertyMetadata(Brushes.Transparent,
              FrameworkPropertyMetadataOptions.AffectsRender, OnTrackBrushChanged));

  /// <summary>Identifies the <see cref="BarBackground"/> dependency property.</summary>
  public static readonly DependencyProperty BarBackgroundProperty =
      DependencyProperty.Register(nameof(BarBackground), typeof(Brush), typeof(RangeBar),
          new FrameworkPropertyMetadata(Brushes.Black,
              FrameworkPropertyMetadataOptions.AffectsRender, OnBarBackgroundChanged));

  /// <summary>Identifies the <see cref="BorderBrush"/> dependency property.</summary>
  public static readonly DependencyProperty BorderBrushProperty =
      DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(RangeBar),
          new FrameworkPropertyMetadata(Brushes.Black,
              FrameworkPropertyMetadataOptions.AffectsRender, OnBorderBrushChanged));

  /// <summary>Identifies the <see cref="BorderThickness"/> dependency property.</summary>
  public static readonly DependencyProperty BorderThicknessProperty =
      DependencyProperty.Register(nameof(BorderThickness), typeof(double), typeof(RangeBar),
          new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsRender, OnBorderThicknessChanged));

  private readonly BackgroundRenderer _backgroundRender = new();
  private readonly FillRenderer _fillRender = new();
  private readonly BorderRenderer _borderRender = new();
  private readonly RangeBarStyle _style = new();

  public RangeBar() {
    SnapsToDevicePixels = true;
    UseLayoutRounding = true;
  }

  /// <summary>Current reading; clamped to <see cref="MinValue"/>..<see cref="MaxValue"/> when drawn.</summary>
  public double Value {
    get => (double)GetValue(ValueProperty);
    set => SetValue(ValueProperty, value);
  }

  /// <summary>Reading that maps to the left edge of the bar.</summary>
  public double MinValue {
    get => (double)GetValue(MinValueProperty);
    set => SetValue(MinValueProperty, value);
  }

  /// <summary>Reading that maps to the right edge of the bar.</summary>
  public double MaxValue {
    get => (double)GetValue(MaxValueProperty);
    set => SetValue(MaxValueProperty, value);
  }

  /// <summary>Brush for the filled portion (from the left up to <see cref="Value"/>).</summary>
  public Brush FillBrush {
    get => (Brush)GetValue(FillBrushProperty);
    set => SetValue(FillBrushProperty, value);
  }

  /// <summary>Brush for the unfilled portion of the track (the remainder of the scale).</summary>
  public Brush TrackBrush {
    get => (Brush)GetValue(TrackBrushProperty);
    set => SetValue(TrackBrushProperty, value);
  }

  /// <summary>Solid backdrop painted behind the track.</summary>
  public Brush BarBackground {
    get => (Brush)GetValue(BarBackgroundProperty);
    set => SetValue(BarBackgroundProperty, value);
  }

  /// <summary>Brush used for the outer border.</summary>
  public Brush BorderBrush {
    get => (Brush)GetValue(BorderBrushProperty);
    set => SetValue(BorderBrushProperty, value);
  }

  /// <summary>Stroke thickness of the outer border.</summary>
  public double BorderThickness {
    get => (double)GetValue(BorderThicknessProperty);
    set => SetValue(BorderThicknessProperty, value);
  }

  /// <summary>Applies every property the given theme sets, leaving anything it leaves null untouched.</summary>
  public void ApplyTheme(RangeBarTheme theme) {
    if (theme == null) return;

    if (theme.BarBackground != null) BarBackground = theme.BarBackground;
    if (theme.FillBrush != null) FillBrush = theme.FillBrush;
    if (theme.TrackBrush != null) TrackBrush = theme.TrackBrush;
    if (theme.BorderBrush != null) BorderBrush = theme.BorderBrush;
  }

  private static void OnFillBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((RangeBar)d)._style.FillBrush = (Brush)e.NewValue;

  private static void OnTrackBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((RangeBar)d)._style.TrackBrush = (Brush)e.NewValue;

  private static void OnBarBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((RangeBar)d)._style.BackgroundBrush = (Brush)e.NewValue;

  private static void OnBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var bar = (RangeBar)d;
    bar._style.BorderPen = Helpers.CreateFrozenPen((Brush)e.NewValue, bar._style.BorderPen.Thickness);
  }

  private static void OnBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var bar = (RangeBar)d;
    double thickness = (double)e.NewValue;
    bar._style.BorderPen = Helpers.CreateFrozenPen(bar._style.BorderPen.Brush, thickness);
    bar._style.BorderThickness = thickness;
  }

  protected override Size MeasureOverride(Size availableSize) {
    double width = double.IsInfinity(availableSize.Width) ? 200 : availableSize.Width;
    double height = double.IsInfinity(availableSize.Height) ? 40 : availableSize.Height;
    return new Size(width, height);
  }

  protected override Size ArrangeOverride(Size finalSize) => finalSize;

  protected override void OnRender(DrawingContext dc) {
    base.OnRender(dc);

    Rect bounds = new(RenderSize);

    _backgroundRender.Draw(dc, bounds, _style);

    double range = MaxValue - MinValue;
    double fraction = range > 0 ? (Value - MinValue) / range : 0;

    _fillRender.Draw(dc, bounds, _style, fraction);

    // Border drawn last so its edge stays crisp over the fill instead of being covered.
    _borderRender.Draw(dc, bounds, _style);
  }
}
