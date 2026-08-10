using Crystal.Controls.PerformanceGraphs;
using Crystal.Service.Storage;

namespace StorageModule.ViewModels;

/// <summary>One physical disk, selectable on the detail view's disk list. Carries the static
/// identity (model, capacity, interface) and the live per-disk metrics (active time, read/write
/// rates, average response time) that Task Manager shows on the selected disk's page.</summary>
public sealed class StorageDriveViewModel : BindableBase {
  // Transfer rate has no natural ceiling, so the graph's Y-axis tracks the rolling peak over the
  // visible window instead of a fixed max — same treatment the tile used for the aggregate rate.
  private const int TransferWindow = 60;
  private const double TransferFloorMBps = 100;

  private readonly Queue<double> _transferSamples = new(TransferWindow + 1);
  private double _activityPercent;
  private double _readRateMBps;
  private double _writeRateMBps;
  private double? _responseMs;
  private double _transferMaxMBps = TransferFloorMBps;
  private PerformanceGraph? _activityGraph;
  private PerformanceGraph? _transferGraph;

  public StorageDriveViewModel(StorageDriveInfo info) {
    DriveIndex = info.DriveIndex;
    DiskLabel = info.DriveIndex is { } i ? $"Disk {i}" : "Disk";
    Model = info.Model;
    CapacityLabel = info.CapacityGB is { } gb ? $"{gb:0.#} GB" : "—";
    InterfaceType = string.IsNullOrWhiteSpace(info.InterfaceType) ? "—" : info.InterfaceType!;
    MediaType = string.IsNullOrWhiteSpace(info.MediaType) ? "—" : info.MediaType!;
    Manufacturer = string.IsNullOrWhiteSpace(info.Manufacturer) ? "—" : info.Manufacturer!;
    SerialNumber = string.IsNullOrWhiteSpace(info.SerialNumber) ? "—" : info.SerialNumber!;
    FirmwareRevision = string.IsNullOrWhiteSpace(info.FirmwareRevision) ? "—" : info.FirmwareRevision!;
    PartitionsLabel = info.Partitions is { } p ? p.ToString() : "—";

    // WMI's media-type strings ("Fixed hard disk media", …) are too long for the dense selector
    // header; collapse them to a one-word label so the disk kind fits without clipping.
    ShortMediaType =
        info.MediaType is not { } media || string.IsNullOrWhiteSpace(media) ? "Fixed"
        : media.Contains("removable", StringComparison.OrdinalIgnoreCase) ? "Removable"
        : media.Contains("external", StringComparison.OrdinalIgnoreCase) ? "External"
        : media.Contains("fixed", StringComparison.OrdinalIgnoreCase) ? "Fixed"
        : media;

    // The selector header pairs the disk number with the media kind, e.g. "Disk 0 (SSD)".
    var kind = ShortMediaType == "Fixed" ? "Disk" : ShortMediaType;
    HeaderLabel = info.DriveIndex is { } n ? $"Disk {n} ({kind})" : Model;
  }

  public int? DriveIndex { get; }
  public string DiskLabel { get; }
  public string HeaderLabel { get; }
  public string Model { get; }
  public string CapacityLabel { get; }
  public string InterfaceType { get; }
  public string MediaType { get; }
  public string ShortMediaType { get; }
  public string Manufacturer { get; }
  public string SerialNumber { get; }
  public string FirmwareRevision { get; }
  public string PartitionsLabel { get; }

  public double ActivityPercent { get => _activityPercent; private set => SetProperty(ref _activityPercent, value); }
  public double ReadRateMBps { get => _readRateMBps; private set => SetProperty(ref _readRateMBps, value); }
  public double WriteRateMBps { get => _writeRateMBps; private set => SetProperty(ref _writeRateMBps, value); }
  public double TransferMaxMBps { get => _transferMaxMBps; private set => SetProperty(ref _transferMaxMBps, value); }

  public string ActivityLabel => $"{ActivityPercent:0.0}%";
  public string ReadSpeedLabel => $"{ReadRateMBps:0.0} MB/s";
  public string WriteSpeedLabel => $"{WriteRateMBps:0.0} MB/s";
  public string ResponseLabel => _responseMs is { } ms ? $"{ms:0.0} ms" : "—";

  public void AttachActivityGraph(PerformanceGraph graph) => _activityGraph = graph;
  public void AttachTransferGraph(PerformanceGraph graph) => _transferGraph = graph;

  /// <summary>Feeds this disk's newest live sample in, pushing the graphs and refreshing labels.</summary>
  public void Update(StorageDiskLoad load) {
    ActivityPercent = load.ActivityPercent;
    ReadRateMBps = load.ReadRateMBps;
    WriteRateMBps = load.WriteRateMBps;
    _responseMs = load.ResponseMs;

    _activityGraph?.AddValue(load.ActivityPercent);

    var transfer = load.ReadRateMBps + load.WriteRateMBps;
    _transferGraph?.AddValue(transfer);
    _transferSamples.Enqueue(transfer);
    while (_transferSamples.Count > TransferWindow) _transferSamples.Dequeue();
    TransferMaxMBps = NiceCeiling(Math.Max(TransferFloorMBps, _transferSamples.Max()));

    RaisePropertyChanged(nameof(ActivityLabel));
    RaisePropertyChanged(nameof(ReadSpeedLabel));
    RaisePropertyChanged(nameof(WriteSpeedLabel));
    RaisePropertyChanged(nameof(ResponseLabel));
  }

  // Round a peak up to a readable axis top: 1/2/5 × a power of ten (100, 200, 500, 1000, …).
  private static double NiceCeiling(double value) {
    double magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
    double normalized = value / magnitude;
    double step = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
    return step * magnitude;
  }
}
