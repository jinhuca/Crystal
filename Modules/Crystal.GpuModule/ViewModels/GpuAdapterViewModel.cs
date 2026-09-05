using Crystal.Controls.PerformanceGraphs;
using Crystal.Service.Gpu;
using System.Collections.ObjectModel;

namespace Crystal.GpuModule.ViewModels;

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
  private uint? _refreshRateHz;
  private double _load;
  private double _load3D;
  private bool _isIntegrated;
  private bool _isDedicated;
  private double? _temperatureC;
  private double? _clockMhz;
  private double? _powerW;
  private double? _memoryUsedGB;
  private double? _memoryTotalGB;
  private double? _memoryUsedPercent;
  private double? _memoryClockMhz;
  private double? _fanRpm;
  private double? _coreVoltageV;
  private double? _hotSpotTemperatureC;
  private double? _memoryTemperatureC;
  private double? _pcieRxMBps;
  private double? _pcieTxMBps;

  // Core temperature uses a fixed 0-100 °C scale (like Load's 0-100 %); clock and power span wildly
  // different ranges per adapter (iGPU vs dGPU), so their graph ceilings ratchet to a "nice" value
  // above the running peak. Peaks decay slowly so the ceiling relaxes after a spike.
  private const double PeakDecay = 0.95;
  private const double MinClockScale = 500;
  private const double MinPowerScale = 50;
  private const double MinPcieScale = 10;
  private double _clockScaleMax = MinClockScale;
  private double _powerScaleMax = MinPowerScale;
  private double _pcieRxScaleMax = MinPcieScale;
  private double _pcieTxScaleMax = MinPcieScale;
  private double _clockPeak;
  private double _powerPeak;
  private double _pcieRxPeak;
  private double _pcieTxPeak;

  // History graphs are registered by their GraphIdentity.Id as each metric sub-view loads (the
  // detail view registers with the same ids explicitly), then fed by that same id in UpdateLoad.
  // One graph per id per adapter; last registration for an id wins, matching the previous
  // single-field behaviour when the summary and detail views attach to the same adapter VM.
  private readonly Dictionary<string, ISingleSeriesGraph> _graphs = [];

  public string Name { get => _name; private set => SetProperty(ref _name, value); }
  public string KindLabel { get => _kindLabel; private set => SetProperty(ref _kindLabel, value); }
  public double? VideoRamGB { get => _videoRamGB; private set => SetProperty(ref _videoRamGB, value); }
  public string DisplayMode { get => _displayMode; private set => SetProperty(ref _displayMode, value); }
  public string? DriverVersion { get => _driverVersion; private set => SetProperty(ref _driverVersion, value); }
  public DateTime? DriverDate { get => _driverDate; private set => SetProperty(ref _driverDate, value); }
  public string? VideoProcessor { get => _videoProcessor; private set => SetProperty(ref _videoProcessor, value); }
  public string? PhysicalLocation { get => _physicalLocation; private set => SetProperty(ref _physicalLocation, value); }
  public uint? RefreshRateHz { get => _refreshRateHz; private set => SetProperty(ref _refreshRateHz, value); }
  public double Load { get => _load; private set => SetProperty(ref _load, value); }

  /// <summary>Utilization of the 3D engine (0-100%), the headline "3D" tile in the reference
  /// design. Falls back to the aggregate core load when the adapter exposes no distinct 3D engine.</summary>
  public double Load3D { get => _load3D; private set => SetProperty(ref _load3D, value); }

  /// <summary>True when this adapter is the CPU's integrated graphics (left column of the design).</summary>
  public bool IsIntegrated { get => _isIntegrated; private set => SetProperty(ref _isIntegrated, value); }

  /// <summary>True when this adapter is a discrete card (right column of the design).</summary>
  public bool IsDedicated { get => _isDedicated; private set => SetProperty(ref _isDedicated, value); }

  public double? TemperatureC { get => _temperatureC; private set => SetProperty(ref _temperatureC, value); }
  public double? ClockMhz { get => _clockMhz; private set => SetProperty(ref _clockMhz, value); }
  public double? PowerW { get => _powerW; private set => SetProperty(ref _powerW, value); }
  public double? MemoryUsedGB { get => _memoryUsedGB; private set => SetProperty(ref _memoryUsedGB, value); }
  public double? MemoryTotalGB { get => _memoryTotalGB; private set => SetProperty(ref _memoryTotalGB, value); }
  public double? MemoryUsedPercent { get => _memoryUsedPercent; private set => SetProperty(ref _memoryUsedPercent, value); }
  public double? MemoryClockMhz { get => _memoryClockMhz; private set => SetProperty(ref _memoryClockMhz, value); }
  public double? FanRpm { get => _fanRpm; private set => SetProperty(ref _fanRpm, value); }
  public double? CoreVoltageV { get => _coreVoltageV; private set => SetProperty(ref _coreVoltageV, value); }
  public double? HotSpotTemperatureC { get => _hotSpotTemperatureC; private set => SetProperty(ref _hotSpotTemperatureC, value); }
  public double? MemoryTemperatureC { get => _memoryTemperatureC; private set => SetProperty(ref _memoryTemperatureC, value); }
  public double? PcieRxMBps { get => _pcieRxMBps; private set => SetProperty(ref _pcieRxMBps, value); }
  public double? PcieTxMBps { get => _pcieTxMBps; private set => SetProperty(ref _pcieTxMBps, value); }

  /// <summary>
  /// Upper bound of the core-clock history graph, ratcheted to a round value above the
  /// running peak so a 1.3 GHz iGPU and a 2.6 GHz dGPU each plot on a sensibly-scaled axis.
  /// </summary>
  public double ClockScaleMax { get => _clockScaleMax; private set => SetProperty(ref _clockScaleMax, value); }

  /// <summary>
  /// Upper bound of the power history graph, ratcheted like <see cref="ClockScaleMax"/>.
  /// </summary>
  public double PowerScaleMax { get => _powerScaleMax; private set => SetProperty(ref _powerScaleMax, value); }

  /// <summary>
  /// Upper bound of the PCIe Rx throughput graph, ratcheted over that direction's running peak.
  /// Scaled independently of <see cref="PcieTxScaleMax"/> so a busy Rx direction can't flatten a
  /// low-but-live Tx trace (Rx/Tx routinely differ by an order of magnitude).
  /// </summary>
  public double PcieRxScaleMax { get => _pcieRxScaleMax; private set => SetProperty(ref _pcieRxScaleMax, value); }

  /// <summary>
  /// Upper bound of the PCIe Tx throughput graph, ratcheted over that direction's running peak,
  /// independently of <see cref="PcieRxScaleMax"/>.
  /// </summary>
  public double PcieTxScaleMax { get => _pcieTxScaleMax; private set => SetProperty(ref _pcieTxScaleMax, value); }

  /// <summary>
  /// Per-engine utilization breakdown, reconciled in place across polls so the rows stay
  /// stable and only their values tick.
  /// </summary>
  public ObservableCollection<GpuEngineLoadViewModel> EngineLoads { get; } = [];

  public bool HasEngineLoads => EngineLoads.Count > 0;

  /// <summary>
  /// Per-rail power breakdown, reconciled in place across polls like <see cref="EngineLoads"/>.
  /// </summary>
  public ObservableCollection<GpuPowerRailViewModel> PowerRails { get; } = [];

  public bool HasPowerRails => PowerRails.Count > 0;

  public void AttachGraph(string id, ISingleSeriesGraph graph) => _graphs[id] = graph;

  private void FeedGraph(string id, double value) {
    if (_graphs.TryGetValue(id, out var graph)) graph.AddValue(value);
  }

  /// <summary>
  /// Refreshes the static identity from the inventory row.
  /// </summary>
  public void UpdateSpecs(GpuAdapterInfo info) {
    Name = info.Name;
    IsIntegrated = info.Kind == GpuKind.Integrated;
    IsDedicated = info.Kind == GpuKind.Dedicated;
    KindLabel = info.Kind == GpuKind.Integrated ? "Integrated GPU" : "Dedicated GPU";
    VideoRamGB = info.VideoRamGB;
    DisplayMode = info.DisplayMode;
    DriverVersion = info.DriverVersion;
    DriverDate = info.DriverDate;
    VideoProcessor = info.VideoProcessor;
    PhysicalLocation = info.PhysicalLocation;
    RefreshRateHz = info.RefreshRateHz;
  }

  /// <summary>
  /// Pushes fresh live readings into the values and history graphs.
  /// </summary>
  public void UpdateLoad(GpuLoadReading reading) {
    Load = reading.CoreLoadPercent;
    FeedGraph("Gpu.Utilization", reading.CoreLoadPercent);

    TemperatureC = reading.TemperatureC;
    if (reading.TemperatureC is { } t) FeedGraph("Gpu.Temperature", t);

    ClockMhz = reading.ClockMhz;
    if (reading.ClockMhz is { } c) {
      FeedGraph("Gpu.Clock", c);
      _clockPeak = Math.Max(c, _clockPeak * PeakDecay);
      ClockScaleMax = NiceScale(_clockPeak, MinClockScale);
    }

    PowerW = reading.PowerW;
    if (reading.PowerW is { } p) {
      FeedGraph("Gpu.Power", p);
      _powerPeak = Math.Max(p, _powerPeak * PeakDecay);
      PowerScaleMax = NiceScale(_powerPeak, MinPowerScale);
    }

    MemoryUsedGB = reading.MemoryUsedGB;
    MemoryTotalGB = reading.MemoryTotalGB;
    MemoryUsedPercent = reading is { MemoryUsedGB: { } used, MemoryTotalGB: { } total } && total > 0
        ? used / total * 100
        : null;
    if (MemoryUsedPercent is { } mem) FeedGraph("Gpu.Memory", mem);
    MemoryClockMhz = reading.MemoryClockMhz;
    FanRpm = reading.FanRpm;
    CoreVoltageV = reading.CoreVoltageV;
    HotSpotTemperatureC = reading.HotSpotTemperatureC;
    if (reading.HotSpotTemperatureC is { } hot) FeedGraph("Gpu.HotSpot", hot);
    MemoryTemperatureC = reading.MemoryTemperatureC;

    PcieRxMBps = reading.PcieRxMBps;
    PcieTxMBps = reading.PcieTxMBps;
    if (reading.PcieRxMBps is { } rx) FeedGraph("Gpu.PcieRx", rx);
    if (reading.PcieTxMBps is { } tx) FeedGraph("Gpu.PcieTx", tx);
    _pcieRxPeak = Math.Max(reading.PcieRxMBps ?? 0, _pcieRxPeak * PeakDecay);
    PcieRxScaleMax = NiceScale(_pcieRxPeak, MinPcieScale);
    _pcieTxPeak = Math.Max(reading.PcieTxMBps ?? 0, _pcieTxPeak * PeakDecay);
    PcieTxScaleMax = NiceScale(_pcieTxPeak, MinPcieScale);

    var engines = reading.EngineLoads ?? [];
    ReconcileEngineLoads(engines);
    ReconcilePowerRails(reading.PowerRails ?? []);

    // "3D" is the headline engine in the design; fall back to aggregate core load when the adapter
    // exposes no distinct 3D engine, so the tile always shows a live value.
    var threeD = engines.FirstOrDefault(e => e.Name.Contains("3D", StringComparison.OrdinalIgnoreCase));
    Load3D = threeD?.LoadPercent ?? reading.CoreLoadPercent;
    FeedGraph("Gpu.3D", Load3D);
  }

  /// <summary>
  /// The engine set is stable across polls (same D3D nodes), so match by name and update values in
  /// place rather than replacing the collection — keeps the list from flickering every second.
  /// </summary>
  /// <param name="engines">The latest engine load readings.</param>
  private void ReconcileEngineLoads(IReadOnlyList<GpuEngineLoad> engines) {
    int before = EngineLoads.Count;

    for (int i = EngineLoads.Count - 1; i >= 0; i--)
      if (!engines.Any(e => e.Name == EngineLoads[i].Name)) {
        EngineLoads.RemoveAt(i);
      }

    foreach (var engine in engines) {
      var existing = EngineLoads.FirstOrDefault(vm => vm.Name == engine.Name);
      if (existing is null) {
        EngineLoads.Add(new GpuEngineLoadViewModel(engine.Name) { LoadPercent = engine.LoadPercent });
      }
      else {
        existing.LoadPercent = engine.LoadPercent;
      }
    }

    if ((before > 0) != (EngineLoads.Count > 0)) {
      RaisePropertyChanged(nameof(HasEngineLoads));
    }
  }

  /// <summary>
  /// The power rail set is stable across polls (same physical rails), so match by name and update
  /// values in place rather than replacing the collection — keeps the list from flickering every second.
  /// </summary>
  /// <param name="rails">The latest power rail readings.</param>
  private void ReconcilePowerRails(IReadOnlyList<GpuPowerRail> rails) {
    int before = PowerRails.Count;

    for (int i = PowerRails.Count - 1; i >= 0; i--)
      if (!rails.Any(r => r.Name == PowerRails[i].Name)) {
        PowerRails.RemoveAt(i);
      }

    foreach (var rail in rails) {
      var existing = PowerRails.FirstOrDefault(vm => vm.Name == rail.Name);
      if (existing is null) {
        PowerRails.Add(new GpuPowerRailViewModel(rail.Name) { PowerW = rail.PowerW });
      }
      else {
        existing.PowerW = rail.PowerW;
      }
    }

    if ((before > 0) != (PowerRails.Count > 0)) {
      RaisePropertyChanged(nameof(HasPowerRails));
    }
  }

  /// <summary>
  /// Round the peak up to a "nice" 1/2/5·10ⁿ value (never below the floor) so the graph ceiling stays
  /// readable and doesn't jitter its axis on every sample.
  /// </summary>
  /// <param name="value">The value to scale.</param>
  /// <param name="min">The minimum value.</param>
  /// <returns>The scaled value.</returns>
  private static double NiceScale(double value, double min) {
    if (value <= min) {
      return min;
    }

    var magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
    var normalized = value / magnitude;
    var nice = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
    return nice * magnitude;
  }
}
