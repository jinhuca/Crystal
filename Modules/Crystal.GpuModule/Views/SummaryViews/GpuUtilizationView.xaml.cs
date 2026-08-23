using Crystal.Controls.PerformanceGraphs;
using Crystal.GpuModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Utilization metric tile for one GPU adapter: the core-load history graph plus the live
/// load readout. Binds to the GpuAdapterViewModel inherited from the per-adapter item and
/// self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class GpuUtilizationView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuUtilizationView"/> class.
  /// </summary>
  public GpuUtilizationView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  /// <summary>
  /// Handles the Loaded event of the GpuUtilizationView control. When the control is loaded, 
  /// it checks if the DataContext is a GpuAdapterViewModel and if the UtilizationGraph has a valid ID. 
  /// If both conditions are met, it attaches the graph to the view model for live updates.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is GpuAdapterViewModel adapter && GraphIdentity.GetId(UtilizationGraph) is { } id) {
      adapter.AttachGraph(id, UtilizationGraph);
    }
  }
}
