using System.Windows;
using System.Windows.Controls;

namespace Crystal.Controls.MeterGauges.Controls;

/// <summary>
/// Groups a <see cref="Title"/> header, a <see cref="MeterGauge"/> arc with its current
/// <see cref="Value"/> shown large at the center, and a <see cref="Unit"/> label into a single
/// templated control — the layout from the reference image (title / value / unit).
/// <para>
/// <see cref="Value"/>, <see cref="MinValue"/>, and <see cref="MaxValue"/> are pushed onto the
/// wrapped <see cref="MeterGauge"/> automatically, so binding them to a view model both sweeps
/// the arc and updates the center label together. The numeric value is rendered through
/// <see cref="ValueFormat"/> (e.g. "{0:0.00}"). The inner gauge is exposed via <see cref="Gauge"/>
/// for setting a theme.
/// </para>
/// </summary>
[TemplatePart(Name = PartGauge, Type = typeof(MeterGauge))]
public class MeterGaugeView : Control {
  private const string PartGauge = "PART_Gauge";

  static MeterGaugeView() {
    DefaultStyleKeyProperty.OverrideMetadata(
        typeof(MeterGaugeView),
        new FrameworkPropertyMetadata(typeof(MeterGaugeView)));
  }

  /// <summary>Header title, e.g. "Voltage".</summary>
  public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register(nameof(Title), typeof(string), typeof(MeterGaugeView),
          new FrameworkPropertyMetadata(string.Empty));

  public string Title {
    get => (string)GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
  }

  /// <summary>Unit label shown below the value, e.g. "V".</summary>
  public static readonly DependencyProperty UnitProperty =
      DependencyProperty.Register(nameof(Unit), typeof(string), typeof(MeterGaugeView),
          new FrameworkPropertyMetadata(string.Empty));

  public string Unit {
    get => (string)GetValue(UnitProperty);
    set => SetValue(UnitProperty, value);
  }

  /// <summary>Current reading; sweeps the gauge and shows at its center. Drives <see cref="MeterGauge.Value"/>.</summary>
  public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value), typeof(double), typeof(MeterGaugeView),
          new FrameworkPropertyMetadata(0.0, OnValueChanged));

  public double Value {
    get => (double)GetValue(ValueProperty);
    set => SetValue(ValueProperty, value);
  }

  /// <summary>Composite format applied to <see cref="Value"/> for the center label, e.g. "{0:0.00}".</summary>
  public static readonly DependencyProperty ValueFormatProperty =
      DependencyProperty.Register(nameof(ValueFormat), typeof(string), typeof(MeterGaugeView),
          new FrameworkPropertyMetadata("{0:0.00}"));

  public string ValueFormat {
    get => (string)GetValue(ValueFormatProperty);
    set => SetValue(ValueFormatProperty, value);
  }

  /// <summary>Bottom of the gauge's scale. Drives <see cref="MeterGauge.MinValue"/>.</summary>
  public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(MeterGaugeView),
          new FrameworkPropertyMetadata(0.0, OnMinValueChanged));

  public double MinValue {
    get => (double)GetValue(MinValueProperty);
    set => SetValue(MinValueProperty, value);
  }

  /// <summary>Top of the gauge's scale. Drives <see cref="MeterGauge.MaxValue"/>.</summary>
  public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(MeterGaugeView),
          new FrameworkPropertyMetadata(100.0, OnMaxValueChanged));

  public double MaxValue {
    get => (double)GetValue(MaxValueProperty);
    set => SetValue(MaxValueProperty, value);
  }

  /// <summary>The wrapped gauge, available once the template is applied.</summary>
  public MeterGauge? Gauge { get; private set; }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();
    Gauge = GetTemplateChild(PartGauge) as MeterGauge;
    if (Gauge != null) {
      // The scale DPs may have been set (or bound) before the template produced the gauge;
      // push their current values so the arc starts in sync.
      Gauge.MinValue = MinValue;
      Gauge.MaxValue = MaxValue;
      Gauge.Value = Value;
    }
  }

  private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (((MeterGaugeView)d).Gauge is { } gauge) gauge.Value = (double)e.NewValue;
  }

  private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (((MeterGaugeView)d).Gauge is { } gauge) gauge.MinValue = (double)e.NewValue;
  }

  private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (((MeterGaugeView)d).Gauge is { } gauge) gauge.MaxValue = (double)e.NewValue;
  }
}
