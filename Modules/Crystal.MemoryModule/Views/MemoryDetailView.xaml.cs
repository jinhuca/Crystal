using System.Windows;
using System.Windows.Controls;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using Crystal.MemoryModule.ViewModels;

namespace Crystal.MemoryModule.Views;

/// <summary>Full-scale Memory view laid out like Windows Task Manager's Memory page: a 60-second
/// usage graph, an in-use composition bar, the kernel-memory stats grid, and the per-slot module
/// list. Reached by selecting the Memory summary tile; the Back control returns to the dashboard.</summary>
public partial class MemoryDetailView : UserControl {
  public MemoryDetailView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    // The wrapped PerformanceGraph is produced by the view's control template, so it isn't
    // available until after the template is applied (i.e. at Loaded, not ctor).
    if (DataContext is not IMemoryViewModel vm) return;
    if (UsageView.Graph is { } usage) {
      usage.ApplyTheme(GraphThemes.Sky(GraphKind.Line));
      vm.AttachUsageGraph(usage);
    }
    if (CommitView.Graph is { } commit) {
      commit.ApplyTheme(GraphThemes.Amber(GraphKind.Line));
      vm.AttachCommitGraph(commit);
    }
  }
}
