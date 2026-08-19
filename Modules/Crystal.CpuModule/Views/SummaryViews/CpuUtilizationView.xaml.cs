using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Utilization metric tile for the CPU summary: the utilization history graph plus the
/// live load readout and package C-state residency. Binds to the CPU SensorsViewModel inherited
/// from the host tile and self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class CpuUtilizationView : UserControl {
  public CpuUtilizationView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  /// <summary>
  /// Handles the Loaded event of the view. 
  /// If the DataContext is an ICpuSensorViewModel, it retrieves the graph ID from the 
  /// CpuUtilizationGraph and calls AttachGraph on the view model to register the graph for updates.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is ICpuSensorViewModel vm && GraphIdentity.GetId(CpuUtilizationGraph) is { } id) {
      vm.AttachGraph(id, CpuUtilizationGraph);
    }
  }
}
