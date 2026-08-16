using Crystal.Controls.PerformanceGraphs;
using Crystal.StorageModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.StorageModule.Views;

/// <summary>Compact Storage tile on the dashboard: total capacity and per-drive identity, plus
/// live active-time-% and transfer-rate history graphs each with a big-value readout (matching the
/// Memory/GPU tiles). Double-clicking opens the full detail view.</summary>
public partial class StorageSummaryView : UserControl {
  public StorageSummaryView() {
    InitializeComponent();
  }

  // Appearance (kind/accent/category/history) is owned by the graph-settings feature, keyed by
  // GraphIdentity.Id in XAML; the handlers only wire each graph's sample buffer to the view model.
  private void OnLoadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (DataContext is IStorageViewModel vm)
      vm.AttachGraph(graph);
  }

  private void OnTransferGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (DataContext is IStorageViewModel vm)
      vm.AttachTransferGraph(graph);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IStorageViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
