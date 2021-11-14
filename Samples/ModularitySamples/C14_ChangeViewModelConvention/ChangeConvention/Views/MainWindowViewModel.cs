using Crystal;

namespace ChangeConvention.Views
{
	internal class MainWindowViewModel : BindableBase
	{
		private string _title = "Crystal Application";
		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}
	}
}
