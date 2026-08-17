using Crystal.StorageModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.StorageModule.Views;

/// <summary>Compact Storage tile on the dashboard: composes the header, filesystem-capacity roll-up,
/// and the active-time / transfer-rate graph tiles, each defined in Views/SummaryViews.
/// Double-clicking opens the full detail view.</summary>
public partial class StorageSummaryView : UserControl {
  public StorageSummaryView() => InitializeComponent();

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IStorageViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
