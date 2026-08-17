using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>Voltage metric tile for the CPU summary: the voltage history graph plus the live core
/// voltage and SoC voltage. Binds to the CPU SensorsViewModel inherited from the host tile and
/// self-registers its graph so the view model feeds it on each poll.</summary>
public partial class CpuVoltageView : UserControl {
  public CpuVoltageView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is ICpuSensorViewModel vm && GraphIdentity.GetId(CpuVoltageGraph) is { } id)
      vm.AttachGraph(id, CpuVoltageGraph);
  }
}
