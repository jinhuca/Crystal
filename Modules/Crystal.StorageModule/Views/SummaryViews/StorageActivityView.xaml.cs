using Crystal.Controls.PerformanceGraphs;
using Crystal.StorageModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.StorageModule.Views.SummaryViews;

/// <summary>Active-time metric tile for the storage summary: the activity history graph plus the
/// live busiest-disk active-time percentage. Binds to the root IStorageViewModel inherited from the
/// host tile and self-registers its graph so the view model feeds it on each update.</summary>
public partial class StorageActivityView : UserControl {
  public StorageActivityView() {
    InitializeComponent();
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (DataContext is IStorageViewModel vm && GraphIdentity.GetId(ActivityGraph) is { } id)
      vm.AttachGraph(id, ActivityGraph);
  }
}
