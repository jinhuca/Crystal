using Crystal.MemoryModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.MemoryModule.Views;

/// <summary>Memory dashboard tile, laid out like Windows Task Manager's Memory page: the usage and
/// commit-charge history graphs plus composition bar and per-slot list on the left, the kernel-memory
/// stats and hardware specs on the right.</summary>
public partial class MemorySummaryView : UserControl {
  public MemorySummaryView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is not IMemoryViewModel vm) return;
    vm.AttachUsageGraph(UsageGraph);
    vm.AttachCommitGraph(CommitGraph);
  }
}
