using System.Windows.Controls;

namespace StorageModule.Views;

/// <summary>Full-scale Storage view: rolled-up totals and a per-drive list. Reached by
/// selecting the Storage summary tile; the Back control returns to the dashboard.</summary>
public partial class StorageDetailView : UserControl {
  public StorageDetailView() {
    InitializeComponent();
  }
}
