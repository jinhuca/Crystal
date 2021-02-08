using Crystal.Regions;
using System.Windows;

namespace C04ViewDiscovery.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow(IRegionManager regionManager)
		{
			InitializeComponent();
			regionManager.RegisterViewWithRegion("ContentRegion", typeof(ViewA));
		}
	}
}
