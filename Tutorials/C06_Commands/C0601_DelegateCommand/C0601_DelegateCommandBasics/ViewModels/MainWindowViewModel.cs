using Crystal;

namespace C0601_DelegateCommandBasics.ViewModels
{
  public class MainWindowViewModel : BindableBase
  {
    private int _count;
    public int Count
    {
      get => _count;
      set => SetProperty(ref _count, value);
    }

    private bool _isCommandEnabled;
    public bool IsCommandEnabled
    {
      get => _isCommandEnabled;
      set => SetProperty(ref _isCommandEnabled, value);
    }

    private bool _anotherEnabled;

    public bool AnotherEnabled
    {
      get => _anotherEnabled;
      set => SetProperty(ref _anotherEnabled, value);
    }

    public DelegateCommand AddCountCommand { get; }

    private void ExecuteAddCount() => Count++;

    private bool CanExecuteAddCount() => IsCommandEnabled;

    public MainWindowViewModel()
    {
      AddCountCommand = new DelegateCommand(ExecuteAddCount, CanExecuteAddCount)
        .ObservesProperty(() => IsCommandEnabled);
    }
  }
}
