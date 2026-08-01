using System.Windows.Media;

namespace Crystal.Controls.RangeBars.Themes;

/// <summary>
/// A reusable bundle of the visual properties <see cref="RangeBar"/> exposes. Apply one with
/// <see cref="RangeBar.ApplyTheme"/> instead of setting each property by hand. Every property
/// is optional — <see cref="RangeBar.ApplyTheme"/> only touches the ones you set.
/// </summary>
public sealed class RangeBarTheme {
  /// <summary>Brush for the filled portion.</summary>
  public Brush? FillBrush { get; set; }

  /// <summary>Brush for the unfilled portion of the track.</summary>
  public Brush? TrackBrush { get; set; }

  /// <summary>Solid backdrop painted behind the track.</summary>
  public Brush? BarBackground { get; set; }

  /// <summary>Brush for the outer border.</summary>
  public Brush? BorderBrush { get; set; }
}
