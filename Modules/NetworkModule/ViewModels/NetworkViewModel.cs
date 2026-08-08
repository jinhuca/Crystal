using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Crystal.Infrastructure.Constants.Navigation;
using NetworkModule.Models;

namespace NetworkModule.ViewModels;

public sealed class NetworkViewModel : BindableBase, INetworkViewModel, IDisposable {
  private readonly IDisposable _sensorsSubscription;
  private string _downloadLabel = "—";
  private string _uploadLabel = "—";
  private bool _hasWifi;
  private string _wifiLabel = "—";
  private string _wifiLinkRate = "—";
  private string _wifiSecurity = "—";
  private string _wifiBssid = "—";

  public NetworkViewModel(INetworkModel model, IEventAggregator events) {
    ShowDetailCommand = new DelegateCommand(
        () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Network));
    ShowDashboardCommand = new DelegateCommand(
        () => events.GetEvent<ShowDashboardEvent>().Publish());

    _sensorsSubscription = model.Sensors.Subscribe(s => OnUi(() => Apply(s)));
  }

  public ObservableCollection<NetworkAdapterViewModel> Adapters { get; } = [];
  public string DownloadLabel { get => _downloadLabel; private set => SetProperty(ref _downloadLabel, value); }
  public string UploadLabel { get => _uploadLabel; private set => SetProperty(ref _uploadLabel, value); }
  public bool HasWifi { get => _hasWifi; private set => SetProperty(ref _hasWifi, value); }
  public string WifiLabel { get => _wifiLabel; private set => SetProperty(ref _wifiLabel, value); }
  public string WifiLinkRate { get => _wifiLinkRate; private set => SetProperty(ref _wifiLinkRate, value); }
  public string WifiSecurity { get => _wifiSecurity; private set => SetProperty(ref _wifiSecurity, value); }
  public string WifiBssid { get => _wifiBssid; private set => SetProperty(ref _wifiBssid, value); }
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

    ApplyWifiSummary(snapshot.Interfaces);
  }

  // Pick the strongest connected Wi-Fi adapter for the compact tile (a machine can have several
  // wireless radios). Absent any Wi-Fi, the tile hides the row.
  private void ApplyWifiSummary(IReadOnlyList<NetworkInterfaceReading> interfaces) {
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
      return;
    }

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

  private static void OnUi(Action action) {
    var dispatcher = Application.Current?.Dispatcher;
    if (dispatcher is null || dispatcher.CheckAccess()) action();
    else dispatcher.Invoke(action);
  }

  public void Dispose() => _sensorsSubscription.Dispose();
}
