using Crystal.ProcessModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.ProcessModule.Views;

/// <summary>
/// Compact Processes tile on the dashboard: live system-wide process, thread and handle totals.
/// Double-clicking opens the full <see cref="ProcessDetailView"/> in its own window.
/// </summary>
public partial class ProcessSummaryView : UserControl {
  public ProcessSummaryView() {
    InitializeComponent();
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is ProcessSummaryViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
