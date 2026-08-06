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
