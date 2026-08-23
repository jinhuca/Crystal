namespace Crystal.GpuModule.ViewModels;

/// <summary>
/// One engine row in the per-adapter utilization breakdown. The name is fixed for the
/// life of the row; only <see cref="LoadPercent"/> ticks each poll.
/// </summary>
public sealed class GpuEngineLoadViewModel : BindableBase {
  /// <summary>
  /// Gets or sets the load percentage.
  /// </summary>
  private double _loadPercent;

  /// <summary>
  /// Initializes a new instance of the <see cref="GpuEngineLoadViewModel"/> class.
  /// </summary>
  /// <param name="name">The name of the engine load view model.</param>
  public GpuEngineLoadViewModel(string name) => Name = name;

  /// <summary>
  /// Gets the name of the engine load view model.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets or sets the load percentage.
  /// </summary>
  public double LoadPercent {
    get => _loadPercent;
    set => SetProperty(ref _loadPercent, value);
  }
}
