using Crystal.Plot2D.Common;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Charts;

/// <summary>
///   Represents a menu that appears in Debug version of Crystal.Plot2D.
/// </summary>
public sealed class DebugMenu : IPlotterElement {
  /// <summary>
  ///   Initializes a new instance of the <see cref="DebugMenu"/> class.
  /// </summary>
  public DebugMenu() {
    Panel.SetZIndex(element: Menu, value: 1);
  }

  public Menu Menu { get; } = new() {
    HorizontalAlignment = HorizontalAlignment.Left,
    VerticalAlignment = VerticalAlignment.Top,
    Margin = new Thickness(uniformLength: 3)
  };

  public MenuItem TryFindMenuItem(string itemName) {
    return Menu.Items.OfType<MenuItem>().FirstOrDefault(predicate: item => item.Header.ToString() == itemName);
  }

  #region IPlotterElement Members

  public void OnPlotterAttached(PlotterBase plotter) {
    Plotter = plotter;
    plotter.CentralGrid.Children.Add(element: Menu);
  }

  public void OnPlotterDetaching(PlotterBase plotter) {
    plotter.CentralGrid.Children.Remove(element: Menu);
    Plotter = null;
  }

  public PlotterBase Plotter { get; private set; }

  #endregion
}
