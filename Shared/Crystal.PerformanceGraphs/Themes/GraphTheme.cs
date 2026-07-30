using System.Windows.Media;

namespace Crystal.PerformanceGraphs.Themes;

/// <summary>
/// A reusable bundle of the visual properties <see cref="PerformanceGraph"/> exposes.
/// Apply one with <see cref="PerformanceGraph.ApplyTheme"/> instead of setting each
/// property by hand. Every property is optional — <see cref="PerformanceGraph.ApplyTheme"/>
/// only touches the ones you set, so a theme can override just a couple of properties and
/// leave everything else on the control as it was.
/// </summary>
public sealed class GraphTheme {
  /// <summary>Stroke color/brush of the data line.</summary>
  public Brush? LineBrush { get; set; }

  /// <summary>Stroke thickness of the data line.</summary>
  public double? LineThickness { get; set; }

  /// <summary>Fill brush painted under/behind the data (line area, bars, or bar segments).</summary>
  public Brush? FillBrush { get; set; }

  /// <summary>Solid backdrop painted behind the grid and data.</summary>
  public Brush? GraphBackground { get; set; }

  /// <summary>Brush used for the grid lines.</summary>
  public Brush? GridBrush { get; set; }

  /// <summary>Brush used for the outer border.</summary>
  public Brush? BorderBrush { get; set; }
}
