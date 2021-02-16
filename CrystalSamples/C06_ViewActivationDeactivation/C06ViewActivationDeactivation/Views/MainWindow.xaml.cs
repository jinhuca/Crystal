using Crystal.Ioc;
using Crystal.Regions;
using System.Windows;

namespace C06ViewActivationDeactivation.Views
{
	public partial class MainWindow : Window
	{
		IContainerExtension _container;
		IRegionManager _regionManager;
		IRegion _region;

		ViewA _viewA;
		ViewB _viewB;

		public MainWindow(IContainerExtension container, IRegionManager regionManager)
		{
			InitializeComponent();
			_container = container;
			_regionManager = regionManager;
			Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			_viewA = _container.Resolve<ViewA>();
			_viewB = _container.Resolve<ViewB>();
			_region = _regionManager.Regions["ContentRegion"];
			_region.Add(_viewA);
			_region.Add(_viewB);
		}

		private void ActivateViewA_Click(object sender, RoutedEventArgs e)
		{
			_region.Activate(_viewA);
		}

		private void DeactivateViewA_Click(object sender, RoutedEventArgs e)
		{
			_region.Deactivate(_viewA);
		}

		private void ActivateViewB_Click(object sender, RoutedEventArgs e)
		{
			_region.Activate(_viewB);
		}

		private void DeactivateViewB_Click(object sender, RoutedEventArgs e)
		{
			_region.Deactivate(_viewB);
		}
	}
}
