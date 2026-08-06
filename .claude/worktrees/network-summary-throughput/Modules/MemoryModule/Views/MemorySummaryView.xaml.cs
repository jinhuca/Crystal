using System.Windows.Controls;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs.Themes;
using MemoryModule.ViewModels;

namespace MemoryModule.Views;

/// <summary>Compact Memory tile on the dashboard: total capacity, slot count, top speed, plus a
/// live Load gauge and load-percentage history graph. Double-clicking opens the full detail view.</summary>
public partial class MemorySummaryView : UserControl {
  public MemorySummaryView() {
    InitializeComponent();
  }

  private void OnGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (UtilizationView.Graph is not { } utilization) return;
    utilization.ApplyTheme(GraphThemes.Emerald());
    if (DataContext is IMemoryViewModel vm)
      vm.AttachGraph(utilization);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IMemoryViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
