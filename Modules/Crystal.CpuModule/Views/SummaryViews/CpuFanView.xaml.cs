using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// CPU fan metric tile for the CPU summary: the fan history graph plus the live fan readout
/// (RPM, or PWM percentage on tachometer-less laptops). Binds to the CPU SensorsViewModel inherited
/// from the host tile and self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class CpuFanView : UserControl {
  public CpuFanView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  /// <summary>
  /// Handles the Loaded event of the UserControl. When the control is loaded, it checks if the DataContext
  /// is an ICpuSensorViewModel and if the CpuFanGraph has a valid ID. If both conditions are met, it 
  /// attaches the graph to the view model so that it can receive updates.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is ICpuSensorViewModel vm && GraphIdentity.GetId(CpuFanGraph) is { } id) {
      vm.AttachGraph(id, CpuFanGraph);
    }
  }
}
