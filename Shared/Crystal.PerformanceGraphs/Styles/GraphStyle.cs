using System.Windows.Media;

namespace Crystal.PerformanceGraphs.Styles;

internal sealed class GraphStyle {
  public GraphStyle() {
    BorderPen = Helpers.CreateFrozenPen(Brushes.Black, 1);
    BorderThickness = 1;

    GridPen = Helpers.CreateFrozenPen(Brushes.DarkBlue, 1);
    LinePen = Helpers.CreateFrozenPen(Brushes.Blue, 2);

    FillBrush = Brushes.Transparent;
    BackgroundBrush = Brushes.Black;
  }

  public Pen BorderPen { get; set; }

  public Pen GridPen { get; set; }

  public Pen LinePen { get; set; }

  public Brush FillBrush { get; set; }

  public Brush BackgroundBrush { get; set; }

  public double BorderThickness { get; set; } = 1;
}
