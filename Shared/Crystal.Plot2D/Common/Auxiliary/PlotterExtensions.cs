namespace Crystal.Plot2D.Common.Auxiliary;

public static class PlotterExtensions {
  public static void AddChild(this PlotterBase plotter, IPlotterElement child) {
    plotter.Children.Add(item: child);
  }
}
