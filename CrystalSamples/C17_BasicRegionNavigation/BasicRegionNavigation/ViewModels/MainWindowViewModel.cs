using Crystal;

namespace BasicRegionNavigation.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private readonly IRegionManager regionManager;

		public DelegateCommand<string> NavigateCommand { get; private set; }

		public MainWindowViewModel(IRegionManager regionManagerInstance)
		{
			regionManager = regionManagerInstance;
			NavigateCommand = new DelegateCommand<string>(Navigate);
		}

		private void Navigate(string navigatePath)
		{
			if (navigatePath != null)
			{
				regionManager.RequestNavigate(RegionNames.MainContentRegionName, navigatePath);
			}
		}
	}
}
