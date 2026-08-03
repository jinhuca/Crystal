using Crystal.Controls.PerformanceGraphs;
using NetworkModule.Models;

namespace NetworkModule.ViewModels;

/// <summary>
/// One network interface in the detail view: its name plus live utilization and throughput, with
/// a history graph. The graph is a ring buffer owned by <see cref="PerformanceGraph"/>, so the
/// view hands the instance in via <see cref="AttachGraph"/> and the VM pushes samples into it.
/// </summary>
public sealed class NetworkAdapterViewModel : BindableBase {
  private string _name = "—";
  private double _load;
  private string _uploadLabel = "—";
  private string _downloadLabel = "—";

  private PerformanceGraph? _loadGraph;

  public string Name { get => _name; private set => SetProperty(ref _name, value); }
  public double Load { get => _load; private set => SetProperty(ref _load, value); }
  public string UploadLabel { get => _uploadLabel; private set => SetProperty(ref _uploadLabel, value); }
  public string DownloadLabel { get => _downloadLabel; private set => SetProperty(ref _downloadLabel, value); }

  public void AttachGraph(PerformanceGraph graph) => _loadGraph = graph;

  /// <summary>Pushes a fresh reading into the values and the history graph.</summary>
  public void Update(NetworkInterfaceReading reading) {
    Name = reading.Name;
    Load = reading.UtilizationPercent;
    UploadLabel = FormatSpeed(reading.UploadBytesPerSecond);
    DownloadLabel = FormatSpeed(reading.DownloadBytesPerSecond);
    _loadGraph?.AddValue(reading.UtilizationPercent);
  }

  private static string FormatSpeed(double bytesPerSecond) {
    var bits = bytesPerSecond * 8;
    if (bits >= 1_000_000_000) return $"{bits / 1_000_000_000:0.0} Gbps";
    if (bits >= 1_000_000) return $"{bits / 1_000_000:0.0} Mbps";
    if (bits >= 1_000) return $"{bits / 1_000:0.0} Kbps";
    return $"{bits:0} bps";
  }
}
