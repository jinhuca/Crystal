namespace CpuModule.ViewModels;

/// <summary>One row in the per-core load list: a stable core label (e.g. "C00") and the
/// core's most recent load percentage, refreshed in place on every sensor emission.</summary>
public sealed class CoreLoadViewModel : BindableBase {
  private double _load;

  public CoreLoadViewModel(string label) => Label = label;

  public string Label { get; }
  public double Load { get => _load; set => SetProperty(ref _load, value); }
}
