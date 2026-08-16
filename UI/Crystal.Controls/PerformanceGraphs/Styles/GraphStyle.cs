using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Styles;

internal sealed class GraphStyle {
  public GraphStyle() {
    BorderPen = Helpers.CreateFrozenPen(Brushes.Black, 0.8);
    BorderThickness = 0.8;

    GridPen = Helpers.CreateFrozenPen(Brushes.DarkBlue, 0.6);
    LinePen = Helpers.CreateFrozenPen(Brushes.Blue, 1);

    FillBrush = Brushes.Transparent;
    BackgroundBrush = Brushes.Black;
  }

  public Pen BorderPen { get; set; }

  public Pen GridPen { get; set; }

  public Pen LinePen { get; set; }

  // Null until a MarkerBrush is set on the control — no markers are drawn by default, so existing
  // graphs are unaffected.
  public Pen? MarkerPen { get; set; }

  public Brush FillBrush { get; set; }

  public Brush BackgroundBrush { get; set; }

  public double BorderThickness { get; set; } = 0.8;
}
