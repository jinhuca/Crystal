using System.Windows.Controls;

namespace Crystal.CpuModule.Views.DetailViews;

/// <summary>
/// Alert banners for the CPU detail view: MSR driver unavailable and active throttling, each shown
/// only while its condition holds.
/// </summary>
public partial class CpuStatusBannersView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="CpuStatusBannersView"/> class.
  /// </summary>
  public CpuStatusBannersView() => InitializeComponent();
}
