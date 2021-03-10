using C04ViewDiscovery.Views;
using Crystal;
using Crystal.Unity;
using System.Windows;

namespace C04ViewDiscovery
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
