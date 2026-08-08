using System.Windows.Controls;
using Crystal.Controls.PerformanceGraphs.Controls;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using GpuModule.ViewModels;

namespace GpuModule.Views;

/// <summary>Full-scale GPU view: one panel per adapter with static specs and a live load
/// gauge + history graph. Reached by selecting the GPU summary tile; the Back control returns
/// to the dashboard.</summary>
public partial class GpuDetailView : UserControl {
  public GpuDetailView() {
    InitializeComponent();
  }

  private void OnGraphLoaded(object sender, System.Windows.RoutedEventArgs e) {
    if (sender is not PerformanceGraphView view) return;
    if (view.Graph is not { } graph) return;
    if (view.DataContext is not GpuAdapterViewModel adapter) return;
    graph.ApplyTheme(GraphThemes.Rose(GraphKind.SegmentedBar));
    adapter.AttachGraph(graph);
  }
}
