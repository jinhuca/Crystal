using Crystal.Controls.PerformanceGraphs;
using Crystal.Service.Gpu;

namespace GpuModule.ViewModels;

/// <summary>
/// One GPU column in the view: an adapter's static identity plus its live core-load value and
/// history graph. The graph is a ring buffer owned by <see cref="PerformanceGraph"/>, so the
/// view hands the instance in via <see cref="AttachGraph"/> and the VM pushes samples into it.
/// </summary>
public sealed class GpuAdapterViewModel : BindableBase {
  private string _name = "—";
  private string _kindLabel = string.Empty;
  private double? _videoRamGB;
  private string _displayMode = string.Empty;
  private string? _driverVersion;
  private DateTime? _driverDate;
  private string? _videoProcessor;
  private string? _physicalLocation;
  private double _load;
  private double? _temperatureC;
  private double? _clockMhz;
  private double? _powerW;

  private PerformanceGraph? _loadGraph;
  private PerformanceGraph? _temperatureGraph;
  private PerformanceGraph? _clockGraph;
  private PerformanceGraph? _powerGraph;

  public string Name { get => _name; private set => SetProperty(ref _name, value); }
  public string KindLabel { get => _kindLabel; private set => SetProperty(ref _kindLabel, value); }
  public double? VideoRamGB { get => _videoRamGB; private set => SetProperty(ref _videoRamGB, value); }
  public string DisplayMode { get => _displayMode; private set => SetProperty(ref _displayMode, value); }
  public string? DriverVersion { get => _driverVersion; private set => SetProperty(ref _driverVersion, value); }
  public DateTime? DriverDate { get => _driverDate; private set => SetProperty(ref _driverDate, value); }
  public string? VideoProcessor { get => _videoProcessor; private set => SetProperty(ref _videoProcessor, value); }
  public string? PhysicalLocation { get => _physicalLocation; private set => SetProperty(ref _physicalLocation, value); }
  public double Load { get => _load; private set => SetProperty(ref _load, value); }
  public double? TemperatureC { get => _temperatureC; private set => SetProperty(ref _temperatureC, value); }
  public double? ClockMhz { get => _clockMhz; private set => SetProperty(ref _clockMhz, value); }
  public double? PowerW { get => _powerW; private set => SetProperty(ref _powerW, value); }

  public void AttachGraph(PerformanceGraph graph) => _loadGraph = graph;
  public void AttachTemperatureGraph(PerformanceGraph graph) => _temperatureGraph = graph;
  public void AttachClockGraph(PerformanceGraph graph) => _clockGraph = graph;
  public void AttachPowerGraph(PerformanceGraph graph) => _powerGraph = graph;

  /// <summary>Refreshes the static identity from the inventory row.</summary>
  public void UpdateSpecs(GpuAdapterInfo info) {
    Name = info.Name;
    KindLabel = info.Kind == GpuKind.Integrated ? "Integrated GPU" : "Dedicated GPU";
    VideoRamGB = info.VideoRamGB;
    DisplayMode = info.DisplayMode;
    DriverVersion = info.DriverVersion;
    DriverDate = info.DriverDate;
    VideoProcessor = info.VideoProcessor;
    PhysicalLocation = info.PhysicalLocation;
  }

  /// <summary>Pushes fresh load, temperature, clock and power readings into the values and
  /// history graphs.</summary>
  public void UpdateLoad(double loadPercent, double? temperatureC, double? clockMhz, double? powerW) {
    Load = loadPercent;
    _loadGraph?.AddValue(loadPercent);

    TemperatureC = temperatureC;
    if (temperatureC is { } t) _temperatureGraph?.AddValue(t);

    ClockMhz = clockMhz;
    if (clockMhz is { } c) _clockGraph?.AddValue(c);

    PowerW = powerW;
    if (powerW is { } p) _powerGraph?.AddValue(p);
  }
}
