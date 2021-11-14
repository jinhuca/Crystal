using Crystal;
using System;

namespace GenericDelegateCommand.ViewModels
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

    private string _id = string.Empty;
    public string Id
    {
      get => _id;
      set => SetProperty(ref _id, value);
    }

    public DelegateCommand<string> GenericDelegateCommand { get; set; }

    public MainWindowViewModel()
    {
      GenericDelegateCommand = new DelegateCommand<string>(Execute).ObservesCanExecute(() => IsEnabled);
      Id = "\nHello Generic DelegateCommand";
    }

    private void Execute(string obj) => UpdateText = $"Updated: {DateTime.Now} by {obj}";
  }
}
