using System.Windows.Controls;

namespace MemoryModule.Views;

/// <summary>Full-scale Memory view: rolled-up totals and a per-slot module list. Reached by
/// selecting the Memory summary tile; the Back control returns to the dashboard.</summary>
public partial class MemoryDetailView : UserControl {
  public MemoryDetailView() {
    InitializeComponent();
  }
}
