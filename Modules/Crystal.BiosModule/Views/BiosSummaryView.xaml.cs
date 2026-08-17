using Crystal.BiosModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.BiosModule.Views;

/// <summary>Full-width BIOS strip on the dashboard: composes the header, firmware posture, board
/// health status, and live board readings, each defined in Views/SummaryViews. Double-clicking opens
/// the full BIOS detail view.</summary>
public partial class BiosSummaryView : UserControl {
  public BiosSummaryView() => InitializeComponent();

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IBiosViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
