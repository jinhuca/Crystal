using Crystal.Controls.Metrics;
using Crystal.Controls.PerformanceGraphs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Crystal.NetworkModule.ViewModels;

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

  /// <summary>Shared upper bound (bytes/sec) for the summary download/upload sparklines, tracking the
  /// recent busiest sample across both so the two graphs stay on a common, comparable scale.</summary>
  double ThroughputMaxBytesPerSecond { get; }

  /// <summary>Session min/avg/max and trend of the total download rate (KiB/s), for the tile's caret stat line.</summary>
  MetricRowViewModel DownloadRow { get; }

  /// <summary>Session min/avg/max and trend of the total upload rate (KiB/s), for the tile's caret stat line.</summary>
  MetricRowViewModel UploadRow { get; }

  /// <summary>
  /// Registers a history graph to be fed on each update, keyed by its <c>GraphIdentity.Id</c>
  /// (e.g. "Network.Download" / "Network.Upload"). The throughput sub-view self-registers its
  /// sparklines on load, so the view model feeds only the graphs a consumer chose to realize.
  /// </summary>
  void AttachGraph(string id, ISingleSeriesGraph graph);

  /// <summary>True when the primary connection is a connected Wi-Fi adapter; gates the Wi-Fi-only
  /// detail rows (SSID, connection type, signal strength) on the summary tile.</summary>
  bool HasWifi { get; }

  /// <summary>True when a wireless radio exists but isn't connected (off or unassociated) and no other
  /// interface is carrying the connection; drives a muted status row shown in place of the detail block.</summary>
  bool HasWifiStatus { get; }

  /// <summary>Muted status text for a present-but-not-connected radio ("Wi-Fi disabled" /
  /// "Wi-Fi disconnected"). Empty when a connection is shown or no radio exists.</summary>
  string WifiStatusLabel { get; }

  /// <summary>True when a primary interface is connected; drives the TaskManager-style detail block
  /// (adapter name, SSID, DNS, connection type, addresses, signal) on the summary tile.</summary>
  bool HasConnection { get; }

  /// <summary>Friendly name of the primary connection's adapter (e.g. "Wi-Fi", "Ethernet").</summary>
  string ConnectionAdapterName { get; }

  /// <summary>SSID of the primary connection when it is Wi-Fi; "—" otherwise.</summary>
  string ConnectionSsid { get; }

  /// <summary>Connection-specific DNS suffix of the primary connection (e.g. "Home"); "—" when none.</summary>
  string ConnectionDnsName { get; }

  /// <summary>802.11 PHY type of the primary connection when it is Wi-Fi (e.g. "802.11ac"); "—" otherwise.</summary>
  string ConnectionType { get; }

  /// <summary>Primary IPv4 address of the primary connection; "—" when none.</summary>
  string ConnectionIPv4 { get; }

  /// <summary>Primary IPv6 address of the primary connection; "—" when none.</summary>
  string ConnectionIPv6 { get; }

  /// <summary>Four-segment bar glyph for the primary Wi-Fi connection's signal strength (e.g. "▂▄▆▁");
  /// "—" when the primary connection isn't Wi-Fi.</summary>
  string SignalBars { get; }

  /// <summary>Raises <c>ShowDetailEvent</c> so the shell swaps in the network detail view.</summary>
  ICommand ShowDetailCommand { get; }

  /// <summary>Raises <c>ShowDashboardEvent</c> so the shell returns to the tile dashboard.</summary>
  ICommand ShowDashboardCommand { get; }
}
