using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Temperature metric tile for the CPU summary: the live package temperature and thermal headroom
/// (TjMax). Binds to the CPU SensorsViewModel inherited from the host tile.
/// </summary>
public partial class CpuTemperatureView : UserControl {
  public CpuTemperatureView() {
    InitializeComponent();
  }

  /// <summary>
  /// Self-registers the bottom history strip with the inherited SensorsViewModel so it is fed on
  /// every poll, keyed by its <c>GraphIdentity.Id</c> ("Cpu.Temperature"). Mirrors the Power tile.
  /// </summary>
  private void OnGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is ISingleSeriesGraph graph
        && sender is FrameworkElement { DataContext: ICpuSensorViewModel vm } fe
        && GraphIdentity.GetId(fe) is { } id) {
      vm.AttachGraph(id, graph);
    }
  }
}
