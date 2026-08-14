using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Controls;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using Crystal.GpuModule.ViewModels;
using System;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views;

/// <summary>Full-scale GPU view: one panel per adapter with static specs and live load,
/// temperature, clock and power history graphs. Reached by selecting the GPU summary tile;
/// the Back control returns to the dashboard.</summary>
public partial class GpuDetailView : UserControl {
  public GpuDetailView() {
    InitializeComponent();
  }

  private void OnLoadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) =>
      Attach(sender, GraphThemes.Rose(GraphKind.SegmentedBar), (a, g) => a.AttachGraph(g));

  private void OnTemperatureGraphLoaded(object sender, System.Windows.RoutedEventArgs e) =>
      Attach(sender, GraphThemes.Amber(GraphKind.Line), (a, g) => a.AttachTemperatureGraph(g));

  private void OnClockGraphLoaded(object sender, System.Windows.RoutedEventArgs e) =>
      Attach(sender, GraphThemes.Sky(GraphKind.Line), (a, g) => a.AttachClockGraph(g));

  private void OnPowerGraphLoaded(object sender, System.Windows.RoutedEventArgs e) =>
      Attach(sender, GraphThemes.Emerald(GraphKind.SegmentedBar), (a, g) => a.AttachPowerGraph(g));

  private static void Attach(object sender, GraphTheme theme, Action<GpuAdapterViewModel, PerformanceGraph> attach) {
    if (sender is not PerformanceGraphView view) return;
    if (view.Graph is not { } graph) return;
    if (view.DataContext is not GpuAdapterViewModel adapter) return;
    graph.ApplyTheme(theme);
    attach(adapter, graph);
  }
}
