using Crystal.Commands;
using Crystal.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleDelegateCommand.ViewModels
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
			}
		}

		private string _updateText;
		public string UpdateText
		{
			get { return _updateText; }
			set { SetProperty(ref _updateText, value); }
		}

		public DelegateCommand MyAwesomeCommand { get; private set; }

		public MainWindowViewModel()
		{
			MyAwesomeCommand = new DelegateCommand(Execute).ObservesCanExecute(() => IsEnabled);
		}

		//private bool CanExecute()
		//{
		//	return IsEnabled;
		//}

		private void Execute()
		{
			UpdateText = $"Updated: {DateTime.Now}";
		}
	}
}
