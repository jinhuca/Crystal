using System.Windows;
using System.Windows.Markup;

namespace Crystal.GpuModule.Views.SummaryViews;

//internal sealed class GraphWidthExtension : MarkupExtension {
//  public override object ProvideValue(IServiceProvider serviceProvider) =>
//      GpuSummaryViewConstants.GraphWidth;
//}

internal static class GpuSummaryViewConstants {
  public const double GraphWidth = 210d;
  public const double UtilizationGraphWidth = 260d;

  // The full-width Utilization graph spans the metric row, so it must be pinned to the exact width of
  // that row — the tiles live in a horizontal strip that measures children with infinite width, where
  // an unbounded stretch-to-fill graph balloons to Capacity×pitch instead. Width = (metric graphs) +
  // (8px inter-column gaps): 3 graphs / 2 gaps for the integrated tile, 5 graphs / 4 gaps for the
  // dedicated tile.
  public const double IntegratedUtilizationWidth = (3 * GraphWidth) + (2 * 8d);
  public const double DedicatedUtilizationWidth = (5 * GraphWidth) + (4 * 8d);
}
