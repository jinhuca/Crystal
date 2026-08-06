using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace Crystal.Shell.Startup;

/// <summary>
/// Backs the startup loading overlay. Exposes the current status line, overall percent, and a
/// per-component checklist. <see cref="Report"/> is fed from the loader's <c>IProgress</c>, which
/// marshals onto the UI thread, so property changes are safe to raise directly here.
/// </summary>
public sealed class LoadingViewModel : BindableBase {
  private string _status = "Starting Crystal...";
  private double _percent;

  public LoadingViewModel(IEnumerable<string> componentNames) {
    foreach (var name in componentNames)
      Components.Add(new LoadingComponentViewModel(name));
  }

  public ObservableCollection<LoadingComponentViewModel> Components { get; } = [];

  public string Status {
    get => _status;
    private set => SetProperty(ref _status, value);
  }

  public double Percent {
    get => _percent;
    private set => SetProperty(ref _percent, value);
  }

  /// <summary>Applies a progress report: updates the matching row, status line and percent.</summary>
  public void Report(StartupProgress progress) {
    var row = Components.FirstOrDefault(c => c.Name == progress.Name);
    if (row is not null)
      row.State = progress.State;

    Percent = progress.Percent;
    Status = progress.State == StartupComponentState.Loading
        ? $"Loading {progress.Name}..."
        : $"Loaded {progress.Completed} of {progress.Total} components";
  }
}
