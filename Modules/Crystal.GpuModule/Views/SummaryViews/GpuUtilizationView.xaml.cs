using Crystal.Controls.PerformanceGraphs;
using Crystal.GpuModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>Utilization metric tile for one GPU adapter: the core-load history graph plus the live
/// load readout. Binds to the GpuAdapterViewModel inherited from the per-adapter item and
/// self-registers its graph so the view model feeds it on each poll.</summary>
public partial class GpuUtilizationView : UserControl {
  public GpuUtilizationView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is GpuAdapterViewModel adapter && GraphIdentity.GetId(UtilizationGraph) is { } id)
      adapter.AttachGraph(id, UtilizationGraph);
  }
}
