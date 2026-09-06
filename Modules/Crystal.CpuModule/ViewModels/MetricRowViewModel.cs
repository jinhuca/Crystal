namespace Crystal.CpuModule.ViewModels;

/// <summary>
/// One row in a CPU metric breakdown table: a fixed label (e.g. a power rail, a clock domain, a
/// temperature sensor) with its current value and the session min/max, in the tile's unit. Updated
/// in place on every sensor emission; a reading the platform doesn't expose stays at zero.
/// </summary>
public sealed class MetricRowViewModel : BindableBase {
  private double _value;
  private double _min;
  private double _max;

  public MetricRowViewModel(string label) => Label = label;

  /// <summary>The row label (e.g. Package / Cores / GT / DRAM, Core / Effective, Core Max / Core Avg).</summary>
  public string Label { get; }

  /// <summary>This row's current reading, in the tile's unit.</summary>
  public double Value { get => _value; set => SetProperty(ref _value, value); }

  /// <summary>This row's session-minimum reading, in the tile's unit.</summary>
  public double Min { get => _min; set => SetProperty(ref _min, value); }

  /// <summary>This row's session-maximum reading, in the tile's unit.</summary>
  public double Max { get => _max; set => SetProperty(ref _max, value); }
}
