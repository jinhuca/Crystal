using Crystal.Ioc;
using Crystal.Modularity;
using Crystal.Unity;
using ModuleA;
using RegionContext.Views;
using System.Windows;

namespace RegionContext
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

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			moduleCatalog.AddModule<ModuleAModule>();
		}
	}
}
