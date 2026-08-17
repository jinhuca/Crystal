using Crystal.Controls.PerformanceGraphs;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Crystal.MemoryModule.ViewModels;

/// <summary>Root view model bound to the memory summary tile and detail view: rolled-up totals,
/// live used-percentage load, the per-slot list, and the two navigation commands the shell wires to.</summary>
public interface IMemoryViewModel {
  string TotalCapacityLabel { get; }
  string SlotsLabel { get; }
  string MaxSpeedLabel { get; }
  double Load { get; }
  double? UsedGB { get; }
  double? TotalCapacityGB { get; }
  ObservableCollection<MemoryModuleViewModel> Modules { get; }

  // --- Task Manager-style header + stats grid (detail view) ---
  /// <summary>Header top-right, e.g. "32.0 GB DDR5".</summary>
  string HeaderSpecLabel { get; }
  /// <summary>Newest sampled usage in GB for the graph header, e.g. "31.6 GB".</summary>
  string UsageLabel { get; }
  string InUseLabel { get; }
  string AvailableLabel { get; }
  string CommittedLabel { get; }
  string CommitPeakLabel { get; }
  string CachedLabel { get; }
  string PagedPoolLabel { get; }
  string NonPagedPoolLabel { get; }
  string SpeedLabel { get; }
  string SlotsUsedLabel { get; }
  string FormFactorLabel { get; }
  string HardwareReservedLabel { get; }

  // --- Memory composition bar: four proportional segments plus an empty-track remainder ---
  /// <summary>"In use" fraction (0-1) of the composition bar.</summary>
  double CompositionInUseFraction { get; }
  double CompositionModifiedFraction { get; }
  double CompositionStandbyFraction { get; }
  double CompositionFreeFraction { get; }
  double CompositionRemainderFraction { get; }
  string CompositionInUseLabel { get; }
  string CompositionModifiedLabel { get; }
  string CompositionStandbyLabel { get; }
  string CompositionFreeLabel { get; }
  /// <summary>Total installed GB the composition bar spans (for the tooltip/scale).</summary>
  double? CompositionTotalGB { get; }

  /// <summary>Commit limit in GB (installed RAM + page file) — the commit graph's max scale.</summary>
  double? CommitLimitGB { get; }

  /// <summary>Hands the detail view's "Memory usage" history graph to the VM so it can push samples.</summary>
  void AttachUsageGraph(PerformanceGraph graph);

  /// <summary>Hands the detail view's "Commit charge" history graph to the VM so it can push samples.</summary>
  void AttachCommitGraph(PerformanceGraph graph);

  /// <summary>Registers a summary-tile history graph to be fed on each load update, keyed by its
  /// <c>GraphIdentity.Id</c> (e.g. "Memory.Utilization", "Memory.Used"). Each metric sub-view
  /// self-registers its own graph on load.</summary>
  void AttachGraph(string id, PerformanceGraph graph);

  ICommand ShowDetailCommand { get; }
  ICommand ShowDashboardCommand { get; }
}
