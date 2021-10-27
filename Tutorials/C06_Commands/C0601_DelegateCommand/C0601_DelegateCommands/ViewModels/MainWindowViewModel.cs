using System;
using Crystal;

namespace C0601_DelegateCommands.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private bool _isEnabled;
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				SetProperty(ref _isEnabled, value);
				ExecuteDelegateCommand.RaiseCanExecuteChanged();
			}
		}

		private string _updateText;
		public string UpdateText
		{
			get => _updateText;
			set => SetProperty(ref _updateText, value);
		}


		public DelegateCommand ExecuteDelegateCommand { get; }

		public DelegateCommand<string> ExecuteGenericDelegateCommand { get; }

		public DelegateCommand DelegateCommandObservesProperty { get; }

		public DelegateCommand DelegateCommandObservesCanExecute { get; }


		public MainWindowViewModel()
		{
			ExecuteDelegateCommand = new DelegateCommand(Execute, CanExecute);
			DelegateCommandObservesProperty = new DelegateCommand(Execute, CanExecute).ObservesProperty(() => IsEnabled);
			DelegateCommandObservesCanExecute = new DelegateCommand(Execute).ObservesCanExecute(() => IsEnabled);
			ExecuteGenericDelegateCommand = new DelegateCommand<string>(ExecuteGeneric).ObservesCanExecute(() => IsEnabled);
		}

		private void Execute()
		{
			UpdateText = $"Updated: {DateTime.Now}";
		}

		private void ExecuteGeneric(string parameter)
		{
			UpdateText = parameter;
		}

		private bool CanExecute()
		{
			return IsEnabled;
		}
	}
}
