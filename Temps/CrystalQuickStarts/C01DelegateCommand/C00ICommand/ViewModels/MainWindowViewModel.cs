using C00ICommand.Commands;
using Crystal;

namespace C01UseDelegateCommand.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private bool _isEnabled;
		public bool IsEnabled
		{
			get => _isEnabled;
			set {
				SetProperty(ref _isEnabled, value);
				
			}
		}

		private string _updateText;
		public string UpdateText
		{
			get => _updateText;
			set => SetProperty(ref _updateText, value);
		}

		public MyAwesomeCommand MyCommand { get; private set; }

		public MainWindowViewModel()
		{
			MyCommand = new MyAwesomeCommand(this);
		}
	}
}
