using Crystal;
using Crystal.Unity;
using ModuleA;
using RegionContext.Views;
using System.Windows;

namespace RegionContext
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			moduleCatalog.AddModule<ModuleAModule>();
		}
	}
}
