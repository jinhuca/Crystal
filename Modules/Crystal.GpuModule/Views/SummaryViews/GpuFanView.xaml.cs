using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Fan-speed metric tile for one GPU adapter: the live RPM readout over its history graph. Binds to
/// the GpuAdapterViewModel inherited from the per-adapter block and self-registers its graph so the
/// view model feeds it on each poll.
/// </summary>
public partial class GpuFanView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuFanView"/> class.
  /// </summary>
  public GpuFanView() {
    InitializeComponent();
  }

  private void OnGraphLoaded(object sender, RoutedEventArgs e) => GpuGraphAttach.Attach(sender);
}
