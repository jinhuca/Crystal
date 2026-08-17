using System.Windows.Controls;

namespace Crystal.MemoryModule.Views.SummaryViews;

/// <summary>Header row for the Memory summary tile: title plus the inline capacity roll-up (total,
/// populated slots, max speed). Binds to the root IMemoryViewModel inherited from the host tile.</summary>
public partial class MemoryHeaderView : UserControl {
  public MemoryHeaderView() => InitializeComponent();
}
