using System.Windows.Controls;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs.Themes;
using StorageModule.ViewModels;

namespace StorageModule.Views;

/// <summary>Compact Storage tile on the dashboard: total capacity and drive count, plus a live
/// Activity gauge and activity-percentage history graph. Double-clicking opens the full detail view.</summary>
public partial class StorageSummaryView : UserControl {
  public StorageSummaryView() {
    InitializeComponent();
  }

  private void OnGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (UtilizationView.Graph is not { } utilization) return;
    utilization.ApplyTheme(GraphThemes.Amber());
    if (DataContext is IStorageViewModel vm)
      vm.AttachGraph(utilization);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IStorageViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
