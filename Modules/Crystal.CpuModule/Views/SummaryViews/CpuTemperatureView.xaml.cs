using Crystal.Controls.PerformanceGraphs;
using Crystal.CpuModule.ViewModels.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.CpuModule.Views.SummaryViews;

/// <summary>
/// Temperature metric tile for the CPU summary: the temperature history graph plus the
/// live package temperature and thermal headroom (TjMax). Binds to the CPU SensorsViewModel
/// inherited from the host tile and self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class CpuTemperatureView : UserControl {
  public CpuTemperatureView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  /// <summary>
  /// Registers the graph with the view model so it can feed it on each poll.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The event arguments.</param>
  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is ICpuSensorViewModel vm && GraphIdentity.GetId(CpuTemperatureGraph) is { } id) {
      vm.AttachGraph(id, CpuTemperatureGraph);
    }
  }
}
