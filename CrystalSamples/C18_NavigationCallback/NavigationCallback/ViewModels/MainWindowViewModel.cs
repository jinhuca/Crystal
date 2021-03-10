using Crystal;

namespace BasicRegionNavigation.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private readonly IRegionManager _regionManager;

		private string _title = "Crystal Unity Application";
		public string Title
		{
			get { return _title; }
			set { SetProperty(ref _title, value); }
		}

		public DelegateCommand<string> NavigateCommand { get; private set; }

		public MainWindowViewModel(IRegionManager regionManager)
		{
			_regionManager = regionManager;
			NavigateCommand = new DelegateCommand<string>(Navigate);
		}

		private void Navigate(string navigatePath)
		{
			if (navigatePath != null)
			{
				_regionManager.RequestNavigate("ContentRegion", navigatePath, NavigationComplete);
			}
		}

		private void NavigationComplete(NavigationResult result)
		{
			System.Windows.MessageBox.Show($"Navigation to {result.Context.Uri} complete. ");
		}
	}
}
