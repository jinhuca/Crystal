using Crystal;
using System.Windows;


namespace C0503ActivationDeactivation.Views
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

		private void Activate_ViewA_Click(object sender, RoutedEventArgs e)
		{
			_region.Activate(_viewA);
		}

		private void Deactivate_ViewA_Click(object sender, RoutedEventArgs e)
		{
			_region.Deactivate(_viewA);
		}

		private void Activate_ViewB_Click(object sender, RoutedEventArgs e)
		{
			_region.Activate(_viewB);
		}

		private void Deactivate_ViewB_Click(object sender, RoutedEventArgs e)
		{
			_region.Deactivate(_viewB);
		}
	}
}
