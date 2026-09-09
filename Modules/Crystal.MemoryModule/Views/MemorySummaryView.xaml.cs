using System.Windows.Controls;

namespace Crystal.MemoryModule.Views;

/// <summary>Memory dashboard tile, laid out like Windows Task Manager's Memory page: the usage and
/// commit-charge readouts plus composition bar and per-slot list on the left, the kernel-memory
/// stats and hardware specs on the right.</summary>
public partial class MemorySummaryView : UserControl {
  public MemorySummaryView() {
    InitializeComponent();
  }
}
