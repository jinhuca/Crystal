using Crystal.Mvvm;

namespace UsingEventAggregator.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private string _title = "Crystal Unity Application";
		public string Title
		{
			get { return _title; }
			set { SetProperty(ref _title, value); }
		}

		public MainWindowViewModel()
		{

		}
	}
}
