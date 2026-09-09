using System.Windows.Controls;

namespace Crystal.StorageModule.Views;

/// <summary>Storage dashboard tile: the rolled-up header and disk selector on one line, and the
/// selected disk's identity, Active-time + transfer-rate readouts, capacity bar, stats grid and
/// endurance panel below. The system disk is selected by default; selecting a tab swaps the detail.</summary>
public partial class StorageSummaryView : UserControl {
  public StorageSummaryView() => InitializeComponent();
}
