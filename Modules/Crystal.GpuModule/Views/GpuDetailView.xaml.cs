using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Controls;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using Crystal.GpuModule.ViewModels;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views;

/// <summary>
/// Full-scale GPU view: one panel per adapter with static specs and live load,
/// temperature, clock and power history graphs. Reached by selecting the GPU summary tile;
/// the Back control returns to the dashboard.
/// </summary>
public partial class GpuDetailView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuDetailView"/> class.
  /// </summary>
  public GpuDetailView() {
    InitializeComponent();
  }

  /// <summary>
  /// Handles the Loaded event of the OnLoadGraph control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  private void OnLoadGraphLoaded(object sender, System.Windows.RoutedEventArgs e) =>
    Attach(sender, GraphThemes.Rose(GraphKind.SegmentedBar), (a, g) => a.AttachGraph("Gpu.Utilization", g));

  /// <summary>
  /// Handles the Loaded event of the OnTemperatureGraph control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  private void OnTemperatureGraphLoaded(object sender, System.Windows.RoutedEventArgs e) =>
    Attach(sender, GraphThemes.Amber(GraphKind.Line), (a, g) => a.AttachGraph("Gpu.Temperature", g));

  /// <summary>
  /// Handles the Loaded event of the OnClockGraph control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  private void OnClockGraphLoaded(object sender, System.Windows.RoutedEventArgs e) =>
    Attach(sender, GraphThemes.Sky(GraphKind.Line), (a, g) => a.AttachGraph("Gpu.Clock", g));
  
  /// <summary>
  /// Handles the Loaded event of the OnPowerGraph control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  private void OnPowerGraphLoaded(object sender, System.Windows.RoutedEventArgs e) =>
    Attach(sender, GraphThemes.Emerald(GraphKind.SegmentedBar), (a, g) => a.AttachGraph("Gpu.Power", g));

  /// <summary>
  /// Attaches the specified graph to the GPU adapter view model and applies the specified theme.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="theme">The theme to apply.</param>
  /// <param name="attach">The action to attach the graph.</param>
  private static void Attach(object sender, GraphTheme theme, Action<GpuAdapterViewModel, PerformanceGraph> attach) {
    if (sender is not PerformanceGraphView view) {
      return;
    }

    if (view.Graph is not { } graph) {
      return;
    }

    if (view.DataContext is not GpuAdapterViewModel adapter) {
      return;
    }
    graph.ApplyTheme(theme);
    attach(adapter, graph);
  }
}
