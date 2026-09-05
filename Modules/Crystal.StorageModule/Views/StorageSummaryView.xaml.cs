using Crystal.Controls.PerformanceGraphs;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.StorageModule.Views;

/// <summary>Storage dashboard tile: the rolled-up header and disk selector on one line, and the
/// selected disk's identity, Active-time + transfer-rate graphs, capacity bar, stats grid and
/// endurance panel below. The system disk is selected by default; selecting a tab swaps the detail.</summary>
public partial class StorageSummaryView : UserControl {
  public StorageSummaryView() => InitializeComponent();

  // The graphs live inside the SelectedDisk content template, so their DataContext is the
  // per-disk VM. Attach on Loaded, which re-runs when the selection swaps the template's disk.
  private void OnActivityGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is not AdaptiveGraph graph) return;
    if (graph.DataContext is ViewModels.StorageDriveViewModel disk)
      disk.AttachActivityGraph(graph);
  }

  private void OnTransferGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is not AdaptiveGraph graph) return;
    if (graph.DataContext is ViewModels.StorageDriveViewModel disk)
      disk.AttachTransferGraph(graph);
  }
}
