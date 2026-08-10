using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Service.Network;
using NetworkModule.Models;

namespace NetworkModule.ViewModels;

public sealed class NetworkViewModel : BindableBase, INetworkViewModel, IDisposable {
  private readonly IDisposable _sensorsSubscription;
  private readonly IDisposable _topTalkersSubscription;
  private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
  private readonly Dictionary<uint, ProcessNetworkRowViewModel> _talkersByPid = new();
  private string _downloadLabel = "—";
  private string _uploadLabel = "—";
  private bool _hasWifi;
  private string _wifiLabel = "—";
  private string _wifiLinkRate = "—";
  private string _wifiSecurity = "—";
  private string _wifiBssid = "—";
  private bool _hasWifiStatus;
  private string _wifiStatusLabel = "";
  private bool _hasTopTalkersStatus;
  private string _topTalkersStatusLabel = "";
  private string _topTalkersSortProperty = nameof(ProcessNetworkRowViewModel.RateBytesPerSecond);
  private ListSortDirection _topTalkersSortDirection = ListSortDirection.Descending;

  // How many talkers the compact summary tile shows (the detail view shows all of them).
  private const int SummaryTopCount = 5;

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
  public bool HasWifi { get => _hasWifi; private set => SetProperty(ref _hasWifi, value); }
  public string WifiLabel { get => _wifiLabel; private set => SetProperty(ref _wifiLabel, value); }
  public string WifiLinkRate { get => _wifiLinkRate; private set => SetProperty(ref _wifiLinkRate, value); }
  public string WifiSecurity { get => _wifiSecurity; private set => SetProperty(ref _wifiSecurity, value); }
  public string WifiBssid { get => _wifiBssid; private set => SetProperty(ref _wifiBssid, value); }
  public bool HasWifiStatus { get => _hasWifiStatus; private set => SetProperty(ref _hasWifiStatus, value); }
  public string WifiStatusLabel { get => _wifiStatusLabel; private set => SetProperty(ref _wifiStatusLabel, value); }
  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  private void Apply(NetworkSnapshot snapshot) {
    // Reconcile the adapter list against the current interfaces (they can come and go as NICs
    // connect/disconnect), keyed by name.
    SyncAdapters(snapshot.Interfaces);

    var totalDownload = 0.0;
    var totalUpload = 0.0;
    foreach (var reading in snapshot.Interfaces) {
      var adapter = Adapters.FirstOrDefault(a =>
          string.Equals(a.Name, reading.Name, StringComparison.OrdinalIgnoreCase));
      adapter?.Update(reading);
      totalDownload += reading.DownloadBytesPerSecond;
      totalUpload += reading.UploadBytesPerSecond;
    }

    DownloadLabel = FormatSpeed(totalDownload);
    UploadLabel = FormatSpeed(totalUpload);

    ApplyWifiSummary(snapshot.Interfaces, snapshot.WifiStatus);
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
    } else {
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

  // A present-but-not-connected radio shows a muted status line instead of the connected block; a
  // machine with no radio (WifiStatus.None) shows neither.
  private void ApplyWifiStatus(WifiStatus status) {
    HasWifiStatus = status is WifiStatus.Disabled or WifiStatus.Disconnected;
    WifiStatusLabel = status switch {
      WifiStatus.Disabled => "Wi-Fi disabled",
      WifiStatus.Disconnected => "Wi-Fi disconnected",
      _ => "",
    };
  }

  // Pick the strongest connected Wi-Fi adapter for the compact tile (a machine can have several
  // wireless radios). Absent any connected Wi-Fi, fall back to a muted status row driven by the
  // machine-level Wi-Fi state.
  private void ApplyWifiSummary(IReadOnlyList<NetworkInterfaceReading> interfaces, WifiStatus status) {
    NetworkInterfaceReading? best = null;
    foreach (var reading in interfaces) {
      if (reading.WifiSignalPercent is null && reading.WifiSsid is null) continue;
      if (best is null || (reading.WifiSignalPercent ?? -1) > (best.WifiSignalPercent ?? -1))
        best = reading;
    }

    HasWifi = best is not null;
    if (best is null) {
      WifiLabel = "—";
      WifiLinkRate = "—";
      WifiSecurity = "—";
      WifiBssid = "—";
      ApplyWifiStatus(status);
      return;
    }

    // A connected radio shows the full block; the muted status row stands down.
    ApplyWifiStatus(WifiStatus.Connected);

    var ssid = best.WifiSsid ?? "Wi-Fi";
    WifiLabel = best.WifiSignalPercent is { } pct ? $"{ssid}  {pct}%" : ssid;
    WifiLinkRate = FormatLinkRate(best.WifiRxRateKbps, best.WifiTxRateKbps);
    WifiSecurity = best.WifiSecurity ?? "—";
    WifiBssid = best.WifiBssid ?? "—";
  }

  // wlanapi reports link rates in Kbps; show whole Mbps as "Rx / Tx". When both sides match (the
  // common case) collapse to a single value.
  private static string FormatLinkRate(int? rxKbps, int? txKbps) {
    string? rx = rxKbps is { } r ? (r / 1000).ToString() : null;
    string? tx = txKbps is { } t ? (t / 1000).ToString() : null;
    if (rx is null && tx is null) return "—";
    if (rx == tx) return $"{rx} Mbps";
    return $"{rx ?? "—"} / {tx ?? "—"} Mbps";
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

  // Marshal onto the UI dispatcher captured at construction (this VM is built on the UI thread).
  // TopTalkers is fed by the ETW background thread, and at app shutdown Application.Current is torn
  // down to null; reading the dispatcher fresh each call would then fall through to running inline on
  // the ETW thread and mutate the UI-affined collections, throwing. Capturing the dispatcher and
  // dropping work once it is shutting down keeps late emissions off the UI collections.
  private void OnUi(Action action) {
    if (_dispatcher.HasShutdownStarted || _dispatcher.HasShutdownFinished) return;
    if (_dispatcher.CheckAccess()) action();
    else _dispatcher.BeginInvoke(action);
  }

  public void Dispose() {
    _sensorsSubscription.Dispose();
    _topTalkersSubscription.Dispose();
  }
}
