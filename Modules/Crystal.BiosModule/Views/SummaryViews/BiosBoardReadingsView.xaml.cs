using Crystal.BiosModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.BiosModule.Views.SummaryViews;

/// <summary>Live board telemetry (fan, board temp, CMOS battery, voltage rails) plus the static
/// firmware backfill shown when no sensors are present. Owns the summary tile's sparklines and wires
/// each graph's sample buffer to the root IBiosViewModel inherited from the host tile.</summary>
public partial class BiosBoardReadingsView : UserControl {
  public BiosBoardReadingsView() => InitializeComponent();

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
}
