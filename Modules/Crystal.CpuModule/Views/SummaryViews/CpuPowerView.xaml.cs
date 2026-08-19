using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Power metric tile for the CPU summary: the power history graph plus the live package power 
/// and its limits/currents (PL1/PL2 on Intel, TDC/EDC on AMD). Binds to the CPU SensorsViewModel 
/// inherited from the host tile and self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class CpuPowerView : UserControl {
  public CpuPowerView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  /// <summary>
  /// Registers the graph with the view model so it can feed it on each poll. The graph is registered 
  /// by its unique ID, which is set in XAML and retrieved here via the GraphIdentity attached property.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is ICpuSensorViewModel vm && GraphIdentity.GetId(CpuPowerGraph) is { } id) {
      vm.AttachGraph(id, CpuPowerGraph);
    }
  }
}
