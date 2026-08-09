using System.Collections.ObjectModel;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs;

namespace MemoryModule.ViewModels;

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
  string CachedLabel { get; }
  string PagedPoolLabel { get; }
  string NonPagedPoolLabel { get; }
  string SpeedLabel { get; }
  string SlotsUsedLabel { get; }
  string FormFactorLabel { get; }
  string HardwareReservedLabel { get; }

  /// <summary>"In use" fraction (0-1) of the composition bar.</summary>
  double CompositionInUseFraction { get; }
  /// <summary>Total installed GB the composition bar spans (for the tooltip/scale).</summary>
  double? CompositionTotalGB { get; }

  /// <summary>Hands the detail view's "Memory usage" history graph to the VM so it can push samples.</summary>
  void AttachUsageGraph(PerformanceGraph graph);

  /// <summary>Hands the summary tile's utilization-history graph to the VM so it can push samples.</summary>
  void AttachGraph(PerformanceGraph graph);

  /// <summary>Hands the summary tile's used-GB history graph to the VM so it can push samples.</summary>
  void AttachUsedGraph(PerformanceGraph graph);

  ICommand ShowDetailCommand { get; }
  ICommand ShowDashboardCommand { get; }
}
