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

  /// <summary>True when a Wi-Fi adapter is connected; drives the summary tile's Wi-Fi row.</summary>
  bool HasWifi { get; }

  /// <summary>SSID + signal of the connected Wi-Fi adapter (strongest, if several), for the tile.</summary>
  string WifiLabel { get; }

  /// <summary>Negotiated Rx/Tx link rate of the summary Wi-Fi adapter (e.g. "866 / 866 Mbps").</summary>
  string WifiLinkRate { get; }

  /// <summary>Security suite of the summary Wi-Fi adapter (e.g. "WPA2-Personal / CCMP").</summary>
  string WifiSecurity { get; }

  /// <summary>BSSID (AP MAC) of the summary Wi-Fi adapter.</summary>
  string WifiBssid { get; }

  /// <summary>Raises <c>ShowDetailEvent</c> so the shell swaps in the network detail view.</summary>
  ICommand ShowDetailCommand { get; }

  /// <summary>Raises <c>ShowDashboardEvent</c> so the shell returns to the tile dashboard.</summary>
  ICommand ShowDashboardCommand { get; }
}
