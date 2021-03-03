using C01UseDelegateCommand.ViewModels;
using System;
using System.Windows.Input;

namespace C00ICommand.Commands
{
	public class MyAwesomeCommand : RoutedCommand
	{
		private MainWindowViewModel viewModel;

		public MyAwesomeCommand(MainWindowViewModel vm)
		{
			viewModel = vm;
		}

		public event EventHandler CanExecuteChanged;


		public bool CanExecute(object parameter)
		{
			return viewModel.IsEnabled;
		}

		public void Execute(object parameter)
		{
			viewModel.UpdateText = $"Update: {DateTime.Now}";
		}
	}
}
