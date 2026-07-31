using Crystal.Controls.MeterGauges.Renders;
using Crystal.Controls.MeterGauges.Styles;
using Crystal.Controls.MeterGauges.Themes;
using System;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.MeterGauges;

/// <summary>
/// A radial meter gauge: a fan of tick marks swept along an open-bottom arc, lit from the start
/// up to the point representing <see cref="Value"/> on the <see cref="MinValue"/>..<see cref="MaxValue"/>
/// scale. The lit ticks use <see cref="ActiveBrush"/>, the rest <see cref="InactiveBrush"/>.
/// <para>
/// This is the low-level drawing primitive (analogous to <c>PerformanceGraph</c>); the labeled
/// title/value/unit chrome lives in <see cref="Controls.MeterGaugeView"/>.
/// </para>
/// </summary>
public class MeterGauge : FrameworkElement {
  private const int DefaultTickCount = 60;

  /// <summary>Identifies the <see cref="Value"/> dependency property.</summary>
  public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value), typeof(double), typeof(MeterGauge),
          new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="MinValue"/> dependency property.</summary>
  public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(MeterGauge),
          new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="MaxValue"/> dependency property.</summary>
  public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(MeterGauge),
          new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="TickCount"/> dependency property.</summary>
  public static readonly DependencyProperty TickCountProperty =
      DependencyProperty.Register(nameof(TickCount), typeof(int), typeof(MeterGauge),
          new FrameworkPropertyMetadata(DefaultTickCount, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="StartAngle"/> dependency property.</summary>
  public static readonly DependencyProperty StartAngleProperty =
      DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(MeterGauge),
          new FrameworkPropertyMetadata(135.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="SweepAngle"/> dependency property.</summary>
  public static readonly DependencyProperty SweepAngleProperty =
      DependencyProperty.Register(nameof(SweepAngle), typeof(double), typeof(MeterGauge),
          new FrameworkPropertyMetadata(270.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="TickThickness"/> dependency property.</summary>
  public static readonly DependencyProperty TickThicknessProperty =
      DependencyProperty.Register(nameof(TickThickness), typeof(double), typeof(MeterGauge),
          new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>Identifies the <see cref="ActiveBrush"/> dependency property.</summary>
  public static readonly DependencyProperty ActiveBrushProperty =
      DependencyProperty.Register(nameof(ActiveBrush), typeof(Brush), typeof(MeterGauge),
          new FrameworkPropertyMetadata(
              new SolidColorBrush(Color.FromRgb(0x3B, 0xD1, 0x5A)),
              FrameworkPropertyMetadataOptions.AffectsRender, OnActiveBrushChanged));

  /// <summary>Identifies the <see cref="InactiveBrush"/> dependency property.</summary>
  public static readonly DependencyProperty InactiveBrushProperty =
      DependencyProperty.Register(nameof(InactiveBrush), typeof(Brush), typeof(MeterGauge),
          new FrameworkPropertyMetadata(
              new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x40)),
              FrameworkPropertyMetadataOptions.AffectsRender, OnInactiveBrushChanged));

  /// <summary>Identifies the <see cref="GaugeBackground"/> dependency property.</summary>
  public static readonly DependencyProperty GaugeBackgroundProperty =
      DependencyProperty.Register(nameof(GaugeBackground), typeof(Brush), typeof(MeterGauge),
          new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnGaugeBackgroundChanged));

  private readonly BackgroundRenderer _backgroundRender = new();
  private readonly TickArcRenderer _tickArcRender = new();
  private readonly GaugeStyle _gaugeStyle = new();

  public MeterGauge() {
    SnapsToDevicePixels = true;
    UseLayoutRounding = true;
  }

  /// <summary>Current reading; clamped to <see cref="MinValue"/>..<see cref="MaxValue"/> when drawn.</summary>
  public double Value {
    get => (double)GetValue(ValueProperty);
    set => SetValue(ValueProperty, value);
  }

  /// <summary>Reading that maps to the first tick.</summary>
  public double MinValue {
    get => (double)GetValue(MinValueProperty);
    set => SetValue(MinValueProperty, value);
  }

  /// <summary>Reading that maps to the last tick.</summary>
  public double MaxValue {
    get => (double)GetValue(MaxValueProperty);
    set => SetValue(MaxValueProperty, value);
  }

  /// <summary>Number of tick marks drawn around the arc.</summary>
  public int TickCount {
    get => (int)GetValue(TickCountProperty);
    set => SetValue(TickCountProperty, value);
  }

  /// <summary>Angle (degrees, clockwise from east) of the first tick. Default 135° (lower-left).</summary>
  public double StartAngle {
    get => (double)GetValue(StartAngleProperty);
    set => SetValue(StartAngleProperty, value);
  }

  /// <summary>Total sweep (degrees, clockwise) from the first tick to the last. Default 270°.</summary>
  public double SweepAngle {
    get => (double)GetValue(SweepAngleProperty);
    set => SetValue(SweepAngleProperty, value);
  }

  /// <summary>Width of each tick mark, perpendicular to its radial direction.</summary>
  public double TickThickness {
    get => (double)GetValue(TickThicknessProperty);
    set => SetValue(TickThicknessProperty, value);
  }

  /// <summary>Brush for lit ticks (from the start up to <see cref="Value"/>).</summary>
  public Brush ActiveBrush {
    get => (Brush)GetValue(ActiveBrushProperty);
    set => SetValue(ActiveBrushProperty, value);
  }

  /// <summary>Brush for unlit ticks (the remainder of the scale).</summary>
  public Brush InactiveBrush {
    get => (Brush)GetValue(InactiveBrushProperty);
    set => SetValue(InactiveBrushProperty, value);
  }

  /// <summary>Solid backdrop painted behind the ticks.</summary>
  public Brush GaugeBackground {
    get => (Brush)GetValue(GaugeBackgroundProperty);
    set => SetValue(GaugeBackgroundProperty, value);
  }

  /// <summary>Applies every property the given theme sets, leaving anything it leaves null untouched.</summary>
  public void ApplyTheme(GaugeTheme theme) {
    if (theme == null) return;

    if (theme.GaugeBackground != null) GaugeBackground = theme.GaugeBackground;
    if (theme.ActiveBrush != null) ActiveBrush = theme.ActiveBrush;
    if (theme.InactiveBrush != null) InactiveBrush = theme.InactiveBrush;
  }

  private static void OnActiveBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((MeterGauge)d)._gaugeStyle.ActiveBrush = (Brush)e.NewValue;

  private static void OnInactiveBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((MeterGauge)d)._gaugeStyle.InactiveBrush = (Brush)e.NewValue;

  private static void OnGaugeBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((MeterGauge)d)._gaugeStyle.BackgroundBrush = (Brush)e.NewValue;

  protected override Size MeasureOverride(Size availableSize) {
    double width = double.IsInfinity(availableSize.Width) ? 200 : availableSize.Width;
    double height = double.IsInfinity(availableSize.Height) ? 200 : availableSize.Height;
    return new Size(width, height);
  }

  protected override Size ArrangeOverride(Size finalSize) => finalSize;

  protected override void OnRender(DrawingContext dc) {
    base.OnRender(dc);

    Rect bounds = new(RenderSize);

    _backgroundRender.Draw(dc, bounds, _gaugeStyle);

    double range = MaxValue - MinValue;
    double fraction = range > 0 ? (Value - MinValue) / range : 0;

    _tickArcRender.Draw(dc, bounds, _gaugeStyle, fraction, TickCount,
        StartAngle, SweepAngle, TickThickness,
        innerRadiusRatio: 0.78, outerRadiusRatio: 0.98);
  }
}
