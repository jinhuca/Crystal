using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Power metric tile for the CPU summary: the live package power and its limits/currents
/// (PL1/PL2 on Intel, TDC/EDC on AMD) over its history graph. Binds to the CPU SensorsViewModel
/// inherited from the host tile and self-registers its graph so the view model feeds it each poll.
/// </summary>
public partial class CpuPowerView : UserControl {
  public CpuPowerView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is ICpuSensorViewModel vm && GraphIdentity.GetId(PowerGraph) is { } id) {
      vm.AttachGraph(id, PowerGraph);
    }
  }
}
