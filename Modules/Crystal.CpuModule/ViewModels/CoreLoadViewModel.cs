using System.Collections.ObjectModel;

namespace Crystal.CpuModule.ViewModels;

/// <summary>
/// One row in the per-core list: a stable core label (e.g. "C00") and the core's most recent readings, 
/// refreshed in place on every sensor emission. The summary tile binds only 
/// <see cref="Label"/>/<see cref="Load"/>; the detail view's per-core table also shows clock, 
/// effective clock and multiplier (the latter two AMD-only, so zero elsewhere).
/// </summary>
public sealed class CoreLoadViewModel : BindableBase {

  /// <summary>
  /// The load percentage for the core.
  /// </summary>
  private double _load;

  /// <summary>
  /// The clock speed in GHz for the core.
  /// </summary>
  private double _speedGhz;

  /// <summary>
  /// The effective clock speed in GHz for the core, weighted by C-state residency; 
  /// lower than <see cref="SpeedGhz"/> when cores idle.
  /// </summary>
  private double _effectiveSpeedGhz;

  /// <summary>
  /// The multiplier for the core (e.g. 36 for 3.6 GHz on a 100 MHz bus). AMD-only; zero elsewhere.
  /// </summary>
  private double _multiplier;

  /// <summary>
  /// The headroom to TjMax in °C for the core. Intel-only; zero elsewhere.
  /// </summary>
  private double _distanceToTjMax;

  /// <summary>
  /// The power in W for the core. AMD-only; zero elsewhere.
  /// </summary>
  private double _power;

  /// <summary>
  /// The temperature in °C for the core.
  /// </summary>
  private double _temperature;

  /// <summary>
  /// Initializes a new instance of the <see cref="CoreLoadViewModel"/> class with the specified core label.
  /// </summary>
  /// <param name="label"></param>
  public CoreLoadViewModel(string label) => Label = label;

  /// <summary>
  /// The stable core label (e.g. "C00") used in the summary tile and detail table.
  /// </summary>
  public string Label { get; }

  /// <summary>
  /// This core's load percentage, 0–100. Updated in place on every sensor emission.
  /// </summary>
  public double Load { get => _load; set => SetProperty(ref _load, value); }

  /// <summary>
  /// This core's clock in GHz. Zero when not exposed.
  /// </summary>
  public double SpeedGhz { get => _speedGhz; set => SetProperty(ref _speedGhz, value); }

  /// <summary>
  /// This core's effective clock in GHz, weighted by C-state residency; lower than <see cref="SpeedGhz"/> when cores idle. 
  /// Zero when not exposed.
  /// </summary>
  public double EffectiveSpeedGhz { get => _effectiveSpeedGhz; set => SetProperty(ref _effectiveSpeedGhz, value); }

  /// <summary>
  /// This core's multiplier (e.g. 36 for 3.6 GHz on a 100 MHz bus). AMD-only; zero elsewhere.
  /// </summary>
  public double Multiplier { get => _multiplier; set => SetProperty(ref _multiplier, value); }

  /// <summary>
  /// This core's temperature in °C.
  /// </summary>
  public double Temperature { get => _temperature; set => SetProperty(ref _temperature, value); }

  /// <summary>
  /// This core's headroom to TjMax in °C. Intel-only; zero elsewhere.
  /// </summary>
  public double DistanceToTjMax { get => _distanceToTjMax; set => SetProperty(ref _distanceToTjMax, value); }

  /// <summary>
  /// This core's power in W. AMD-only; zero elsewhere.
  /// </summary>
  public double Power { get => _power; set => SetProperty(ref _power, value); }

  /// <summary>
  /// Per-logical-thread loads on this core (one bar each in the detail table). Created
  /// once and updated in place, mirroring the core-row lifetime.
  /// </summary>
  public ObservableCollection<ThreadLoadViewModel> Threads { get; } = [];
}
