using Crystal;
using System.Windows;

namespace C0501ViewDiscovery.Views
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
