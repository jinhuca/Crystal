using Crystal.Controls.PerformanceGraphs;
using Crystal.StorageModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.StorageModule.Views.SummaryViews;

/// <summary>Transfer-rate metric tile for the storage summary: the transfer history graph plus the
/// live system-wide transfer rate, read/write split and session peak. Binds to the root
/// IStorageViewModel inherited from the host tile and self-registers its graph so the view model
/// feeds it on each update.</summary>
public partial class StorageTransferView : UserControl {
  public StorageTransferView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is IStorageViewModel vm && GraphIdentity.GetId(TransferGraph) is { } id)
      vm.AttachGraph(id, TransferGraph);
  }
}
