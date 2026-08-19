namespace Crystal.CpuModule.ViewModels;

/// <summary>
/// One logical thread's load (%) within a core row, refreshed in place each emission.
/// </summary>
public sealed class ThreadLoadViewModel : BindableBase {
  /// <summary>
  /// the load percentage for the thread.
  /// </summary>
  private double _load;

  /// <summary>
  /// Gets or sets the load percentage for the thread.
  /// </summary>
  public double Load { get => _load; set => SetProperty(ref _load, value); }
}
