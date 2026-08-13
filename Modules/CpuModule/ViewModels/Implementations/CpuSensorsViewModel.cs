using System.Collections.ObjectModel;
using CpuModule.Models;
using CpuModule.ViewModels;
using CpuModule.ViewModels.Interfaces;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Infrastructure.DataStructures.Sensors;

namespace CpuModule.ViewModels.Implementations;

public sealed class CpuSensorsViewModel : BindableBase, ICpuSensorViewModel {
  private double _load;
  private double _voltage;
  private double _socVoltage;
  private double _speedGhz;
  private double _effectiveSpeedGhz;
  private double _busSpeedMHz;
  private double _power;
  private double _powerLimitLongW;
  private double _powerLimitShortW;
  private double _tdcAmps;
  private double _edcAmps;
  private double _packageC2Pct;
  private double _packageC3Pct;
  private double _packageC6Pct;
  private double _packageC7Pct;
  private double _temperature;
  private double _distanceToTjMax;
  private bool _isThrottling;
  private string _throttleStatus = string.Empty;
  private int _fanRpm;
  private bool _hasCpuFan;
  private double _fanPercent;
  private bool _hasCpuFanPercent;
  private bool _msrSensorsAvailable;

  private PerformanceGraph? _utilizationGraph;
  private PerformanceGraph? _voltageGraph;
  private PerformanceGraph? _clockGraph;
  private PerformanceGraph? _powerGraph;
  private PerformanceGraph? _temperatureGraph;
  private PerformanceGraph? _fanGraph;

  public double Load { get => _load; private set => SetProperty(ref _load, value); }
  public double Voltage { get => _voltage; private set => SetProperty(ref _voltage, value); }
  public double SocVoltage { get => _socVoltage; private set => SetProperty(ref _socVoltage, value); }
  public double SpeedGhz { get => _speedGhz; private set => SetProperty(ref _speedGhz, value); }
  public double EffectiveSpeedGhz { get => _effectiveSpeedGhz; private set => SetProperty(ref _effectiveSpeedGhz, value); }
  public double BusSpeedMHz { get => _busSpeedMHz; private set => SetProperty(ref _busSpeedMHz, value); }
  public double Power { get => _power; private set => SetProperty(ref _power, value); }
  public double PowerLimitLongW { get => _powerLimitLongW; private set => SetProperty(ref _powerLimitLongW, value); }
  public double PowerLimitShortW { get => _powerLimitShortW; private set => SetProperty(ref _powerLimitShortW, value); }
  public double TdcAmps { get => _tdcAmps; private set => SetProperty(ref _tdcAmps, value); }
  public double EdcAmps { get => _edcAmps; private set => SetProperty(ref _edcAmps, value); }
  public double PackageC2Pct { get => _packageC2Pct; private set => SetProperty(ref _packageC2Pct, value); }
  public double PackageC3Pct { get => _packageC3Pct; private set => SetProperty(ref _packageC3Pct, value); }
  public double PackageC6Pct { get => _packageC6Pct; private set => SetProperty(ref _packageC6Pct, value); }
  public double PackageC7Pct { get => _packageC7Pct; private set => SetProperty(ref _packageC7Pct, value); }
  public double Temperature { get => _temperature; private set => SetProperty(ref _temperature, value); }
  public double DistanceToTjMax { get => _distanceToTjMax; private set => SetProperty(ref _distanceToTjMax, value); }
  public bool IsThrottling { get => _isThrottling; private set => SetProperty(ref _isThrottling, value); }
  public string ThrottleStatus { get => _throttleStatus; private set => SetProperty(ref _throttleStatus, value); }
  public int FanRpm { get => _fanRpm; private set { if (SetProperty(ref _fanRpm, value)) RaiseFanReadoutChanged(); } }
  public bool HasCpuFan { get => _hasCpuFan; private set { if (SetProperty(ref _hasCpuFan, value)) RaiseFanReadoutChanged(); } }
  public double FanPercent { get => _fanPercent; private set { if (SetProperty(ref _fanPercent, value)) RaiseFanReadoutChanged(); } }
  public bool HasCpuFanPercent { get => _hasCpuFanPercent; private set { if (SetProperty(ref _hasCpuFanPercent, value)) RaiseFanReadoutChanged(); } }
  public bool MsrSensorsAvailable { get => _msrSensorsAvailable; private set => SetProperty(ref _msrSensorsAvailable, value); }

  // Default to the RPM readout (showing 0 before any reading); only fall back to the percentage
  // readout when a percentage arrived and no tachometer did (laptops behind the embedded controller).
  private bool ShowPercent => HasCpuFanPercent && !HasCpuFan;

  /// <summary>The fan readout value: RPM by default, or the fan-speed percentage on tachometer-less laptops.</summary>
  public double FanReadoutValue => ShowPercent ? FanPercent : FanRpm;

  /// <summary>The fan readout, preformatted: whole RPM (e.g. "1,200") or whole percent (e.g. "45").</summary>
  public string FanReadoutText => ShowPercent ? FanPercent.ToString("0") : FanRpm.ToString("N0");

  /// <summary>The fan readout unit: "RPM" by default, "%" on tachometer-less laptops.</summary>
  public string FanReadoutUnit => ShowPercent ? "%" : "RPM";

  /// <summary>The fan history graph's upper bound: 100 for a percentage, otherwise 4000 RPM.</summary>
  public double FanGraphMax => ShowPercent ? 100 : 4000;

  private void RaiseFanReadoutChanged() {
    RaisePropertyChanged(nameof(FanReadoutValue));
    RaisePropertyChanged(nameof(FanReadoutText));
    RaisePropertyChanged(nameof(FanReadoutUnit));
    RaisePropertyChanged(nameof(FanGraphMax));
  }

  public ObservableCollection<CoreLoadViewModel> CoreLoads { get; } = [];

  public void AttachGraphs(PerformanceGraph? utilization = null, PerformanceGraph? voltage = null,
                           PerformanceGraph? clock = null, PerformanceGraph? power = null,
                           PerformanceGraph? temperature = null, PerformanceGraph? fan = null) {
    _utilizationGraph = utilization;
    _voltageGraph = voltage;
    _clockGraph = clock;
    _powerGraph = power;
    _temperatureGraph = temperature;
    _fanGraph = fan;
  }

  public void Update(ISystemCpuInfo info) {
    var socket = info.Sockets.FirstOrDefault();
    if (socket is null) return;

    var sensors = socket.Sensors;

    Load = sensors.TotalLoad.Value ?? 0;
    Voltage = sensors.Voltage.Value ?? 0;
    SocVoltage = sensors.SocVoltage.Value ?? 0;
    // CpuSpeed reads in MHz; the Speed gauge/graph are scaled in GHz.
    SpeedGhz = (sensors.CpuSpeed.Value ?? 0) / 1000.0;
    EffectiveSpeedGhz = (sensors.CpuEffectiveSpeed.Value ?? 0) / 1000.0;
    BusSpeedMHz = sensors.BusSpeed.Value ?? 0;
    Power = sensors.PackagePower.Value ?? 0;
    PowerLimitLongW = sensors.PowerLimitLong.Value ?? 0;
    PowerLimitShortW = sensors.PowerLimitShort.Value ?? 0;
    TdcAmps = sensors.Tdc.Value ?? 0;
    EdcAmps = sensors.Edc.Value ?? 0;
    PackageC2Pct = sensors.PackageC2Residency.Value ?? 0;
    PackageC3Pct = sensors.PackageC3Residency.Value ?? 0;
    PackageC6Pct = sensors.PackageC6Residency.Value ?? 0;
    PackageC7Pct = sensors.PackageC7Residency.Value ?? 0;
    Temperature = sensors.PackageTemperature.Value ?? 0;
    DistanceToTjMax = sensors.MinDistanceToTjMax.Value ?? 0;
    UpdateThrottleStatus(sensors);

    // Latch once any MSR-backed reading arrives: these are empty without the ring-0
    // driver, so a single non-null value proves it is present and lets the view drop
    // the "MSR driver not available" notice.
    if (!MsrSensorsAvailable
        && (sensors.Voltage.Value is not null
            || sensors.CpuSpeed.Value is not null
            || sensors.PackagePower.Value is not null
            || sensors.PackageTemperature.Value is not null)) {
      MsrSensorsAvailable = true;
    }

    UpdateCoreLoads(socket.Cores);

    _utilizationGraph?.AddValue(Load);
    _voltageGraph?.AddValue(Voltage);
    _clockGraph?.AddValue(SpeedGhz);
    _powerGraph?.AddValue(Power);
    _temperatureGraph?.AddValue(Temperature);
  }

  // Latch HasCpuFan once a fan is seen: a machine with a CPU fan header keeps the readout even if
  // a single poll momentarily reports null, avoiding the row flickering in and out.
  public void UpdateFan(float? rpm) {
    if (rpm is not { } value) return;
    HasCpuFan = true;
    FanRpm = (int)value;
    _fanGraph?.AddValue(FanRpm);
  }

  // Fan-speed percentage fallback for laptops with no tachometer. Latches HasCpuFanPercent like the
  // RPM path so the readout survives a momentary null. Only feeds the graph when RPM is absent —
  // when a tachometer exists the graph plots RPM and the percentage is not shown.
  public void UpdateFanPercent(float? percent) {
    if (percent is not { } value) return;
    HasCpuFanPercent = true;
    FanPercent = value;
    if (!HasCpuFan) _fanGraph?.AddValue(FanPercent);
  }

  // Core count is fixed for a given CPU, so the rows are created once (labelled C00, C01, …)
  // and their Load is updated in place — avoids clearing/rebuilding the bound collection on
  // every 1-second emission, which would reset selection and churn the visual tree.
  private void UpdateCoreLoads(IReadOnlyList<ICoreInfo> cores) {
    for (int i = 0; i < cores.Count; i++) {
      var s = cores[i].Sensors;
      var row = i < CoreLoads.Count ? CoreLoads[i] : AddCoreRow(i);
      row.Load = s.Load.Value ?? 0;
      // Per-core clocks read in MHz; the table shows GHz to match the aggregate gauges.
      row.SpeedGhz = (s.Speed.Value ?? 0) / 1000.0;
      row.EffectiveSpeedGhz = (s.EffectiveSpeed.Value ?? 0) / 1000.0;
      row.Multiplier = s.Multiplier.Value ?? 0;
      row.DistanceToTjMax = s.DistanceToTjMax.Value ?? 0;
      row.Power = s.Power.Value ?? 0;
      UpdateThreadLoads(row, s.ThreadLoads);
    }
  }

  // Package throttle status. Prefers the provider's decoded flags; when the thermal flag
  // is unavailable (no MSR / non-Intel), falls back to a thermal-headroom check — but only
  // when the distance-to-TjMax sensor actually reports a value, so an unexposed 0 can't
  // masquerade as "at TjMax".
  private void UpdateThrottleStatus(Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus.ICpuSensors sensors) {
    bool thermal = sensors.ThermalThrottling.Value is { } t
        ? t >= 0.5
        : sensors.MinDistanceToTjMax.Value is { } d && d <= 0;
    bool powerLimit = sensors.PowerLimitThrottling.Value is { } p && p >= 0.5;
    bool prochot = sensors.Prochot.Value is { } pr && pr >= 0.5;

    var reasons = new List<string>(3);
    if (thermal) reasons.Add("Thermal");
    if (powerLimit) reasons.Add("Power Limit");
    if (prochot) reasons.Add("PROCHOT");

    IsThrottling = reasons.Count > 0;
    ThrottleStatus = IsThrottling ? "THROTTLING: " + string.Join(", ", reasons) : string.Empty;
  }

  // Per-thread bars are created once (thread count is fixed per core) and their Load
  // is updated in place, matching the core-row lifetime.
  private static void UpdateThreadLoads(CoreLoadViewModel row, IReadOnlyList<SensorReading> threads) {
    for (int t = 0; t < threads.Count; t++) {
      var thread = t < row.Threads.Count ? row.Threads[t] : AddThreadRow(row);
      thread.Load = threads[t].Value ?? 0;
    }
  }

  private static ThreadLoadViewModel AddThreadRow(CoreLoadViewModel row) {
    var thread = new ThreadLoadViewModel();
    row.Threads.Add(thread);
    return thread;
  }

  private CoreLoadViewModel AddCoreRow(int index) {
    var row = new CoreLoadViewModel($"C{index:00}");
    CoreLoads.Add(row);
    return row;
  }
}
