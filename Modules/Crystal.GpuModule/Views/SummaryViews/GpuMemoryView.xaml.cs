using Crystal.Controls.PerformanceGraphs;
using Crystal.GpuModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views.SummaryViews;

/// <summary>
/// Memory-used metric tile for one GPU adapter: the "used / total GB (percent)" readout over its
/// value-banded history graph driven by the used-percent. Binds to the GpuAdapterViewModel
/// inherited from the per-adapter block and self-registers its graph so the view model feeds it on
/// each poll.
/// </summary>
public partial class GpuMemoryView : UserControl {
  /// <summary>
  /// Initializes a new instance of the <see cref="GpuMemoryView"/> class.
  /// </summary>
  public GpuMemoryView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is GpuAdapterViewModel adapter && GraphIdentity.GetId(Graph) is { } id) {
      adapter.AttachGraph(id, Graph);
    }
  }
}
