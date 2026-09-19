using Crystal.Controls.PerformanceGraphs;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Crystal.StorageModule.ViewModels;

/// <summary>Root view model bound to the storage summary tile and detail view: rolled-up totals,
/// live busiest-drive activity, the per-drive list, and the two navigation commands the shell wires to.</summary>
public interface IStorageViewModel {
  string TotalCapacityLabel { get; }
  string DriveCountLabel { get; }
  double Load { get; }
  double TransferRateMBps { get; }
  double ReadRateMBps { get; }
  double WriteRateMBps { get; }
  double TransferMaxMBps { get; }
  string PeakTransferLabel { get; }
  bool HasCapacityData { get; }
  double UsedSpaceFraction { get; }
  double FreeSpaceFraction { get; }
  string CapacityUsageLabel { get; }
  string UsedSpacePercentLabel { get; }
  string FreeSpacePercentLabel { get; }
  string TotalSpaceLabel { get; }
  string FreeSpaceLabel { get; }
  string AvailablePercentLabel { get; }
  string DriveCountValue { get; }
  string DriveNoun { get; }
  bool ShowBusiestDrive { get; }
  string BusiestDriveLabel { get; }
  ObservableCollection<StorageDriveViewModel> Drives { get; }

  /// <summary>The disk whose graphs and stats the detail view currently shows.</summary>
  StorageDriveViewModel? SelectedDisk { get; set; }

  /// <summary>
  /// Registers a history graph to be fed on each update, keyed by its <c>GraphIdentity.Id</c>
  /// (e.g. "Storage.Activity", "Storage.Transfer"). Each metric sub-view self-registers its own
  /// graph on load, so the view model feeds only the graphs that were realized.
  /// </summary>
  void AttachGraph(string id, ISingleSeriesGraph graph);

  ICommand ShowDetailCommand { get; }
  ICommand ShowDashboardCommand { get; }
}
