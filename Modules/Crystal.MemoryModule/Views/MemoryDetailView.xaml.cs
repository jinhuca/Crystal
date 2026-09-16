using System.Windows.Controls;

namespace Crystal.MemoryModule.Views;

/// <summary>Full-scale memory view, laid out like Windows Task Manager's Memory page: the usage and
/// commit-charge readouts plus composition bar and per-slot list on the left, the kernel-memory
/// stats and hardware specs on the right. Reached by double-clicking the memory summary tile.</summary>
public partial class MemoryDetailView : UserControl {
  public MemoryDetailView() {
    InitializeComponent();
  }
}
