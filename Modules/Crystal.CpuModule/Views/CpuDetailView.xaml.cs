using System.Windows.Controls;

namespace Crystal.CpuModule.Views;

/// <summary>
/// Full-scale CPU view: static specs, the metric history graphs (relocated from the summary
/// tiles), the per-core clocks table, and the instruction-set grid. Reached by selecting the CPU
/// summary tile on the dashboard. Composed from per-section sub-views in Views/DetailViews.
/// </summary>
public partial class CpuDetailView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="CpuDetailView"/> class.
  /// </summary>
  public CpuDetailView() => InitializeComponent();
}
