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
  bool ShowBusiestDrive { get; }
  string BusiestDriveLabel { get; }
  ObservableCollection<StorageDriveViewModel> Drives { get; }

  /// <summary>The disk whose graphs and stats the detail view currently shows.</summary>
  StorageDriveViewModel? SelectedDisk { get; set; }

  /// <summary>Hands the summary tile's activity-history graph to the VM so it can push samples.</summary>
  void AttachGraph(PerformanceGraph graph);

  /// <summary>Hands the summary tile's transfer-rate history graph to the VM so it can push samples.</summary>
  void AttachTransferGraph(PerformanceGraph graph);

  ICommand ShowDetailCommand { get; }
  ICommand ShowDashboardCommand { get; }
}
