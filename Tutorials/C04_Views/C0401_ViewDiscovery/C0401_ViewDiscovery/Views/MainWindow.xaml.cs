using C0401_ViewDiscovery.Views;
using Crystal;

namespace C0401_ViewDiscovery
{
	public partial class MainWindow
	{
		public MainWindow(IRegionManager regionManager)
		{
			InitializeComponent();
			regionManager.RegisterViewWithRegion("ContentRegion", typeof(ViewA));
		}
	}
}
