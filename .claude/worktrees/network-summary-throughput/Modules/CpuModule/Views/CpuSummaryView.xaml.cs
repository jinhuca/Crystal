using System.Windows.Controls;
using System.Windows.Input;
using CpuModule.ViewModels.Interfaces;
using Crystal.Controls.PerformanceGraphs.Themes;

namespace CpuModule.Views;

/// <summary>Compact CPU tile on the dashboard: brand, a Load gauge and a load-percentage
/// history graph. Double-clicking (or any click) opens the full CPU detail view.</summary>
public partial class CpuSummaryView : UserControl {
  public CpuSummaryView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (UtilizationView.Graph is not { } utilization) return;
    utilization.ApplyTheme(GraphThemes.Rose());
    if (DataContext is ICpuViewModel vm)
      vm.SensorsViewModel.AttachGraphs(utilization: utilization);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    // Open the detail view on double-click, matching the dashboard's "select or double-click"
    // affordance; a single click is ignored so the tile can host interactive content later.
    if (e.ClickCount >= 2 && DataContext is ICpuViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
