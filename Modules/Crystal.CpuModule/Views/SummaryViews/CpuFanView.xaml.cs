using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// CPU fan metric tile for the CPU summary: the live fan readout (RPM, or PWM percentage on
/// tachometer-less laptops) over its history graph. Binds to the CPU SensorsViewModel inherited
/// from the host tile and self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class CpuFanView : UserControl {
  public CpuFanView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is ICpuSensorViewModel vm && GraphIdentity.GetId(FanGraph) is { } id) {
      vm.AttachGraph(id, FanGraph);
    }
  }
}
