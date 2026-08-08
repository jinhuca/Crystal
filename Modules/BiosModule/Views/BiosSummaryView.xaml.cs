using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BiosModule.ViewModels;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;

namespace BiosModule.Views;

/// <summary>Full-width BIOS strip on the dashboard: manufacturer, version, release date, SMBIOS.
/// Double-clicking opens the full BIOS detail view.</summary>
public partial class BiosSummaryView : UserControl {
  public BiosSummaryView() {
    InitializeComponent();
  }

  // Each rail sparkline themes itself on load; the VM attach is idempotent so whichever graph
  // loads first wires all three. (A root-element Loaded handler collides with the BiosModule
  // namespace/class name in WPF's generated code, so it's done per-graph like StorageModule.)
  private void OnGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is PerformanceGraph graph)
      graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    if (DataContext is IBiosViewModel vm)
      vm.AttachRailGraphs(Rail3V3Spark, Rail5VSpark, Rail12VSpark);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IBiosViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
