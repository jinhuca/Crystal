namespace Crystal.GpuModule.ViewModels;

/// <summary>
/// One rail row in the per-adapter power breakdown. The name is fixed for the life of the
/// row; only <see cref="PowerW"/> ticks each poll.
/// </summary>
public sealed class GpuPowerRailViewModel(string name) : BindableBase {
  private double _powerW;
  private double? _minW;
  private double? _maxW;

  /// <summary>
  /// Initializes a new instance of the <see cref="GpuPowerRailViewModel"/> class.
  /// </summary>
  public string Name { get; } = name;

  /// <summary>
  /// Gets or sets the power in watts.
  /// </summary>
  public double PowerW { get => _powerW; set => SetProperty(ref _powerW, value); }

  /// <summary>Lowest power this rail has drawn this session, in watts (null until recorded).</summary>
  public double? MinW { get => _minW; set => SetProperty(ref _minW, value); }

  /// <summary>Highest power this rail has drawn this session, in watts (null until recorded).</summary>
  public double? MaxW { get => _maxW; set => SetProperty(ref _maxW, value); }
}
