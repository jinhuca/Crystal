using System.Collections.ObjectModel;
using System.Windows.Input;
using Crystal.Controls.PerformanceGraphs;

namespace NetworkModule.ViewModels;

/// <summary>
/// Root view model bound to the network summary tile and detail view. The summary shows the
/// combined download/upload throughput across interfaces; the detail lists one
/// <see cref="NetworkAdapterViewModel"/> per connected interface. Also exposes the two navigation
/// commands the shell wires to.
/// </summary>
public interface INetworkViewModel {
  ObservableCollection<NetworkAdapterViewModel> Adapters { get; }

  /// <summary>Overall utilization (0-100%) across all interfaces, plotted on the summary graph.</summary>
  double Load { get; }

  /// <summary>Combined download throughput across all interfaces, shown on the summary tile.</summary>
  string DownloadLabel { get; }

  /// <summary>Combined upload throughput across all interfaces, shown on the summary tile.</summary>
  string UploadLabel { get; }

  /// <summary>Attaches the summary tile's history graph so overall load is plotted.</summary>
  void AttachGraph(PerformanceGraph graph);

  /// <summary>Raises <c>ShowDetailEvent</c> so the shell swaps in the network detail view.</summary>
  ICommand ShowDetailCommand { get; }

  /// <summary>Raises <c>ShowDashboardEvent</c> so the shell returns to the tile dashboard.</summary>
  ICommand ShowDashboardCommand { get; }
}
