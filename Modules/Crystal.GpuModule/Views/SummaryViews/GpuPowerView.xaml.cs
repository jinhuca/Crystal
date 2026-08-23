using Crystal.Controls.PerformanceGraphs;
using Crystal.GpuModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Power metric tile for one GPU adapter: the power history graph plus the live power
/// readout. Binds to the GpuAdapterViewModel inherited from the per-adapter item and self-registers
/// its graph so the view model feeds it on each poll.
/// </summary>
public partial class GpuPowerView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuPowerView"/> class.
  /// </summary>
  public GpuPowerView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  /// <summary>
  /// Handles the Loaded event of the GpuPowerView control. When the control is loaded, it checks if the DataContext 
  /// is a GpuAdapterViewModel and if the PowerGraph has a valid ID. If both conditions are met, it attaches the graph 
  /// to the adapter view model.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is GpuAdapterViewModel adapter && GraphIdentity.GetId(PowerGraph) is { } id) {
      adapter.AttachGraph(id, PowerGraph);
    }
  }
}
