using Crystal.Controls.PerformanceGraphs;
using Crystal.GpuModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Clock metric tile for one GPU adapter: the core-clock history graph plus the live clock
/// readout. Binds to the GpuAdapterViewModel inherited from the per-adapter item and self-registers
/// its graph so the view model feeds it on each poll.
/// </summary>
public partial class GpuClockView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuClockView"/> class.
  /// </summary>
  public GpuClockView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  /// <summary>
  /// Handles the Loaded event of the GpuClockView control. When the control is loaded, 
  /// it checks if the DataContext is a GpuAdapterViewModel and if the ClockGraph has a valid ID. 
  /// If both conditions are met, it attaches the graph to the adapter view model.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is GpuAdapterViewModel adapter && GraphIdentity.GetId(ClockGraph) is { } id) {
      adapter.AttachGraph(id, ClockGraph);
    }
  }
}
