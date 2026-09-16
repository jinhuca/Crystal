using System.Windows;
using System.Windows.Markup;

namespace Crystal.GpuModule.Views.SummaryViews;

//internal sealed class GraphWidthExtension : MarkupExtension {
//  public override object ProvideValue(IServiceProvider serviceProvider) =>
//      GpuSummaryViewConstants.GraphWidth;
//}

internal static class GpuSummaryViewConstants {
  public const double GraphWidth = 200d;
  public const double UtilizationGraphWidth = 260d;

  // The Utilization graph+readout is pinned to an exact width — the tiles live in a horizontal strip
  // that measures children with infinite width, where an unbounded stretch-to-fill graph balloons to
  // Capacity×pitch instead. The readout is a full GraphWidth tile on the right, so the graph fills the
  // remaining width. For the dedicated tile that remainder is cols 0-6 (7 tiles + their 7 gaps = 1456)
  // and the readout tile lands exactly under PCIe Tx (col 7), leaving col 8 (under GPU Fan) for Voltage.
  public const double IntegratedUtilizationWidth = (3 * GraphWidth) + (2 * 8d);
  public const double DedicatedUtilizationWidth = (7 * GraphWidth) + (7 * 8d) + GraphWidth;
}
