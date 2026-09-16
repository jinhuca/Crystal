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
}
