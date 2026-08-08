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
  ObservableCollection<MemoryModuleViewModel> SummaryModules { get; }

  /// <summary>Hands the summary tile's utilization-history graph to the VM so it can push samples.</summary>
  void AttachGraph(PerformanceGraph graph);

  /// <summary>Hands the summary tile's used-GB history graph to the VM so it can push samples.</summary>
  void AttachUsedGraph(PerformanceGraph graph);

  ICommand ShowDetailCommand { get; }
  ICommand ShowDashboardCommand { get; }
}
