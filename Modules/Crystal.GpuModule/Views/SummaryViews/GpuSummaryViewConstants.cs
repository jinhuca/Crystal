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
}
