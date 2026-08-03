using Prism.Mvvm;

namespace Crystal.NewShell.Startup;

/// <summary>One row in the loading checklist: a component name and its live state.</summary>
public sealed class LoadingComponentViewModel : BindableBase {
  private StartupComponentState _state = StartupComponentState.Pending;

  public LoadingComponentViewModel(string name) => Name = name;

  public string Name { get; }

  public StartupComponentState State {
    get => _state;
    set {
      if (SetProperty(ref _state, value)) {
        RaisePropertyChanged(nameof(IsLoading));
        RaisePropertyChanged(nameof(IsCompleted));
        RaisePropertyChanged(nameof(IsFailed));
      }
    }
  }

  public bool IsLoading => _state == StartupComponentState.Loading;
  public bool IsCompleted => _state == StartupComponentState.Completed;
  public bool IsFailed => _state == StartupComponentState.Failed;
}
