using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Power metric tile for the CPU summary: the live package power and its limits/currents
/// (PL1/PL2 on Intel, TDC/EDC on AMD). Binds to the CPU SensorsViewModel inherited from the host tile.
/// </summary>
public partial class CpuPowerView : UserControl {
  public CpuPowerView() {
    InitializeComponent();
  }

  /// <summary>
  /// Self-registers the bottom power-history strip with the inherited SensorsViewModel so it is fed
  /// on every poll, keyed by its <c>GraphIdentity.Id</c> ("Cpu.Power"). Mirrors the detail view's
  /// history graphs; the feed registry fans each sample out to every graph sharing the key.
  /// </summary>
  private void OnGraphLoaded(object sender, RoutedEventArgs e) {
    if (sender is ISingleSeriesGraph graph
        && sender is FrameworkElement { DataContext: ICpuSensorViewModel vm } fe
        && GraphIdentity.GetId(fe) is { } id) {
      vm.AttachGraph(id, graph);
    }
  }
}
