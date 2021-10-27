using Crystal;

namespace C0304_RegionContextApp.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private string _title = "Crystal Unity Application";
		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}

		public MainWindowViewModel()
		{

		}
	}
}
