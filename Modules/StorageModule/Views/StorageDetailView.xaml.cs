using System.Windows;
using System.Windows.Controls;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;

namespace StorageModule.Views;

/// <summary>Full-scale Storage view laid out like Windows Task Manager's Disk page: a disk selector
/// on the left, and the selected disk's Active-time + transfer-rate graphs with a stats grid on the
/// right. Reached by selecting the Storage summary tile; the Back control returns to the dashboard.</summary>
public partial class StorageDetailView : UserControl {
  public StorageDetailView() {
    InitializeComponent();
  }

  // The graphs live inside the SelectedDisk content template, so their DataContext is the
  // per-disk VM. Attach on Loaded, which re-runs when the selection swaps the template's disk.
  private void OnActivityGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    graph.ApplyTheme(GraphThemes.Amber(GraphKind.Line));
    if (graph.DataContext is ViewModels.StorageDriveViewModel disk)
      disk.AttachActivityGraph(graph);
  }

  private void OnTransferGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    if (graph.DataContext is ViewModels.StorageDriveViewModel disk)
      disk.AttachTransferGraph(graph);
  }
}
