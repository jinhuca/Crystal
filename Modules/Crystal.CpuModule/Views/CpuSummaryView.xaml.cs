using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.CpuModule.Views;

/// <summary>Compact CPU tile on the dashboard: composes the header and the per-metric tiles
/// (clock, voltage, temperature, power, utilization, fan) plus the per-core strip, each defined in
/// Views/SummaryViews. Double-clicking opens the full CPU detail view.</summary>
public partial class CpuSummaryView : UserControl {
  public CpuSummaryView() => InitializeComponent();

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    // Open the detail view on double-click, matching the dashboard's "select or double-click"
    // affordance; a single click is ignored so the tile can host interactive content later.
    if (e.ClickCount >= 2 && DataContext is ICpuViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
