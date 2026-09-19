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
}
