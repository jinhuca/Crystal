using Crystal.Mvvm;

namespace CustomRegistration.ViewModels
{
	public class CustomViewModel : BindableBase
	{
		private string _title = "Custom ViewModel";
		public string Title
		{
			get { return _title; }
			set { SetProperty(ref _title, value); }
		}
		public CustomViewModel()
		{
		}
	}
}
