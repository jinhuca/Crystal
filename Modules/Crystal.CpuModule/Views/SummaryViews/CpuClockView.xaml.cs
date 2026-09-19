using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Clock metric tile for the CPU summary: the live package clock, effective clock and bus speed.
/// Binds to the CPU SensorsViewModel inherited from the host tile.
/// </summary>
public partial class CpuClockView : UserControl {
  public CpuClockView() {
    InitializeComponent();
  }

  /// <summary>
  /// Self-registers the bottom history strip with the inherited SensorsViewModel so it is fed on
  /// every poll, keyed by its <c>GraphIdentity.Id</c> ("Cpu.Clock"). Mirrors the Power tile.
  /// </summary>
  private void OnGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is ISingleSeriesGraph graph
        && sender is FrameworkElement { DataContext: ICpuSensorViewModel vm } fe
        && GraphIdentity.GetId(fe) is { } id) {
      vm.AttachGraph(id, graph);
    }
  }
}
