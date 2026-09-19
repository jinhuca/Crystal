using Crystal.Controls.PerformanceGraphs;
using Crystal.GpuModule.ViewModels;
using System.Windows;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Shared self-registration for the per-tile history graphs: on Loaded, an
/// <see cref="ISingleSeriesGraph"/> (each tile's <see cref="PerformanceGraph"/>) hands itself to the
/// inherited <see cref="GpuAdapterViewModel"/> under its <c>GraphIdentity.Id</c> so the VM feeds it
/// each poll (mirrors the CPU summary tiles).
/// </summary>
internal static class GpuGraphAttach {
  public static void Attach(object sender) {
    if (sender is ISingleSeriesGraph graph
        && sender is FrameworkElement { DataContext: GpuAdapterViewModel vm } fe
        && GraphIdentity.GetId(fe) is { } id) {
      vm.AttachGraph(id, graph);
    }
  }
}
