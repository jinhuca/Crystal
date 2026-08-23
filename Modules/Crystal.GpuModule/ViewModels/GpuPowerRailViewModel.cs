namespace Crystal.GpuModule.ViewModels;

/// <summary>
/// One rail row in the per-adapter power breakdown. The name is fixed for the life of the
/// row; only <see cref="PowerW"/> ticks each poll.
/// </summary>
public sealed class GpuPowerRailViewModel : BindableBase {
  private double _powerW;

  public GpuPowerRailViewModel(string name) => Name = name;

  public string Name { get; }
  public double PowerW { get => _powerW; set => SetProperty(ref _powerW, value); }
}
