using Crystal.Controls.PerformanceGraphs;
using Crystal.MemoryModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.MemoryModule.Views.SummaryViews;

/// <summary>Utilization metric tile for the Memory summary: the utilization history graph plus the
/// live used-percentage readout. Binds to the root IMemoryViewModel inherited from the host tile
/// and self-registers its graph so the view model feeds it on each load update.</summary>
public partial class MemoryUtilizationView : UserControl {
  public MemoryUtilizationView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is IMemoryViewModel vm && GraphIdentity.GetId(MemoryUtilizationGraph) is { } id)
      vm.AttachGraph(id, MemoryUtilizationGraph);
  }
}
