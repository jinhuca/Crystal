using Crystal.Controls.PerformanceGraphs;
using Crystal.MemoryModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.MemoryModule.Views.SummaryViews;

/// <summary>Used-capacity metric tile for the Memory summary: the used-GB history graph (scaled to
/// total installed capacity) plus the live used-GB readout. Binds to the root IMemoryViewModel
/// inherited from the host tile and self-registers its graph so the view model feeds it.</summary>
public partial class MemoryUsedView : UserControl {
  public MemoryUsedView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is IMemoryViewModel vm && GraphIdentity.GetId(MemoryUsedGraph) is { } id)
      vm.AttachGraph(id, MemoryUsedGraph);
  }
}
