using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
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

  private void OnLoadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    graph.ApplyTheme(GraphThemes.Amber(GraphKind.Line));
    if (DataContext is IStorageViewModel vm)
      vm.AttachGraph(graph);
  }

  private void OnTransferGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    if (DataContext is IStorageViewModel vm)
      vm.AttachTransferGraph(graph);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IStorageViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
