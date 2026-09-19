using Crystal.Controls.PerformanceGraphs;
using Crystal.StorageModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.StorageModule.Views;

/// <summary>Full-scale Storage view laid out like Windows Task Manager's Disk page: a disk selector
/// on the left, and the selected disk's Active-time + transfer-rate graphs with a stats grid on the
/// right. Reached by selecting the Storage summary tile; the Back control returns to the dashboard.</summary>
public partial class StorageDetailView : UserControl {
  public StorageDetailView() {
    InitializeComponent();
  }

  // The graphs live inside the SelectedDisk content template, whose DataContext is the per-disk VM.
  // The template is reused across selections (the ContentControl swaps its bound disk rather than
  // rebuilding) and the disk can still be null on first render, so Loaded alone misses the attach.
  // Attach on Loaded for the disk already present, and on DataContextChanged for every later swap.
  private void OnActivityGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    graph.DataContextChanged -= OnActivityDataContextChanged;
    graph.DataContextChanged += OnActivityDataContextChanged;
    (graph.DataContext as StorageDriveViewModel)?.AttachActivityGraph(graph);
  }

  private void OnTransferGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    graph.DataContextChanged -= OnTransferDataContextChanged;
    graph.DataContextChanged += OnTransferDataContextChanged;
    (graph.DataContext as StorageDriveViewModel)?.AttachTransferGraph(graph);
  }

  private static void OnActivityDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (e.OldValue is StorageDriveViewModel previous) {
      previous.DetachActivityGraph(graph);
      graph.Reset();
    }
    (e.NewValue as StorageDriveViewModel)?.AttachActivityGraph(graph);
  }

  private static void OnTransferDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
    if (sender is not PerformanceGraph graph) return;
    if (e.OldValue is StorageDriveViewModel previous) {
      previous.DetachTransferGraph(graph);
      graph.Reset();
    }
    (e.NewValue as StorageDriveViewModel)?.AttachTransferGraph(graph);
  }
}
