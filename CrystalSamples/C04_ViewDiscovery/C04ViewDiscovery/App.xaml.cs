using C04ViewDiscovery.Views;
using Crystal.Ioc;
using Crystal.Unity;
using System.Windows;

namespace C04ViewDiscovery
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}
	}
}
