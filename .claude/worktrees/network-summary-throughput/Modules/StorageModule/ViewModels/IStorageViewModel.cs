using System.Collections.ObjectModel;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs;

namespace StorageModule.ViewModels;

/// <summary>Root view model bound to the storage summary tile and detail view: rolled-up totals,
/// live busiest-drive activity, the per-drive list, and the two navigation commands the shell wires to.</summary>
public interface IStorageViewModel {
  string TotalCapacityLabel { get; }
  string DriveCountLabel { get; }
  double Load { get; }
  ObservableCollection<StorageDriveViewModel> Drives { get; }

  /// <summary>Hands the summary tile's load-history graph to the VM so it can push samples.</summary>
  void AttachGraph(PerformanceGraph graph);

  ICommand ShowDetailCommand { get; }
  ICommand ShowDashboardCommand { get; }
}
