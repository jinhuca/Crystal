using Crystal.BiosModule.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.BiosModule.Views;

/// <summary>Full-width BIOS strip on the dashboard: manufacturer, version, release date, SMBIOS.
/// Double-clicking opens the full BIOS detail view.</summary>
public partial class BiosSummaryView : UserControl {
  public BiosSummaryView() {
    InitializeComponent();
  }

  // Appearance (kind/accent/category/history) is owned by the graph-settings feature, keyed by
  // GraphIdentity.Id in XAML; the handlers only wire each graph's sample buffer to the view model.
  // The VM attach is idempotent so whichever graph loads first wires all three rails.
  private void OnGraphLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is IBiosViewModel vm)
      vm.AttachRailGraphs(Rail3V3Spark, Rail5VSpark, Rail12VSpark);
  }

  private void OnBoardTempSparkLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is IBiosViewModel vm)
      vm.AttachBoardTempGraph(BoardTempSpark);
  }

  private void OnFanSparkLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is IBiosViewModel vm)
      vm.AttachFanGraph(ChassisFanSpark);
  }

  private void OnCmosSparkLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is IBiosViewModel vm)
      vm.AttachCmosGraph(CmosSpark);
  }

  private void OnTileClick(object sender, MouseButtonEventArgs e) {
    if (e.ClickCount >= 2 && DataContext is IBiosViewModel vm && vm.ShowDetailCommand.CanExecute(null))
      vm.ShowDetailCommand.Execute(null);
  }
}
