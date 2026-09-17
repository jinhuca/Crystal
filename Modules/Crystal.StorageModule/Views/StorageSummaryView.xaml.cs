using Crystal.Controls.PerformanceGraphs;
using Crystal.StorageModule.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.StorageModule.Views;

/// <summary>Storage dashboard tile: the rolled-up header and disk selector on one line, and the
/// selected disk's identity, Active-time + transfer-rate readouts, capacity bar, stats grid and
/// endurance panel below. The system disk is selected by default; selecting a tab swaps the detail.
/// Double-clicking opens the full detail view.</summary>
public partial class StorageSummaryView : UserControl {
  public StorageSummaryView() => InitializeComponent();

  // The two graph cards live inside the SelectedDisk template, so each sparkline self-registers with
  // its own disk's view model as it's realized. Loaded re-fires with a fresh graph when the disk
  // selection swaps the template, and re-attaching the same instance is a no-op.
  private void OnActivityGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is AdaptiveGraph { DataContext: StorageDriveViewModel disk } graph)
      disk.AttachActivityGraph(graph);
  }

  private void OnTransferGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is AdaptiveGraph { DataContext: StorageDriveViewModel disk } graph)
      disk.AttachTransferGraph(graph);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IStorageViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
