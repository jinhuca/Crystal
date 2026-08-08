using Crystal.Controls.PerformanceGraphs;
using NetworkModule.Models;

namespace NetworkModule.ViewModels;

/// <summary>
/// One network interface in the detail view: its name plus live download/upload throughput, each
/// with its own history graph. The graphs are ring buffers owned by <see cref="PerformanceGraph"/>,
/// so the view hands each instance in via the Attach methods and the VM pushes samples into them.
/// Graph samples and scale are kept in KiB/s; the large text labels adapt their own units.
/// </summary>
public sealed class NetworkAdapterViewModel : BindableBase {
  // Idle interfaces would otherwise auto-scale to a flat line; keep a small floor so the graph
  // has a sane baseline, and let a decaying peak drive the scale up when traffic picks up.
  private const double MinScaleKib = 64;
  private const double PeakDecay = 0.9;

  private string _name = "—";
  private string _uploadLabel = "—";
  private string _downloadLabel = "—";
  private double _downloadScaleMax = MinScaleKib;
  private double _uploadScaleMax = MinScaleKib;

  private bool _isWifi;
  private string _wifiSsid = "—";
  private string _wifiSignal = "—";
  private string _wifiPhyType = "—";
  private string _wifiBand = "—";
  private string _wifiChannel = "—";

  private PerformanceGraph? _downloadGraph;
  private PerformanceGraph? _uploadGraph;
  private PerformanceGraph? _signalGraph;
  private double _downloadPeakKib;
  private double _uploadPeakKib;

  public string Name { get => _name; private set => SetProperty(ref _name, value); }
  public string UploadLabel { get => _uploadLabel; private set => SetProperty(ref _uploadLabel, value); }
  public string DownloadLabel { get => _downloadLabel; private set => SetProperty(ref _downloadLabel, value); }

  /// <summary>Top of the download graph's KiB/s scale (auto-scaled to a decaying peak).</summary>
  public double DownloadScaleMax { get => _downloadScaleMax; private set => SetProperty(ref _downloadScaleMax, value); }

  /// <summary>Top of the upload graph's KiB/s scale (auto-scaled to a decaying peak).</summary>
  public double UploadScaleMax { get => _uploadScaleMax; private set => SetProperty(ref _uploadScaleMax, value); }

  /// <summary>True when this interface is an associated Wi-Fi radio; drives the Wi-Fi block's visibility.</summary>
  public bool IsWifi { get => _isWifi; private set => SetProperty(ref _isWifi, value); }
  public string WifiSsid { get => _wifiSsid; private set => SetProperty(ref _wifiSsid, value); }
  public string WifiSignal { get => _wifiSignal; private set => SetProperty(ref _wifiSignal, value); }
  public string WifiPhyType { get => _wifiPhyType; private set => SetProperty(ref _wifiPhyType, value); }
  public string WifiBand { get => _wifiBand; private set => SetProperty(ref _wifiBand, value); }
  public string WifiChannel { get => _wifiChannel; private set => SetProperty(ref _wifiChannel, value); }

  public void AttachDownloadGraph(PerformanceGraph graph) => _downloadGraph = graph;
  public void AttachUploadGraph(PerformanceGraph graph) => _uploadGraph = graph;
  public void AttachSignalGraph(PerformanceGraph graph) => _signalGraph = graph;

  /// <summary>Pushes a fresh reading into the labels and the two history graphs.</summary>
  public void Update(NetworkInterfaceReading reading) {
    Name = reading.Name;
    DownloadLabel = FormatSpeed(reading.DownloadBytesPerSecond);
    UploadLabel = FormatSpeed(reading.UploadBytesPerSecond);

    var downloadKib = reading.DownloadBytesPerSecond / 1024d;
    var uploadKib = reading.UploadBytesPerSecond / 1024d;

    _downloadPeakKib = Math.Max(downloadKib, _downloadPeakKib * PeakDecay);
    _uploadPeakKib = Math.Max(uploadKib, _uploadPeakKib * PeakDecay);
    DownloadScaleMax = NiceScale(_downloadPeakKib);
    UploadScaleMax = NiceScale(_uploadPeakKib);

    _downloadGraph?.AddValue(downloadKib);
    _uploadGraph?.AddValue(uploadKib);

    ApplyWifi(reading);
  }

  // A connected Wi-Fi adapter reports a signal quality; wired NICs report none. Populate the Wi-Fi
  // block only in that case and leave the placeholders ("—") otherwise so a wired card stays clean.
  private void ApplyWifi(NetworkInterfaceReading reading) {
    IsWifi = reading.WifiSignalPercent is not null || reading.WifiSsid is not null;
    if (!IsWifi) return;

    WifiSsid = reading.WifiSsid ?? "—";
    WifiSignal = reading.WifiSignalPercent is { } pct
        ? (reading.WifiRssiDbm is { } dbm ? $"{pct}%  ({dbm} dBm)" : $"{pct}%")
        : "—";
    WifiPhyType = reading.WifiPhyType ?? "—";
    WifiBand = reading.WifiBand ?? "—";
    WifiChannel = reading.WifiChannel is { } ch ? ch.ToString() : "—";

    // Signal quality is already a 0-100 percentage, so the graph uses a fixed scale (no auto-scale).
    if (reading.WifiSignalPercent is { } signal)
      _signalGraph?.AddValue(signal);
  }

  // Round the peak up to a "nice" 1/2/5·10ⁿ value so the scale label stays readable and the plot
  // doesn't jitter its ceiling on every sample.
  private static double NiceScale(double kib) {
    if (kib <= MinScaleKib) return MinScaleKib;
    var magnitude = Math.Pow(10, Math.Floor(Math.Log10(kib)));
    var normalized = kib / magnitude;
    var nice = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
    return nice * magnitude;
  }

  private static string FormatSpeed(double bytesPerSecond) {
    if (bytesPerSecond >= 1024d * 1024 * 1024) return $"{bytesPerSecond / (1024d * 1024 * 1024):0.00} GiB/s";
    if (bytesPerSecond >= 1024d * 1024) return $"{bytesPerSecond / (1024d * 1024):0.00} MiB/s";
    if (bytesPerSecond >= 1024d) return $"{bytesPerSecond / 1024d:0.00} KiB/s";
    return $"{bytesPerSecond:0} B/s";
  }
}
