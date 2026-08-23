using Crystal.Controls.PerformanceGraphs;
using Crystal.GpuModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Temperature metric tile for one GPU adapter: the temperature history graph plus the live
/// temperature readout. Binds to the GpuAdapterViewModel inherited from the per-adapter item and
/// self-registers its graph so the view model feeds it on each poll.
/// </summary>
public partial class GpuTemperatureView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuTemperatureView"/> class.
  /// </summary>
  public GpuTemperatureView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  /// <summary>
  /// Handles the Loaded event of the GpuTemperatureView control. When the control is loaded, 
  /// it checks if the DataContext is a GpuAdapterViewModel and if the TemperatureGraph has a valid ID. 
  /// If both conditions are met, it attaches the graph to the adapter's view model for live updates.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is GpuAdapterViewModel adapter && GraphIdentity.GetId(TemperatureGraph) is { } id) {
      adapter.AttachGraph(id, TemperatureGraph);
    }
  }
}
