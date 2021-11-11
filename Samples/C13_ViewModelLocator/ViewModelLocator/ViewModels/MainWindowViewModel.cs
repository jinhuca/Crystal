using Crystal;

namespace ViewModelLocator.ViewModels
{
	internal class MainWindowViewModel : BindableBase
	{
		private string _title;
		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}

		public MainWindowViewModel()
		{
			_title = "Composite Application";
		}
	}
}
