using Crystal.GpuModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.GpuModule.Views;

/// <summary>Compact GPU tile on the dashboard: one column per adapter (integrated / dedicated),
/// composing the per-metric tiles (utilization, temperature, clock, power) defined in
/// Views/SummaryViews. Double-clicking opens the full GPU detail view.</summary>
public partial class GpuSummaryView : UserControl {
  public GpuSummaryView() => InitializeComponent();

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IGpuViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
