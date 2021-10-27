using Crystal;

namespace C0403_ViewActivationDeactivation.Views
{
	public partial class MainWindow
	{
		private readonly IContainerExtension _container;
		private readonly IRegionManager _regionManager;
		private IRegion _region;
		private ViewA _viewA;
		private ViewB _viewB;

		public MainWindow(IContainerExtension container, IRegionManager regionManager)
		{
			InitializeComponent();
			_container = container;
			_regionManager = regionManager;
			Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			_viewA = _container.Resolve<ViewA>();
			_viewB = _container.Resolve<ViewB>();
			_region = _regionManager.Regions["ContentRegion"];
			_region.Add(_viewA);
			_region.Add(_viewB);
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_region.Activate(_viewA);
		}

		private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
		{
			_region.Deactivate(_viewA);
		}

		private void Button_Click_2(object sender, System.Windows.RoutedEventArgs e)
		{
			_region.Activate(_viewB);
		}

		private void Button_Click_3(object sender, System.Windows.RoutedEventArgs e)
		{
			_region.Deactivate(_viewB);
		}
	}
}
