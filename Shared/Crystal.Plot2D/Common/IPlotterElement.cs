namespace Crystal.Plot2D.Common;

/// <summary>
/// Main interface of Plotter2D: each item that is going to be added to Plotter should implement it.
/// Contains methods that are called by parent plotter when item is added to it or removed from it.
/// </summary>
public interface IPlotterElement {
  /// <summary>
  /// Called when parent plotter is attached.
  /// Allows to, for example, add custom UI parts to Plotter's visual tree or subscribe to Plotter's events.
  /// </summary>
  /// <param name="plotter">
  /// The parent plotter.
  /// </param>
  void OnPlotterAttached(PlotterBase plotter);

  /// <summary>
  /// Called when item is being detached from parent plotter.
  /// Allows to remove added in OnPlotterAttached method UI parts or unsubscribe from events.
  /// This should be done as each chart can be added only one Plotter at one moment of time.
  /// </summary>
  /// <param name="plotter">
  /// The parent plotter.
  /// </param>
  void OnPlotterDetaching(PlotterBase plotter);

  /// <summary>
  /// Gets the parent plotter of chart.
  /// Should be equal to null if item is not connected to any plotter.
  /// </summary>
  /// <value>
  /// The parent plotter.
  /// </value>
  PlotterBase Plotter { get; }
}
