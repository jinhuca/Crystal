using Crystal.OSModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.OSModule.Views;

/// <summary>Compact OS tile on the dashboard: name, feature-update version, build, architecture,
/// plus live uptime and system time. Double-clicking opens the full detail view.</summary>
public partial class OsSummaryView : UserControl {
  public OsSummaryView() {
    InitializeComponent();
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IOsViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }

  // Double-click on the Processes sub-tile opens the full process list in its own window. Marked
  // handled so it doesn't bubble to the outer tile's OnTileClick (which opens the OS detail view).
  private void OnProcessesTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount < 2) return;
    e.Handled = true;
    if (DataContext is IOsViewModel vm && vm.ShowProcessesCommand.CanExecute(null))
      vm.ShowProcessesCommand.Execute(null);
  }
}
