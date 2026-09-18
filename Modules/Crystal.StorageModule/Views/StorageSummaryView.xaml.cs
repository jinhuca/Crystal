using Crystal.Controls.PerformanceGraphs;
using Crystal.StorageModule.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.StorageModule.Views;

/// <summary>
/// Storage dashboard tile: the rolled-up header and disk selector on one line, and the
/// selected disk's identity, Active-time + transfer-rate readouts, capacity bar, stats grid and
/// endurance panel below. The system disk is selected by default; selecting a tab swaps the detail.
/// Double-clicking opens the full detail view.
/// </summary>
public partial class StorageSummaryView : UserControl {
  public StorageSummaryView() => InitializeComponent();

  // The two graph cards live inside the SelectedDisk template. That template is reused across disk
  // selections (the ContentControl swaps its bound disk instead of rebuilding), and on first render
  // SelectedDisk can still be null while the inventory loads — so Loaded alone misses the attach
  // (it fires once, often with no disk yet, and never again). Attach on Loaded for the disk already
  // present, and on DataContextChanged for every later swap, including the null -> first-disk one.

  /// <summary>
  /// Attach the activity graph to the selected disk's view model when the graph is loaded. 
  /// This ensures that the graph is properly wired to the correct data context, even if the selected 
  /// disk changes after the initial load.
  /// </summary>
  /// <param name="sender">The graph control that triggered the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnActivityGraphLoaded(object sender, RoutedEventArgs e) => WireActivity(sender);

  /// <summary>
  /// Attach the transfer graph to the selected disk's view model when the graph is loaded.
  /// </summary>
  /// <param name="sender">The graph control that triggered the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnTransferGraphLoaded(object sender, RoutedEventArgs e) => WireTransfer(sender);

  /// <summary>
  /// Attach the graph to the selected disk's view model when the graph is loaded or when the data context changes.
  /// </summary>
  /// <param name="sender">The graph control that triggered the event.</param>
  private void WireActivity(object sender) {
    if (sender is not AdaptiveGraph graph) return;
    graph.DataContextChanged -= OnActivityDataContextChanged;
    graph.DataContextChanged += OnActivityDataContextChanged;
    (graph.DataContext as StorageDriveViewModel)?.AttachActivityGraph(graph);
  }

  /// <summary>
  /// Attach the graph to the selected disk's view model when the graph is loaded or when the data context changes.
  /// </summary>
  /// <param name="sender">The graph control that triggered the event.</param>
  private void WireTransfer(object sender) {
    if (sender is not AdaptiveGraph graph) return;
    graph.DataContextChanged -= OnTransferDataContextChanged;
    graph.DataContextChanged += OnTransferDataContextChanged;
    (graph.DataContext as StorageDriveViewModel)?.AttachTransferGraph(graph);
  }

  /// <summary>
  /// Handle the DataContextChanged event for the activity graph. Detaches the graph from the previous disk's view model 
  /// and attaches it to the new disk's view model.
  /// </summary>
  /// <param name="sender">The graph control that triggered the event.</param>
  /// <param name="e">The event arguments.</param>
  private static void OnActivityDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
    if (sender is not AdaptiveGraph graph) return;
    if (e.OldValue is StorageDriveViewModel previous) {
      previous.DetachActivityGraph(graph);
      graph.Reset(); // Clear the previous disk's trace so the new selection starts from empty.
    }
    (e.NewValue as StorageDriveViewModel)?.AttachActivityGraph(graph);
  }

  /// <summary>
  /// Handle the DataContextChanged event for the transfer graph. Detaches the graph from the previous disk's view model
  /// </summary>
  /// <param name="sender">The graph control that triggered the event.</param>
  /// <param name="e">The event arguments.</param>
  private static void OnTransferDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
    if (sender is not AdaptiveGraph graph) return;
    if (e.OldValue is StorageDriveViewModel previous) {
      previous.DetachTransferGraph(graph);
      graph.Reset(); // Also drops the read/write overlay; AttachTransferGraph re-registers it below.
    }
    (e.NewValue as StorageDriveViewModel)?.AttachTransferGraph(graph);
  }

  /// <summary>
  /// Handle double-clicks on the tile to open the full detail view. This method checks if the click count is 2 or more 
  /// and if the DataContext is an IStorageViewModel with a valid ShowDetailCommand. If so, it executes the command to 
  /// show the detail view.
  /// </summary>
  /// <param name="sender">The tile control that triggered the event.</param>
  /// <param name="e">The mouse button event arguments.</param>
  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IStorageViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
