using C05ViewInjection.Views;
using Crystal.Ioc;
using Crystal.Regions;
using System.Windows;

namespace C05ViewInjection
{
	public partial class MainWindow : Window
	{
		IContainerExtension _container;
		IRegionManager _regionManager;

		public MainWindow(IContainerExtension container, IRegionManager regionManager)
		{
			InitializeComponent();
			_container = container;
			_regionManager = regionManager;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var view = _container.Resolve<ViewA>();
			IRegion region = _regionManager.Regions["ContentRegion"];
			region.Add(view);
		}
	}
}
