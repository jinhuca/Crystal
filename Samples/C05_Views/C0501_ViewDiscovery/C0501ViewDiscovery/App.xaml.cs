using C0501ViewDiscovery.Views;
using Crystal;
using Crystal.Unity;
using System.Windows;

namespace C0501ViewDiscovery
{
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
