using Crystal.Controls.Metrics;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.Threading;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.NetworkModule.Models;
using Crystal.Service.Network;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Crystal.NetworkModule.ViewModels;

public sealed class NetworkViewModel : BindableBase, INetworkViewModel, IDisposable {
  private readonly IDisposable _sensorsSubscription;
  private readonly IDisposable _topTalkersSubscription;
  private readonly UiThreadMarshaller _ui = new();
  private readonly Dictionary<uint, ProcessNetworkRowViewModel> _talkersByPid = new();
  private string _downloadLabel = "—";
  private string _uploadLabel = "—";
  // History graphs are registered by their GraphIdentity.Id as the throughput sub-view loads, then
  // fed by that same id in Apply(). A consumer that realizes only some graphs feeds only those.
  private readonly GraphFeedRegistry _graphs = new();
  private double _throughputMax = ThroughputFloorBytesPerSecond;
  private bool _hasWifi;
  private bool _hasWifiStatus;
  private string _wifiStatusLabel = "";
  private bool _hasConnection;
  // Sticky identity of the non-Wi-Fi primary connection. Selecting purely by instantaneous throughput
  // makes the detail block thrash as the busiest NIC bounces poll-to-poll (virtual/filter adapters);
  // we hold onto the chosen interface while it stays present so the rows stop flickering to "—".
  private string? _primaryName;
  private string _connectionAdapterName = "—";
  private string _connectionSsid = "—";
  private string _connectionDnsName = "—";
  private string _connectionType = "—";
  private string _connectionIPv4 = "—";
  private string _connectionIPv6 = "—";
  private string _signalBars = "—";
  private bool _hasTopTalkersStatus;
  private string _topTalkersStatusLabel = "";
  private string _topTalkersSortProperty = nameof(ProcessNetworkRowViewModel.RateBytesPerSecond);
  private ListSortDirection _topTalkersSortDirection = ListSortDirection.Descending;

  // How many talkers the compact summary tile shows (the detail view shows all of them).
  private const int SummaryTopCount = 3;

  // Shared throughput-sparkline scaling: track the busiest sample (download or upload) over a short
  // window so both graphs share a Y-axis; the floor keeps an idle link from magnifying noise.
  private const double ThroughputFloorBytesPerSecond = 128 * 1024;
  private const int ThroughputWindow = 60;
  private readonly Queue<double> _throughputSamples = new();

  public NetworkViewModel(INetworkModel model, IEventAggregator events) {
    ShowDetailCommand = new DelegateCommand(
        () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Network));
    ShowDashboardCommand = new DelegateCommand(
        () => events.GetEvent<ShowDashboardEvent>().Publish());

    TopTalkersView = new ListCollectionView(TopTalkers);
    ApplyTopTalkersSort();

    _sensorsSubscription = model.Sensors.Subscribe(s => OnUi(() => Apply(s)));
    _topTalkersSubscription = model.TopTalkers.Subscribe(s => OnUi(() => ApplyTopTalkers(s)));
  }

  public ObservableCollection<NetworkAdapterViewModel> Adapters { get; } = [];
  public ObservableCollection<ProcessNetworkRowViewModel> TopTalkers { get; } = [];

  /// <summary>The top few talkers (rate descending) shown compactly on the summary tile — a capped
  /// slice of the full ranking, sharing the same row VMs so the two views stay in sync.</summary>
  public ObservableCollection<ProcessNetworkRowViewModel> SummaryTopTalkers { get; } = [];

  /// <summary>Sorted view over <see cref="TopTalkers"/>; this is what the table binds to. Defaults to
  /// throughput descending; clicking a column header re-sorts.</summary>
  public ListCollectionView TopTalkersView { get; }

  public string TopTalkersSortProperty => _topTalkersSortProperty;
  public ListSortDirection TopTalkersSortDirection => _topTalkersSortDirection;

  public bool HasTopTalkersStatus { get => _hasTopTalkersStatus; private set => SetProperty(ref _hasTopTalkersStatus, value); }
  public string TopTalkersStatusLabel { get => _topTalkersStatusLabel; private set => SetProperty(ref _topTalkersStatusLabel, value); }
  public string DownloadLabel { get => _downloadLabel; private set => SetProperty(ref _downloadLabel, value); }
  public string UploadLabel { get => _uploadLabel; private set => SetProperty(ref _uploadLabel, value); }
  public double ThroughputMaxBytesPerSecond { get => _throughputMax; private set => SetProperty(ref _throughputMax, value); }
  /// <summary>Session min/avg/max and trend of the total download rate (in KiB/s), for the tile's stat line.</summary>
  public MetricRowViewModel DownloadRow { get; } = new("Download");
  /// <summary>Session min/avg/max and trend of the total upload rate (in KiB/s), for the tile's stat line.</summary>
  public MetricRowViewModel UploadRow { get; } = new("Upload");
  public bool HasWifi { get => _hasWifi; private set => SetProperty(ref _hasWifi, value); }
  public bool HasWifiStatus { get => _hasWifiStatus; private set => SetProperty(ref _hasWifiStatus, value); }
  public string WifiStatusLabel { get => _wifiStatusLabel; private set => SetProperty(ref _wifiStatusLabel, value); }
  public bool HasConnection { get => _hasConnection; private set => SetProperty(ref _hasConnection, value); }
  public string ConnectionAdapterName { get => _connectionAdapterName; private set => SetProperty(ref _connectionAdapterName, value); }
  public string ConnectionSsid { get => _connectionSsid; private set => SetProperty(ref _connectionSsid, value); }
  public string ConnectionDnsName { get => _connectionDnsName; private set => SetProperty(ref _connectionDnsName, value); }
  public string ConnectionType {
    get => _connectionType;
    private set => SetProperty(ref _connectionType, value);
  }
  public string ConnectionIPv4 { get => _connectionIPv4; private set => SetProperty(ref _connectionIPv4, value); }
  public string ConnectionIPv6 { get => _connectionIPv6; private set => SetProperty(ref _connectionIPv6, value); }
  public string SignalBars { get => _signalBars; private set => SetProperty(ref _signalBars, value); }
  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  public void AttachGraph(string id, ISingleSeriesGraph graph) => _graphs.Attach(id, graph);

  private void FeedGraph(string id, double value) => _graphs.Feed(id, value);

  private void Apply(NetworkSnapshot snapshot) {
    // Reconcile the adapter list against the current interfaces (they can come and go as NICs
    // connect/disconnect), keyed by name.
    SyncAdapters(snapshot.Interfaces);

    var totalDownload = 0.0;
    var totalUpload = 0.0;
    NetworkInterfaceReading? busiest = null;
    var busiestRate = -1.0;
    foreach (var reading in snapshot.Interfaces) {
      var adapter = Adapters.FirstOrDefault(a =>
          string.Equals(a.Name, reading.Name, StringComparison.OrdinalIgnoreCase));
      adapter?.Update(reading);
      totalDownload += reading.DownloadBytesPerSecond;
      totalUpload += reading.UploadBytesPerSecond;

      // Track the interface moving the most traffic this poll. Strict > keeps the first interface
      // when everything is idle (all zero), giving a stable fallback for a single-NIC machine.
      var rate = reading.DownloadBytesPerSecond + reading.UploadBytesPerSecond;
      if (rate > busiestRate) {
        busiestRate = rate;
        busiest = reading;
      }
    }

    DownloadLabel = FormatSpeed(totalDownload);
    UploadLabel = FormatSpeed(totalUpload);
    // Feed the stat-line rows in KiB/s so the min/avg/max carets read in a single, stable unit
    // (the big value auto-scales its unit; the caret line stays comparable across polls).
    DownloadRow.Update(totalDownload / 1024d);
    UploadRow.Update(totalUpload / 1024d);

    FeedGraph("Network.Download", totalDownload);
    FeedGraph("Network.Upload", totalUpload);
    _throughputSamples.Enqueue(Math.Max(totalDownload, totalUpload));
    while (_throughputSamples.Count > ThroughputWindow) _throughputSamples.Dequeue();
    ThroughputMaxBytesPerSecond = NiceCeiling(Math.Max(ThroughputFloorBytesPerSecond, _throughputSamples.Max()));

    ApplyConnectionSummary(snapshot.Interfaces, snapshot.WifiStatus, busiest);
  }

  // Reconcile the ranked top-talkers into a PID-keyed collection: update surviving rows in place,
  // add new PIDs, drop the ones that fell out of the ranking, and reorder to match the new ranking
  // (the source already sorted by throughput descending). When ETW isn't running the list is empty
  // and a status line explains why instead of showing a silently blank table.
  private void ApplyTopTalkers(ProcessNetworkSnapshot snapshot) {
    HasTopTalkersStatus = !snapshot.IsRunning;
    TopTalkersStatusLabel = snapshot.IsRunning
        ? ""
        : $"Per-process network needs elevation ({snapshot.StatusError ?? "ETW session not running"})";

    // Reconcile into the PID-keyed collection: update surviving rows in place, add new PIDs, drop the
    // ones that fell out of the ranking. Row order in the backing collection doesn't matter — the
    // ListCollectionView sorts it by the user-chosen column (rate descending by default).
    var live = new HashSet<uint>(snapshot.TopTalkers.Count);
    foreach (var reading in snapshot.TopTalkers) {
      live.Add(reading.ProcessId);
      if (!_talkersByPid.TryGetValue(reading.ProcessId, out var row)) {
        row = new ProcessNetworkRowViewModel(reading.ProcessId);
        _talkersByPid[reading.ProcessId] = row;
        TopTalkers.Add(row);
      }
      row.Update(reading);
    }

    for (var i = TopTalkers.Count - 1; i >= 0; i--) {
      if (!live.Contains(TopTalkers[i].ProcessId)) {
        _talkersByPid.Remove(TopTalkers[i].ProcessId);
        TopTalkers.RemoveAt(i);
      }
    }

    // Live rate values changed in place, so re-sort this poll.
    TopTalkersView.Refresh();

    // Mirror the top few (already rate-descending from the source) into the compact summary list,
    // reusing the same row VMs so both views reflect the same live values.
    SummaryTopTalkers.Clear();
    foreach (var reading in snapshot.TopTalkers.Take(SummaryTopCount))
      SummaryTopTalkers.Add(_talkersByPid[reading.ProcessId]);
  }

  /// <summary>
  /// Sort the top-talkers table by <paramref name="propertyName"/>. Clicking the active column flips
  /// the direction; clicking a new column starts descending (the useful default for rates), except
  /// Name which starts ascending.
  /// </summary>
  public void SortTopTalkersBy(string propertyName) {
    if (_topTalkersSortProperty == propertyName) {
      _topTalkersSortDirection = _topTalkersSortDirection == ListSortDirection.Ascending
          ? ListSortDirection.Descending
          : ListSortDirection.Ascending;
    }
    else {
      _topTalkersSortProperty = propertyName;
      _topTalkersSortDirection = propertyName == nameof(ProcessNetworkRowViewModel.Name)
          ? ListSortDirection.Ascending
          : ListSortDirection.Descending;
    }
    ApplyTopTalkersSort();
    RaisePropertyChanged(nameof(TopTalkersSortProperty));
    RaisePropertyChanged(nameof(TopTalkersSortDirection));
  }

  private void ApplyTopTalkersSort() {
    using (TopTalkersView.DeferRefresh()) {
      TopTalkersView.SortDescriptions.Clear();
      TopTalkersView.SortDescriptions.Add(new SortDescription(_topTalkersSortProperty, _topTalkersSortDirection));
    }
  }

  // Choose the primary connection to detail on the tile and populate the TaskManager-style rows.
  // Prefer the strongest connected Wi-Fi adapter (a machine can have several radios); otherwise pick a
  // stable non-Wi-Fi connection so the throughput has a visible owner. With nothing connected at all,
  // show a muted status row driven by the machine Wi-Fi state.
  private void ApplyConnectionSummary(
      IReadOnlyList<NetworkInterfaceReading> interfaces, WifiStatus status, NetworkInterfaceReading? busiest) {
    NetworkInterfaceReading? bestWifi = null;
    foreach (var reading in interfaces) {
      if (reading.WifiSignalPercent is null && reading.WifiSsid is null) continue;
      if (bestWifi is null || (reading.WifiSignalPercent ?? -1) > (bestWifi.WifiSignalPercent ?? -1))
        bestWifi = reading;
    }

    HasWifi = bestWifi is not null;
    var primary = bestWifi ?? SelectStablePrimary(interfaces, busiest);
    _primaryName = bestWifi is null ? primary?.Name : null;
    HasConnection = primary is not null;

    if (primary is null) {
      ConnectionAdapterName = "—";
      ConnectionSsid = "—";
      ConnectionDnsName = "—";
      ConnectionType = "—";
      ConnectionIPv4 = "—";
      ConnectionIPv6 = "—";
      SignalBars = "—";
      // No connection to detail: surface the machine-level radio state instead.
      HasWifiStatus = status is WifiStatus.Disabled or WifiStatus.Disconnected;
      WifiStatusLabel = status switch {
        WifiStatus.Disabled => "Wi-Fi disabled",
        WifiStatus.Disconnected => "Wi-Fi disconnected",
        _ => "",
      };
      return;
    }

    // A connection is shown, so the muted status row stands down.
    HasWifiStatus = false;
    WifiStatusLabel = "";

    ConnectionAdapterName = primary.Name;
    ConnectionDnsName = primary.DnsSuffix ?? "—";
    ConnectionIPv4 = primary.IPv4Address ?? "—";
    ConnectionIPv6 = primary.IPv6Address ?? "—";
    // Wi-Fi-only rows; blank ("—") for a wired primary connection (the view hides them via HasWifi).
    ConnectionSsid = HasWifi ? (primary.WifiSsid ?? "—") : "—";
    ConnectionType = HasWifi ? (primary.WifiPhyType ?? "Ethernet") : "Ethernet";
    SignalBars = HasWifi ? SignalBarsGlyph(primary.WifiSignalPercent) : "—";
  }

  // Pick a stable non-Wi-Fi primary connection. Keep the previously chosen interface as long as it's
  // still present (so the detail rows don't churn as throughput bounces between NICs); otherwise seed
  // with the busiest interface that actually has an IPv4 address, falling back to the busiest overall.
  private NetworkInterfaceReading? SelectStablePrimary(
      IReadOnlyList<NetworkInterfaceReading> interfaces, NetworkInterfaceReading? busiest) {
    if (_primaryName is not null) {
      var held = interfaces.FirstOrDefault(
          r => string.Equals(r.Name, _primaryName, StringComparison.OrdinalIgnoreCase));
      if (held is not null) return held;
    }

    NetworkInterfaceReading? seed = null;
    var seedRate = -1.0;
    foreach (var reading in interfaces) {
      if (reading.IPv4Address is null) continue;
      var rate = reading.DownloadBytesPerSecond + reading.UploadBytesPerSecond;
      if (rate > seedRate) {
        seedRate = rate;
        seed = reading;
      }
    }
    return seed ?? busiest;
  }

  // A four-segment strength meter from ascending block glyphs (mirrors the detail view): a segment
  // lights once quality passes its lower quartile bound (>0/25/50/75); unlit segments show a low
  // baseline glyph. Null/zero quality reads as fully empty.
  private static string SignalBarsGlyph(int? quality) {
    int pct = quality is { } q ? Math.Clamp(q, 0, 100) : 0;
    char[] filled = ['▂', '▄', '▆', '█'];
    var bars = new char[4];
    for (int i = 0; i < 4; i++)
      bars[i] = pct > i * 25 ? filled[i] : '▁';
    return new string(bars);
  }

  // Round a peak up to a readable axis top: 1/2/5 × a power of ten, so the shared throughput scale
  // grows and subsides in round steps as the busiest sample in the window changes.
  private static double NiceCeiling(double value) {
    double magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
    double normalized = value / magnitude;
    double step = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
    return step * magnitude;
  }

  private static string FormatSpeed(double bytesPerSecond) {
    if (bytesPerSecond >= 1024d * 1024 * 1024) return $"{bytesPerSecond / (1024d * 1024 * 1024):0.00} GiB/s";
    if (bytesPerSecond >= 1024d * 1024) return $"{bytesPerSecond / (1024d * 1024):0.00} MiB/s";
    if (bytesPerSecond >= 1024d) return $"{bytesPerSecond / 1024d:0.00} KiB/s";
    return $"{bytesPerSecond:0} B/s";
  }

  private void SyncAdapters(IReadOnlyList<NetworkInterfaceReading> interfaces) {
    for (var i = Adapters.Count - 1; i >= 0; i--) {
      if (!interfaces.Any(r => string.Equals(r.Name, Adapters[i].Name, StringComparison.OrdinalIgnoreCase)))
        Adapters.RemoveAt(i);
    }
    foreach (var reading in interfaces) {
      if (!Adapters.Any(a => string.Equals(a.Name, reading.Name, StringComparison.OrdinalIgnoreCase)))
        Adapters.Add(new NetworkAdapterViewModel());
    }
  }

  private void OnUi(Action action) => _ui.Post(action);

  public void Dispose() {
    _sensorsSubscription.Dispose();
    _topTalkersSubscription.Dispose();
  }
}
