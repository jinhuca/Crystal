using Crystal.StorageModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.StorageModule.Views;

/// <summary>Storage dashboard tile: the rolled-up header and disk selector on one line, and the
/// selected disk's identity, Active-time + transfer-rate readouts, capacity bar, stats grid and
/// endurance panel below. The system disk is selected by default; selecting a tab swaps the detail.
/// Double-clicking opens the full detail view.</summary>
public partial class StorageSummaryView : UserControl {
  public StorageSummaryView() => InitializeComponent();

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IStorageViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
