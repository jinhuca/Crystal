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
  private string _utilizationLabel = "—";
  private string _linkSpeedLabel = "—";
  private string _dataDownloadedLabel = "—";
  private string _dataUploadedLabel = "—";
  private double _downloadScaleMax = MinScaleKib;
  private double _uploadScaleMax = MinScaleKib;

  private bool _isWifi;
  private string _wifiSsid = "—";
  private string _wifiSignal = "—";
  private string _wifiSignalBars = "▁▁▁▁";
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

  /// <summary>Current NIC utilization as a percentage of link speed (0-100%).</summary>
  public string UtilizationLabel { get => _utilizationLabel; private set => SetProperty(ref _utilizationLabel, value); }

  /// <summary>OS-reported link speed (e.g. "1.0 Gbps").</summary>
  public string LinkSpeedLabel { get => _linkSpeedLabel; private set => SetProperty(ref _linkSpeedLabel, value); }

  /// <summary>Cumulative data received since the counter last reset.</summary>
  public string DataDownloadedLabel { get => _dataDownloadedLabel; private set => SetProperty(ref _dataDownloadedLabel, value); }

  /// <summary>Cumulative data sent since the counter last reset.</summary>
  public string DataUploadedLabel { get => _dataUploadedLabel; private set => SetProperty(ref _dataUploadedLabel, value); }

  /// <summary>Top of the download graph's KiB/s scale (auto-scaled to a decaying peak).</summary>
  public double DownloadScaleMax { get => _downloadScaleMax; private set => SetProperty(ref _downloadScaleMax, value); }

  /// <summary>Top of the upload graph's KiB/s scale (auto-scaled to a decaying peak).</summary>
  public double UploadScaleMax { get => _uploadScaleMax; private set => SetProperty(ref _uploadScaleMax, value); }

  /// <summary>True when this interface is an associated Wi-Fi radio; drives the Wi-Fi block's visibility.</summary>
  public bool IsWifi { get => _isWifi; private set => SetProperty(ref _isWifi, value); }
  public string WifiSsid { get => _wifiSsid; private set => SetProperty(ref _wifiSsid, value); }
  public string WifiSignal { get => _wifiSignal; private set => SetProperty(ref _wifiSignal, value); }

  /// <summary>Four-segment bar glyph filled proportionally to signal quality (e.g. "▂▄▆▁"), for a
  /// quick at-a-glance strength read next to the numeric signal.</summary>
  public string WifiSignalBars { get => _wifiSignalBars; private set => SetProperty(ref _wifiSignalBars, value); }
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
    UtilizationLabel = $"{reading.UtilizationPercent:0.0}%";
    LinkSpeedLabel = FormatLinkSpeed(reading.LinkSpeedBitsPerSecond);
    DataDownloadedLabel = FormatData(reading.DataDownloadedGb);
    DataUploadedLabel = FormatData(reading.DataUploadedGb);

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
    WifiSignalBars = SignalBars(reading.WifiSignalPercent);
    WifiPhyType = reading.WifiPhyType ?? "—";
    WifiBand = reading.WifiBand ?? "—";
    WifiChannel = reading.WifiChannel is { } ch ? ch.ToString() : "—";

    // Signal quality is already a 0-100 percentage, so the graph uses a fixed scale (no auto-scale).
    if (reading.WifiSignalPercent is { } signal)
      _signalGraph?.AddValue(signal);
  }

  // A four-segment strength meter built from ascending block glyphs. A segment lights up (its tall
  // glyph) once quality passes its lower quartile bound (>0/25/50/75), so 72% shows three bars;
  // unlit segments show a low baseline glyph. Null/zero quality reads as fully empty.
  private static string SignalBars(int? quality) {
    int pct = quality is { } q ? Math.Clamp(q, 0, 100) : 0;
    char[] filled = ['▂', '▄', '▆', '█'];
    var bars = new char[4];
    for (int i = 0; i < 4; i++)
      bars[i] = pct > i * 25 ? filled[i] : '▁';
    return new string(bars);
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

  // The provider reports link speed in bits/sec (OS convention); network links are marketed in
  // decimal Mbps/Gbps, so scale by 1000 rather than 1024.
  private static string FormatLinkSpeed(long bitsPerSecond) {
    if (bitsPerSecond <= 0) return "—";
    if (bitsPerSecond >= 1_000_000_000) return $"{bitsPerSecond / 1_000_000_000d:0.0} Gbps";
    if (bitsPerSecond >= 1_000_000) return $"{bitsPerSecond / 1_000_000d:0} Mbps";
    if (bitsPerSecond >= 1_000) return $"{bitsPerSecond / 1_000d:0} Kbps";
    return $"{bitsPerSecond} bps";
  }

  // Cumulative counters arrive in GB (2^30 bytes). Show TB once the total gets large; a fresh
  // counter reads near zero.
  private static string FormatData(double gigabytes) {
    if (gigabytes >= 1024d) return $"{gigabytes / 1024d:0.00} TiB";
    if (gigabytes >= 1d) return $"{gigabytes:0.00} GiB";
    return $"{gigabytes * 1024d:0.0} MiB";
  }
}
