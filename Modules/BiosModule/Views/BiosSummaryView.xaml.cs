using System.Windows.Controls;
using System.Windows.Input;
using BiosModule.ViewModels;

namespace BiosModule.Views;

/// <summary>Full-width BIOS strip on the dashboard: manufacturer, version, release date, SMBIOS.
/// Double-clicking opens the full BIOS detail view.</summary>
public partial class BiosSummaryView : UserControl {
  public BiosSummaryView() {
    InitializeComponent();
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IBiosViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
