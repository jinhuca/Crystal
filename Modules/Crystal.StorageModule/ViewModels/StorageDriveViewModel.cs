using Crystal.Controls.PerformanceGraphs;
using Crystal.Service.Storage;
using System.Windows.Media;

namespace Crystal.StorageModule.ViewModels;

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
  private double _readActivityPercent;
  private double _writeActivityPercent;
  private double _readRateMBps;
  private double _writeRateMBps;
  private double? _responseMs;
  private double? _temperatureC;
  private double? _healthPercent;
  private double? _usedSpacePercent;
  private double? _freeSpaceGB;
  private double? _totalSpaceGB;
  private double? _dataReadGB;
  private double? _dataWrittenGB;
  private double? _powerOnHours;
  private double? _powerOnCount;
  private double _transferMaxMBps = TransferFloorMBps;
  private double _peakTransferMBps;
  private ISingleSeriesGraph? _activityGraph;
  private AdaptiveGraph? _transferGraph;
  private int _transferWriteSeries;

  // Write plots as an amber overlay line against the themed (sky) read series. Line-only, so it
  // stays visible on top of the read series' filled glow.
  private static readonly SolidColorBrush WriteSeriesBrush = CreateFrozen(Color.FromRgb(0xE8, 0x9B, 0x2A));

  private static SolidColorBrush CreateFrozen(Color color) {
    var brush = new SolidColorBrush(color);
    brush.Freeze();
    return brush;
  }

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
  public double ReadActivityPercent { get => _readActivityPercent; private set => SetProperty(ref _readActivityPercent, value); }
  public double WriteActivityPercent { get => _writeActivityPercent; private set => SetProperty(ref _writeActivityPercent, value); }
  public double ReadRateMBps { get => _readRateMBps; private set => SetProperty(ref _readRateMBps, value); }
  public double WriteRateMBps { get => _writeRateMBps; private set => SetProperty(ref _writeRateMBps, value); }
  public double TransferMaxMBps { get => _transferMaxMBps; private set => SetProperty(ref _transferMaxMBps, value); }

  public string ActivityLabel => $"{ActivityPercent:0.0}%";
  public string ReadActivityLabel => $"{ReadActivityPercent:0.0}%";
  public string WriteActivityLabel => $"{WriteActivityPercent:0.0}%";
  public string ReadSpeedLabel => $"{ReadRateMBps:0.0} MB/s";
  public string WriteSpeedLabel => $"{WriteRateMBps:0.0} MB/s";
  // Highest combined read+write rate seen this session; GB/s once it crosses 1000 MB/s.
  public string PeakTransferLabel =>
      _peakTransferMBps >= 1000 ? $"{_peakTransferMBps / 1000:0.0} GB/s" : $"{_peakTransferMBps:0.0} MB/s";
  public string ResponseLabel => _responseMs is { } ms ? $"{ms:0.0} ms" : "—";
  public string TemperatureLabel => _temperatureC is { } c ? $"{c:0.#} °C" : "—";
  public string HealthLabel => _healthPercent is { } h ? $"{h:0.#}%" : "—";

  // Filesystem capacity bar: used vs free of the disk's mounted volumes. Fractions sum to 1 so the
  // two star columns split the track exactly; used comes from total-free when both are known,
  // otherwise from the Used Space percentage.
  private double? UsedSpaceGB =>
      _totalSpaceGB is { } total && _freeSpaceGB is { } free ? Math.Max(0, total - free) : null;

  public double UsedSpaceFraction =>
      _totalSpaceGB is { } total and > 0 && _freeSpaceGB is { } free
          ? Math.Clamp((total - free) / total, 0, 1)
          : _usedSpacePercent is { } percent ? Math.Clamp(percent / 100.0, 0, 1) : 0;

  public double FreeSpaceFraction => Math.Max(0, 1 - UsedSpaceFraction);

  public string CapacityUsageLabel =>
      UsedSpaceGB is { } used && _totalSpaceGB is { } total ? $"{used:0.#} / {total:0.#} GB" : "—";
  public string UsedSpaceLabel => UsedSpaceGB is { } used ? $"{used:0.#} GB" : "—";
  public string FreeSpaceLabel => _freeSpaceGB is { } free ? $"{free:0.#} GB" : "—";
  public string UsedSpacePercentLabel => _totalSpaceGB is { } t and > 0 || _usedSpacePercent is not null
      ? $"{UsedSpaceFraction * 100:0}%" : "—";

  // SSD endurance (SMART): lifetime host reads/writes shown in TB once they cross 1 TB, plus the
  // drive's power-on hours and cycle count. All null without elevation/PawnIO or on drives that
  // don't expose the attributes.
  public string DataWrittenLabel => FormatData(_dataWrittenGB);
  public string DataReadLabel => FormatData(_dataReadGB);
  public string PowerOnHoursLabel => _powerOnHours is { } h ? $"{h:0} h" : "—";
  public string PowerOnCountLabel => _powerOnCount is { } c ? $"{c:0}" : "—";

  private static string FormatData(double? gb) =>
      gb is not { } value ? "—"
      : value >= 1024 ? $"{value / 1024:0.0} TB"
      : $"{value:0.#} GB";

  public void AttachActivityGraph(ISingleSeriesGraph graph) => _activityGraph = graph;

  public void AttachTransferGraph(AdaptiveGraph graph) {
    // Loaded re-fires with a fresh graph when the disk selection swaps the template. Registering
    // the write overlay once per graph is idempotent: re-attaching the same instance is a no-op.
    if (ReferenceEquals(_transferGraph, graph)) return;
    _transferGraph = graph;
    _transferWriteSeries = graph.AddSeries(WriteSeriesBrush, fillBrush: null, thickness: 1.5);
  }

  /// <summary>Feeds this disk's newest live sample in, pushing the graphs and refreshing labels.</summary>
  public void Update(StorageDiskLoad load) {
    ActivityPercent = load.ActivityPercent;
    ReadActivityPercent = load.ReadActivityPercent;
    WriteActivityPercent = load.WriteActivityPercent;
    ReadRateMBps = load.ReadRateMBps;
    WriteRateMBps = load.WriteRateMBps;
    _responseMs = load.ResponseMs;
    _temperatureC = load.TemperatureC;
    _healthPercent = load.HealthPercent;
    _usedSpacePercent = load.UsedSpacePercent;
    _freeSpaceGB = load.FreeSpaceGB;
    _totalSpaceGB = load.TotalSpaceGB;
    _dataReadGB = load.DataReadGB;
    _dataWrittenGB = load.DataWrittenGB;
    _powerOnHours = load.PowerOnHours;
    _powerOnCount = load.PowerOnCount;

    _activityGraph?.AddValue(load.ActivityPercent);

    if (_transferGraph is { } transferGraph) {
      transferGraph.AddValue(load.ReadRateMBps);              // primary (read)
      transferGraph.AddValue(_transferWriteSeries, load.WriteRateMBps);
    }
    // The axis tracks the taller of the two traces, not their sum — the lines are drawn
    // independently, so a shared read+write ceiling would leave both sitting low.
    _transferSamples.Enqueue(Math.Max(load.ReadRateMBps, load.WriteRateMBps));
    while (_transferSamples.Count > TransferWindow) _transferSamples.Dequeue();
    TransferMaxMBps = NiceCeiling(Math.Max(TransferFloorMBps, _transferSamples.Max()));
    _peakTransferMBps = Math.Max(_peakTransferMBps, load.ReadRateMBps + load.WriteRateMBps);

    RaisePropertyChanged(nameof(ActivityLabel));
    RaisePropertyChanged(nameof(PeakTransferLabel));
    RaisePropertyChanged(nameof(ReadActivityLabel));
    RaisePropertyChanged(nameof(WriteActivityLabel));
    RaisePropertyChanged(nameof(ReadSpeedLabel));
    RaisePropertyChanged(nameof(WriteSpeedLabel));
    RaisePropertyChanged(nameof(ResponseLabel));
    RaisePropertyChanged(nameof(TemperatureLabel));
    RaisePropertyChanged(nameof(HealthLabel));
    RaisePropertyChanged(nameof(UsedSpaceFraction));
    RaisePropertyChanged(nameof(FreeSpaceFraction));
    RaisePropertyChanged(nameof(CapacityUsageLabel));
    RaisePropertyChanged(nameof(UsedSpaceLabel));
    RaisePropertyChanged(nameof(FreeSpaceLabel));
    RaisePropertyChanged(nameof(UsedSpacePercentLabel));
    RaisePropertyChanged(nameof(DataWrittenLabel));
    RaisePropertyChanged(nameof(DataReadLabel));
    RaisePropertyChanged(nameof(PowerOnHoursLabel));
    RaisePropertyChanged(nameof(PowerOnCountLabel));
  }

  // Round a peak up to a readable axis top: 1/2/5 × a power of ten (100, 200, 500, 1000, …).
  private static double NiceCeiling(double value) {
    double magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
    double normalized = value / magnitude;
    double step = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
    return step * magnitude;
  }
}
