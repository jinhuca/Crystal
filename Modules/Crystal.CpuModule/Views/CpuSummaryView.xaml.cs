using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.CpuModule.Views;

/// <summary>
/// Compact CPU tile on the dashboard: composes the header and the per-metric tiles
/// (clock, voltage, temperature, power, utilization, fan) plus the per-core strip, 
/// each defined in Views/SummaryViews. 
/// Double-clicking opens the full CPU detail view.
/// </summary>
public partial class CpuSummaryView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="CpuSummaryView"/> class.
  /// </summary>
  public CpuSummaryView() => InitializeComponent();

  /// <summary>
  /// Handles the mouse click event on the tile. If the click count is 2 or more, 
  /// it executes the ShowDetailCommand of the view model to open the detail view.
  /// </summary>
  /// <param name="sender">The sender.</param>
  /// <param name="e">The mouse button event args.</param>
  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is ICpuViewModel vm && vm.ShowDetailCommand.CanExecute(null)) {
      vm.ShowDetailCommand.Execute(null);
    }
  }
}
