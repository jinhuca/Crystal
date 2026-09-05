using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Clock metric tile for the CPU summary: the live package clock, effective clock and bus speed
/// over its history graph. Binds to the CPU SensorsViewModel inherited from the host tile and
/// self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class CpuClockView : UserControl {
  public CpuClockView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is ICpuSensorViewModel vm && GraphIdentity.GetId(ClockGraph) is { } id) {
      vm.AttachGraph(id, ClockGraph);
    }
  }
}
