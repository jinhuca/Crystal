using System.Windows.Media;

namespace Crystal.Controls.MeterGauges.Styles;

internal sealed class GaugeStyle {
  public GaugeStyle() {
    BackgroundBrush = Brushes.Black;
    ActiveBrush = new SolidColorBrush(Color.FromRgb(0x3B, 0xD1, 0x5A));
    InactiveBrush = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x40));
  }

  /// <summary>Solid backdrop painted behind the gauge.</summary>
  public Brush BackgroundBrush { get; set; }

  /// <summary>Brush used for lit ticks (the portion up to the current value).</summary>
  public Brush ActiveBrush { get; set; }

  /// <summary>Brush used for unlit ticks (the remainder of the scale).</summary>
  public Brush InactiveBrush { get; set; }
}
