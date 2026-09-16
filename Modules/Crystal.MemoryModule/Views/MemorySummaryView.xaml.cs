using Crystal.MemoryModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.MemoryModule.Views;

/// <summary>Memory dashboard tile, laid out like Windows Task Manager's Memory page: the usage and
/// commit-charge readouts plus composition bar on the left, the kernel-memory stats and hardware
/// specs on the right. Double-clicking opens the full detail view (which adds the per-slot list).</summary>
public partial class MemorySummaryView : UserControl {
  public MemorySummaryView() {
    InitializeComponent();
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IMemoryViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
