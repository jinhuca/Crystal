using System.Windows;
using System.Windows.Controls;
using BiosModule.ViewModels;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;

namespace BiosModule.Views;

/// <summary>Full-scale BIOS view: complete firmware identity. Reached by selecting the BIOS
/// summary strip; the Back control returns to the dashboard.</summary>
public partial class BiosDetailView : UserControl {
  public BiosDetailView() {
    InitializeComponent();
  }

  // Per-graph rather than a root Loaded handler: a root-element Loaded collides with the
  // BiosModule namespace/class name in WPF's generated code. The VM attach is idempotent.
  private void OnGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is PerformanceGraph graph)
      graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    if (DataContext is IBiosViewModel vm)
      vm.AttachRailGraphs(Rail3V3Graph, Rail5VGraph, Rail12VGraph);
  }

  private void OnFanGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is PerformanceGraph graph)
      graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    if (DataContext is IBiosViewModel vm)
      vm.AttachFanGraph(FanGraph);
  }

  private void OnBoardTempGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is PerformanceGraph graph)
      graph.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
    if (DataContext is IBiosViewModel vm)
      vm.AttachBoardTempGraph(BoardTempGraph);
  }
}
