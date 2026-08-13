namespace CpuModule.ViewModels;

/// <summary>One logical thread's load (%) within a core row, refreshed in place each emission.</summary>
public sealed class ThreadLoadViewModel : BindableBase {
  private double _load;
  public double Load { get => _load; set => SetProperty(ref _load, value); }
}
