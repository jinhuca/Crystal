using System.Collections.ObjectModel;

namespace Crystal.CpuModule.ViewModels;

/// <summary>One row in the per-core list: a stable core label (e.g. "C00") and the core's
/// most recent readings, refreshed in place on every sensor emission. The summary tile binds
/// only <see cref="Label"/>/<see cref="Load"/>; the detail view's per-core table also shows
/// clock, effective clock and multiplier (the latter two AMD-only, so zero elsewhere).</summary>
public sealed class CoreLoadViewModel : BindableBase {
  private double _load;
  private double _speedGhz;
  private double _effectiveSpeedGhz;
  private double _multiplier;
  private double _distanceToTjMax;
  private double _power;

  public CoreLoadViewModel(string label) => Label = label;

  public string Label { get; }
  public double Load { get => _load; set => SetProperty(ref _load, value); }
  public double SpeedGhz { get => _speedGhz; set => SetProperty(ref _speedGhz, value); }
  public double EffectiveSpeedGhz { get => _effectiveSpeedGhz; set => SetProperty(ref _effectiveSpeedGhz, value); }
  public double Multiplier { get => _multiplier; set => SetProperty(ref _multiplier, value); }

  /// <summary>This core's headroom to TjMax in °C. Intel-only; zero elsewhere.</summary>
  public double DistanceToTjMax { get => _distanceToTjMax; set => SetProperty(ref _distanceToTjMax, value); }

  /// <summary>This core's power in W. AMD-only; zero elsewhere.</summary>
  public double Power { get => _power; set => SetProperty(ref _power, value); }

  /// <summary>Per-logical-thread loads on this core (one bar each in the detail table). Created
  /// once and updated in place, mirroring the core-row lifetime.</summary>
  public ObservableCollection<ThreadLoadViewModel> Threads { get; } = [];
}
