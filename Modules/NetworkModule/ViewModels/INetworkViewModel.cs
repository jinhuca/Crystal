using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NetworkModule.ViewModels;

/// <summary>
/// Root view model bound to the network summary tile and detail view. The summary shows the
/// total download/upload throughput across all interfaces; the detail lists one
/// <see cref="NetworkAdapterViewModel"/> per connected interface. Also exposes the two navigation
/// commands the shell wires to.
/// </summary>
public interface INetworkViewModel {
  ObservableCollection<NetworkAdapterViewModel> Adapters { get; }

  /// <summary>Total download throughput across all interfaces, shown on the summary tile.</summary>
  string DownloadLabel { get; }

  /// <summary>Total upload throughput across all interfaces, shown on the summary tile.</summary>
  string UploadLabel { get; }

  /// <summary>Raises <c>ShowDetailEvent</c> so the shell swaps in the network detail view.</summary>
  ICommand ShowDetailCommand { get; }

  /// <summary>Raises <c>ShowDashboardEvent</c> so the shell returns to the tile dashboard.</summary>
  ICommand ShowDashboardCommand { get; }
}
