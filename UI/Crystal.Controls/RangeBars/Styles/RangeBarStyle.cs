using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Styles;

internal sealed class RangeBarStyle {
  public RangeBarStyle() {
    BackgroundBrush = Brushes.Black;
    FillBrush = new SolidColorBrush(Color.FromRgb(0x3B, 0xD1, 0x5A));
    TrackBrush = Brushes.Transparent;
    BorderPen = Helpers.CreateFrozenPen(Brushes.Black, 3);
    BorderThickness = 3;
  }

  /// <summary>Solid backdrop painted behind the track.</summary>
  public Brush BackgroundBrush { get; set; }

  /// <summary>Brush used for the filled portion (up to the current value).</summary>
  public Brush FillBrush { get; set; }

  /// <summary>Brush used for the unfilled portion of the track (the remainder of the scale).</summary>
  public Brush TrackBrush { get; set; }

  /// <summary>Pen used for the outer border.</summary>
  public Pen BorderPen { get; set; }

  /// <summary>Stroke thickness of the outer border.</summary>
  public double BorderThickness { get; set; }

  /// <summary>True to draw the filled portion as discrete LED-meter blocks instead of a solid fill.</summary>
  public bool Segmented { get; set; }

  /// <summary>Width (px) of each lit LED block when <see cref="Segmented"/> is true.</summary>
  public double SegmentWidth { get; set; } = 4;

  /// <summary>Gap (px) between LED blocks when <see cref="Segmented"/> is true.</summary>
  public double SegmentGap { get; set; } = 2;

  /// <summary>When &gt; 0, <see cref="Segmented"/> draws exactly this many equally spaced squares
  /// (side = bar height) spanning the full scale, overriding <see cref="SegmentWidth"/>/<see cref="SegmentGap"/>.
  /// The meter fills per square, so the block at the value edge is partially lit.</summary>
  public int SegmentCount { get; set; }

  /// <summary>When set, the fill is painted with an alpha gradient generated from this color
  /// instead of <see cref="FillBrush"/>. Null keeps the solid <see cref="FillBrush"/> behavior.</summary>
  public Color? AccentColor { get; set; }

  /// <summary>Direction the accent-color alpha gradient runs across the scale.</summary>
  public RangeBarGradientDirection Direction { get; set; } = RangeBarGradientDirection.Ascending;

  /// <summary>Alpha at the low end of the accent gradient.</summary>
  public byte LowestAlpha { get; set; } = 0x55;

  /// <summary>Alpha at the high end of the accent gradient.</summary>
  public byte HighestAlpha { get; set; } = 0xFF;
}
