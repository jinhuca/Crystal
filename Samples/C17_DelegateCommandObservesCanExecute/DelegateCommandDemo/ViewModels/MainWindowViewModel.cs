using Crystal;
using System;

namespace DelegateCommandDemo.ViewModels
{
  internal class MainWindowViewModel : BindableBase
  {
    private bool _isEnabled;
    public bool IsEnabled
    {
      get => _isEnabled;
      set => SetProperty(ref _isEnabled, value);
    }

    private string _updateText = string.Empty;
    public string UpdateText
    {
      get => _updateText;
      set => SetProperty(ref _updateText, value);
    }

    public DelegateCommand DelegateCommandObservesCanExecute { get; set; }

    public MainWindowViewModel()
    {
      DelegateCommandObservesCanExecute = new DelegateCommand(Execute).ObservesCanExecute(() => IsEnabled);
    }

    private void Execute() => UpdateText = $"Updated: {DateTime.Now}";
  }
}
