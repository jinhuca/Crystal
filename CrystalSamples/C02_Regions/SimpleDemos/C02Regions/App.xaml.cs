using Crystal.Unity;
using System.Windows;
using Crystal.Ioc;
using C02Regions.Views;

namespace C02Regions
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
