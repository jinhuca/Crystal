namespace Crystal.GpuModule.ViewModels;

/// <summary>
/// One rail row in the per-adapter power breakdown. The name is fixed for the life of the
/// row; only <see cref="PowerW"/> ticks each poll.
/// </summary>
public sealed class GpuPowerRailViewModel(string name) : BindableBase {
  /// <summary>
  /// Gets or sets the power in watts.
  /// </summary>
  private double _powerW;

  /// <summary>
  /// Initializes a new instance of the <see cref="GpuPowerRailViewModel"/> class.
  /// </summary>
  public string Name { get; } = name;

  /// <summary>
  /// Gets or sets the power in watts.
  /// </summary>
  public double PowerW { get => _powerW; set => SetProperty(ref _powerW, value); }
}
