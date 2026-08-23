using Crystal.GpuModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.GpuModule.Views;

/// <summary>
/// Compact GPU tile on the dashboard: one column per adapter (integrated / dedicated),
/// composing the per-metric tiles (utilization, temperature, clock, power) defined in
/// Views/SummaryViews. Double-clicking opens the full GPU detail view.
/// </summary>
public partial class GpuSummaryView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuSummaryView"/> class.
  /// </summary>
  public GpuSummaryView() => InitializeComponent();

  /// <summary>
  /// Handles the MouseDoubleClick event on the GPU summary tile. If the DataContext is 
  /// an IGpuViewModel and the ShowDetailCommand can be executed, it executes the command
  /// to show the detailed GPU view.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IGpuViewModel vm && vm.ShowDetailCommand.CanExecute(null)) {
      vm.ShowDetailCommand.Execute(null);
    }
  }
}
