using Crystal;
using System;

namespace UsingDelegateCommands.ViewModels
{
  internal class MainWindowViewModel : BindableBase
  {
    private bool _isEnabled;
    public bool IsEnabled
    {
      get => _isEnabled;
      set
      {
        SetProperty(ref _isEnabled, value);
        ExecuteDelegateCommand.RaiseCanExecuteChanged();
      }
    }

    private string _updateText = string.Empty;
    public string UpdateText
    {
      get => _updateText;
      set => SetProperty(ref _updateText, value);
    }

    public DelegateCommand ExecuteDelegateCommand { get; set; }

    public MainWindowViewModel()
    {
      ExecuteDelegateCommand = new DelegateCommand(Execute, CanExecute);
    }

    private bool CanExecute() => IsEnabled;

    private void Execute() => UpdateText = $"Updated: {DateTime.Now}";
  }
}
