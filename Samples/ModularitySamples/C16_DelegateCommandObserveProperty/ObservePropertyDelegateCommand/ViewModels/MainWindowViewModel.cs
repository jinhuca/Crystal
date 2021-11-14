using Crystal;
using System;

namespace ObservePropertyDelegateCommand.ViewModels
{
  internal class MainWindowViewModel : BindableBase
  {
    private bool _isEnabled;
    public bool IsEnabled
    {
      get => _isEnabled;
      set => SetProperty(ref _isEnabled, value);
    }

    private bool _isAnotherEnabled;
    public bool IsAnotherEnabled
    {
      get => _isAnotherEnabled;
      set => SetProperty(ref _isAnotherEnabled, value);
    }

    private string _updateText = string.Empty;
    public string UpdateText
    {
      get { return _updateText; }
      set { SetProperty(ref _updateText, value); }
    }

    public DelegateCommand DelegateCommandObservesProperty { get; private set; }

    public MainWindowViewModel()
    {
      DelegateCommandObservesProperty = new DelegateCommand(Execute, CanExecute).ObservesProperty(() => IsAnotherEnabled);
    }

    private void Execute() => UpdateText = $"Updated: {DateTime.Now}";

    private bool CanExecute() => IsEnabled;
  }
}
