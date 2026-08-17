using Crystal.MemoryModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.MemoryModule.Views;

/// <summary>Compact Memory tile on the dashboard: composes the header and the utilization / used
/// metric tiles (each defined in Views/SummaryViews). Double-clicking opens the full detail view.</summary>
public partial class MemorySummaryView : UserControl {
  public MemorySummaryView() => InitializeComponent();

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IMemoryViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
