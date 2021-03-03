using Crystal.Commands;
using Crystal.Mvvm;
using System;

namespace C01UseDelegateCommand.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private bool _isEnabled;
		public bool IsEnabled
		{
			get => _isEnabled;
			set => SetProperty(ref _isEnabled, value);
		}

		private string _updateText;
		public string UpdateText
		{
			get => _updateText;
			set => SetProperty(ref _updateText, value);
		}

		public DelegateCommand MyAwesomeCommand { get; private set; }

		public MainWindowViewModel()
		{
			MyAwesomeCommand = new DelegateCommand(Execute).ObservesCanExecute(() => IsEnabled);
		}

		private void Execute()
		{
			UpdateText = $"Updated {DateTime.Now}";
		}
	}
}
