using System.Windows.Media;

namespace Crystal.Controls.MeterGauges.Themes;

/// <summary>
/// A reusable bundle of the visual properties <see cref="MeterGauge"/> exposes. Apply one with
/// <see cref="MeterGauge.ApplyTheme"/> instead of setting each property by hand. Every property
/// is optional — <see cref="MeterGauge.ApplyTheme"/> only touches the ones you set.
/// </summary>
public sealed class GaugeTheme {
  /// <summary>Brush for lit ticks.</summary>
  public Brush? ActiveBrush { get; set; }

  /// <summary>Brush for unlit ticks.</summary>
  public Brush? InactiveBrush { get; set; }

  /// <summary>Solid backdrop painted behind the ticks.</summary>
  public Brush? GaugeBackground { get; set; }
}
