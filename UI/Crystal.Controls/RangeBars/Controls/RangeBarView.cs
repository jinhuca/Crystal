using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Controls;

/// <summary>
/// Groups a <see cref="Title"/>, a <see cref="Unit"/>, the current <see cref="Value"/>, the
/// <see cref="MinValue"/>/<see cref="MaxValue"/> end labels, and a <see cref="RangeBar"/> track
/// into a single templated control — the layout from the reference image
/// (Title / Unit / Value / Min Value [====bar====] Max Value).
/// <para>
/// <see cref="Value"/>, <see cref="MinValue"/>, and <see cref="MaxValue"/> are pushed onto the
/// wrapped <see cref="RangeBar"/> automatically, so binding them to a view model both fills the
/// bar and updates the labels together. The numeric values are rendered through
/// <see cref="ValueFormat"/> (e.g. "{0:0.00}"). The inner bar is exposed via <see cref="Bar"/>
/// for setting a theme.
/// </para>
/// </summary>
[TemplatePart(Name = PartBar, Type = typeof(RangeBar))]
public class RangeBarView : Control {
  private const string PartBar = "PART_Bar";

  static RangeBarView() {
    DefaultStyleKeyProperty.OverrideMetadata(
        typeof(RangeBarView),
        new FrameworkPropertyMetadata(typeof(RangeBarView)));
  }

  /// <summary>Header title, e.g. "Voltage".</summary>
  public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register(nameof(Title), typeof(string), typeof(RangeBarView),
          new FrameworkPropertyMetadata(string.Empty));

  public string Title {
    get => (string)GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
  }

  /// <summary>Unit label, e.g. "V".</summary>
  public static readonly DependencyProperty UnitProperty =
      DependencyProperty.Register(nameof(Unit), typeof(string), typeof(RangeBarView),
          new FrameworkPropertyMetadata(string.Empty));

  public string Unit {
    get => (string)GetValue(UnitProperty);
    set => SetValue(UnitProperty, value);
  }

  /// <summary>Current reading; fills the bar and shows as the value label. Drives <see cref="RangeBar.Value"/>.</summary>
  public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value), typeof(double), typeof(RangeBarView),
          new FrameworkPropertyMetadata(0.0));

  public double Value {
    get => (double)GetValue(ValueProperty);
    set => SetValue(ValueProperty, value);
  }

  /// <summary>Composite format applied to the value/min/max labels, e.g. "{0:0.00}".</summary>
  public static readonly DependencyProperty ValueFormatProperty =
      DependencyProperty.Register(nameof(ValueFormat), typeof(string), typeof(RangeBarView),
          new FrameworkPropertyMetadata("{0:0.00}"));

  public string ValueFormat {
    get => (string)GetValue(ValueFormatProperty);
    set => SetValue(ValueFormatProperty, value);
  }

  /// <summary>Left end of the bar's scale. Drives <see cref="RangeBar.MinValue"/>.</summary>
  public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(RangeBarView),
          new FrameworkPropertyMetadata(0.0));

  public double MinValue {
    get => (double)GetValue(MinValueProperty);
    set => SetValue(MinValueProperty, value);
  }

  /// <summary>Right end of the bar's scale. Drives <see cref="RangeBar.MaxValue"/>.</summary>
  public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(RangeBarView),
          new FrameworkPropertyMetadata(100.0));

  public double MaxValue {
    get => (double)GetValue(MaxValueProperty);
    set => SetValue(MaxValueProperty, value);
  }

  /// <summary>When true, the inner bar draws its fill as discrete LED-meter blocks instead of a
  /// solid fill. Drives <see cref="RangeBar.Segmented"/>.</summary>
  public static readonly DependencyProperty SegmentedProperty =
      DependencyProperty.Register(nameof(Segmented), typeof(bool), typeof(RangeBarView),
          new FrameworkPropertyMetadata(false));

  public bool Segmented {
    get => (bool)GetValue(SegmentedProperty);
    set => SetValue(SegmentedProperty, value);
  }

  /// <summary>Width (px) of each lit LED block when <see cref="Segmented"/> is true. Drives <see cref="RangeBar.SegmentWidth"/>.</summary>
  public static readonly DependencyProperty SegmentWidthProperty =
      DependencyProperty.Register(nameof(SegmentWidth), typeof(double), typeof(RangeBarView),
          new FrameworkPropertyMetadata(4.0));

  public double SegmentWidth {
    get => (double)GetValue(SegmentWidthProperty);
    set => SetValue(SegmentWidthProperty, value);
  }

  /// <summary>Gap (px) between LED blocks when <see cref="Segmented"/> is true. Drives <see cref="RangeBar.SegmentGap"/>.</summary>
  public static readonly DependencyProperty SegmentGapProperty =
      DependencyProperty.Register(nameof(SegmentGap), typeof(double), typeof(RangeBarView),
          new FrameworkPropertyMetadata(2.0));

  public double SegmentGap {
    get => (double)GetValue(SegmentGapProperty);
    set => SetValue(SegmentGapProperty, value);
  }

  /// <summary>When &gt; 0, the inner bar draws exactly this many squares across the full scale
  /// instead of pixel-sized LED blocks. Drives <see cref="RangeBar.SegmentCount"/>.</summary>
  public static readonly DependencyProperty SegmentCountProperty =
      DependencyProperty.Register(nameof(SegmentCount), typeof(int), typeof(RangeBarView),
          new FrameworkPropertyMetadata(0));

  public int SegmentCount {
    get => (int)GetValue(SegmentCountProperty);
    set => SetValue(SegmentCountProperty, value);
  }

  /// <summary>When set, the inner bar paints its fill with an alpha gradient generated from this
  /// color. Drives <see cref="RangeBar.AccentColor"/>.</summary>
  public static readonly DependencyProperty AccentColorProperty =
      DependencyProperty.Register(nameof(AccentColor), typeof(Color?), typeof(RangeBarView),
          new FrameworkPropertyMetadata(null));

  public Color? AccentColor {
    get => (Color?)GetValue(AccentColorProperty);
    set => SetValue(AccentColorProperty, value);
  }

  /// <summary>Whether the accent alpha gradient rises or falls across the scale. Drives <see cref="RangeBar.Direction"/>.</summary>
  public static readonly DependencyProperty DirectionProperty =
      DependencyProperty.Register(nameof(Direction), typeof(RangeBarGradientDirection), typeof(RangeBarView),
          new FrameworkPropertyMetadata(RangeBarGradientDirection.Ascending));

  public RangeBarGradientDirection Direction {
    get => (RangeBarGradientDirection)GetValue(DirectionProperty);
    set => SetValue(DirectionProperty, value);
  }

  /// <summary>Alpha at the low end of the accent gradient. Drives <see cref="RangeBar.LowestAlpha"/>.</summary>
  public static readonly DependencyProperty LowestAlphaProperty =
      DependencyProperty.Register(nameof(LowestAlpha), typeof(byte), typeof(RangeBarView),
          new FrameworkPropertyMetadata((byte)0x55));

  public byte LowestAlpha {
    get => (byte)GetValue(LowestAlphaProperty);
    set => SetValue(LowestAlphaProperty, value);
  }

  /// <summary>Alpha at the high end of the accent gradient. Drives <see cref="RangeBar.HighestAlpha"/>.</summary>
  public static readonly DependencyProperty HighestAlphaProperty =
      DependencyProperty.Register(nameof(HighestAlpha), typeof(byte), typeof(RangeBarView),
          new FrameworkPropertyMetadata((byte)0xFF));

  public byte HighestAlpha {
    get => (byte)GetValue(HighestAlphaProperty);
    set => SetValue(HighestAlphaProperty, value);
  }

  /// <summary>The wrapped bar, available once the template is applied.</summary>
  public RangeBar? Bar { get; private set; }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();
    // Value/MinValue/MaxValue flow to the bar via TemplateBindings in the control template;
    // Bar is captured only so callers can set a theme or the segment style on the primitive.
    Bar = GetTemplateChild(PartBar) as RangeBar;
  }
}
