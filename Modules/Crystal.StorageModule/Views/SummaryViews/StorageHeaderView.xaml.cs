using System.Windows.Controls;

namespace Crystal.StorageModule.Views.SummaryViews;

/// <summary>Header row for the storage summary tile: the STORAGE title plus the rolled-up total
/// capacity and drive count. Binds to the root IStorageViewModel inherited from the host tile.</summary>
public partial class StorageHeaderView : UserControl {
  public StorageHeaderView() => InitializeComponent();
}
