using System.Windows;
using BasicRegionNavigation.Views;
using Crystal.Ioc;
using Crystal.Modularity;
using Crystal.Unity;

namespace BasicRegionNavigation
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			moduleCatalog.AddModule<ModuleA.ModuleAModule>();
		}
	}
}
