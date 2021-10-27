using System.Windows;
using Crystal;

namespace C0401_ViewInjection.Views
{
	public partial class MainWindow
	{
		private readonly IContainerExtension _containerExtension;
		private readonly IRegionManager _regionManager;

		public MainWindow(IContainerExtension container, IRegionManager regionManager)
		{
			InitializeComponent();
			_containerExtension = container;
			_regionManager = regionManager;
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			var view = _containerExtension.Resolve<ViewA>();
			IRegion region = _regionManager.Regions["ContentRegion"];
			region.Add(view);
		}
	}
}
