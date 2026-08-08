using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
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

  /// <summary>Per-process network top-talkers for the detail view, ranked by current throughput.</summary>
  ObservableCollection<ProcessNetworkRowViewModel> TopTalkers { get; }

  /// <summary>The top few talkers (rate descending) for the compact summary tile.</summary>
  ObservableCollection<ProcessNetworkRowViewModel> SummaryTopTalkers { get; }

  /// <summary>Sorted view over <see cref="TopTalkers"/> the table binds to; defaults to throughput
  /// descending, re-sorted when a column header is clicked.</summary>
  ListCollectionView TopTalkersView { get; }

  /// <summary>Row-VM property the top-talkers table is currently sorted by.</summary>
  string TopTalkersSortProperty { get; }

  /// <summary>Direction of the current top-talkers sort.</summary>
  ListSortDirection TopTalkersSortDirection { get; }

  /// <summary>Re-sort the top-talkers table by the given row-VM property (toggles direction on a
  /// repeat click of the same column).</summary>
  void SortTopTalkersBy(string propertyName);

  /// <summary>True when the per-process network table has a reason to show instead of rows (ETW not
  /// running — typically not elevated).</summary>
  bool HasTopTalkersStatus { get; }

  /// <summary>Explains a blank top-talkers table (e.g. "Per-process network needs elevation").</summary>
  string TopTalkersStatusLabel { get; }

  /// <summary>Total download throughput across all interfaces, shown on the summary tile.</summary>
  string DownloadLabel { get; }

  /// <summary>Total upload throughput across all interfaces, shown on the summary tile.</summary>
  string UploadLabel { get; }

  /// <summary>True when a Wi-Fi adapter is connected; drives the summary tile's Wi-Fi row.</summary>
  bool HasWifi { get; }

  /// <summary>True when a wireless radio exists but isn't connected (off or unassociated); drives a
  /// muted status row shown in place of the connected Wi-Fi block.</summary>
  bool HasWifiStatus { get; }

  /// <summary>Muted status text for a present-but-not-connected radio ("Wi-Fi disabled" /
  /// "Wi-Fi disconnected"). Empty when a radio is connected or none exists.</summary>
  string WifiStatusLabel { get; }

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
